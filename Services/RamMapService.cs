// File: Services/RamMapService.cs - Dịch vụ tích hợp công cụ RAMMap của Sysinternals

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SystemCleaner.Services
{
    // ═══════════════════════════════════════════════════════
    //  DATA MODELS
    // ═══════════════════════════════════════════════════════

    /// <summary>Tùy chọn cho các lệnh RamMap cần thực thi.</summary>
    public sealed class RamMapOptions
    {
        /// <summary>Empty Working Sets (-Ew)</summary>
        public bool EmptyWorkingSets { get; set; }

        /// <summary>Empty System Working Set (-Es)</summary>
        public bool EmptySystemWorkingSet { get; set; }

        /// <summary>Empty Modified Page List (-Em)</summary>
        public bool EmptyModifiedPageList { get; set; }

        /// <summary>Empty Standby List (-Et)</summary>
        public bool EmptyStandbyList { get; set; }

        /// <summary>Empty Priority 0 Standby (-E0)</summary>
        public bool EmptyPriority0Standby { get; set; }

        /// <summary>Kiểm tra có ít nhất một lệnh được chọn không.</summary>
        public bool HasAnyTask =>
            EmptyWorkingSets || EmptySystemWorkingSet ||
            EmptyModifiedPageList || EmptyStandbyList || EmptyPriority0Standby;
    }

    // ═══════════════════════════════════════════════════════
    //  INTERFACE
    // ═══════════════════════════════════════════════════════

    /// <summary>Interface cho dịch vụ RAMMap.</summary>
    public interface IRamMapService
    {
        /// <summary>
        /// Chạy tuần tự các lệnh RamMap được chọn trong <paramref name="options"/>.
        /// </summary>
        /// <exception cref="FileNotFoundException">
        /// Ném ra khi RamMap.exe không tồn tại tại <paramref name="ramMapPath"/>.
        /// </exception>
        Task RunAsync(RamMapOptions options, string ramMapPath, Action<string, string> logCallback);

        /// <summary>
        /// Tự động tải RAMMap (chính chủ Microsoft Sysinternals), giải nén và
        /// lưu vào <paramref name="targetPath"/>. Tự chọn bản 32-bit/64-bit phù hợp với OS.
        /// </summary>
        /// <returns>true nếu tải và cài đặt thành công.</returns>
        Task<bool> TryAutoDownloadAsync(string targetPath, Action<string, string> logCallback);
    }

    // ═══════════════════════════════════════════════════════
    //  IMPLEMENTATION
    // ═══════════════════════════════════════════════════════

    /// <summary>Triển khai dịch vụ RAMMap.</summary>
    public sealed class RamMapService : IRamMapService
    {
        /// <summary>Timeout mỗi lệnh RamMap: 30 giây.</summary>
        private const int ProcessTimeoutMs = 30_000;

        /// <summary>URL tải chính thức từ Microsoft Sysinternals.</summary>
        private const string DownloadUrl = "https://download.sysinternals.com/files/RAMMap.zip";

        /// <summary>
        /// HttpClient dùng chung (static) theo khuyến nghị của Microsoft để tránh
        /// socket exhaustion khi tạo nhiều instance HttpClient riêng lẻ.
        /// </summary>
        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

        /// <summary>Mapping từ option → (argument CLI, tên hiển thị).</summary>
        private static readonly List<(Func<RamMapOptions, bool> Enabled, string Arg, string Name)> ArgMap =
        [
            (o => o.EmptyWorkingSets,       "-Ew", "Empty Working Sets"),
            (o => o.EmptySystemWorkingSet,  "-Es", "Empty System Working Set"),
            (o => o.EmptyModifiedPageList,  "-Em", "Empty Modified Page List"),
            (o => o.EmptyStandbyList,       "-Et", "Empty Standby List"),
            (o => o.EmptyPriority0Standby,  "-E0", "Empty Priority 0 Standby"),
        ];

        /// <inheritdoc/>
        public async Task RunAsync(RamMapOptions options, string ramMapPath, Action<string, string> logCallback)
        {
            // ── Bước 1: kiểm tra file tồn tại ──────────────────────────────
            if (string.IsNullOrWhiteSpace(ramMapPath) || !File.Exists(ramMapPath))
            {
                throw new FileNotFoundException(
                    "RAMMap not found. Please download from: " +
                    "https://learn.microsoft.com/sysinternals/downloads/rammap",
                    ramMapPath);
            }

            // ── Bước 2: chạy tuần tự từng argument trên thread pool ─────────
            await Task.Run(() =>
            {
                foreach (var (enabled, arg, name) in ArgMap)
                {
                    if (!enabled(options)) continue;

                    logCallback("INFO", $"Running RamMap {arg} ({name})...");

                    try
                    {
                        RunSingleArg(ramMapPath, arg, logCallback);
                        logCallback("SUCCESS", $"Done: {arg} ({name})");
                    }
                    catch (Exception ex)
                    {
                        logCallback("ERROR", $"RamMap error [{arg}]: {ex.Message}");
                    }
                }
            });
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>Khởi động RamMap.exe với argument đã cho và chờ kết thúc (timeout 30s).</summary>
        private static void RunSingleArg(string exePath, string arg, Action<string, string> log)
        {
            var psi = new ProcessStartInfo
            {
                FileName        = exePath,
                // /accepteula: tự động chấp nhận license, tránh treo process do dialog
                // EULA ẩn (CreateNoWindow=true) chờ click Accept ở lần chạy đầu tiên
                // trên máy mới — nếu thiếu cờ này, process sẽ "treo" tới khi timeout.
                Arguments       = $"{arg} /accepteula",
                UseShellExecute = false,
                CreateNoWindow  = true,
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException($"Không thể khởi chạy RamMap với arg {arg}");

            bool finished = process.WaitForExit(ProcessTimeoutMs);

            if (!finished)
            {
                try { process.Kill(); } catch { /* ignore kill errors */ }
                log("WARNING", $"RamMap timed out for argument: {arg} (killed after 30s)");
            }
        }

        /// <inheritdoc/>
        public async Task<bool> TryAutoDownloadAsync(string targetPath, Action<string, string> logCallback)
        {
            // Thư mục tạm riêng cho lần tải này (tránh đụng độ nếu user bấm 2 lần)
            string tempDir = Path.Combine(Path.GetTempPath(), "SystemCleaner_RamMap_" + Guid.NewGuid().ToString("N"));
            string zipPath = Path.Combine(tempDir, "RAMMap.zip");

            try
            {
                Directory.CreateDirectory(tempDir);

                // ── Bước 1: Tải zip từ Microsoft Sysinternals ──────────────────
                logCallback("INFO", $"Đang tải RAMMap từ: {DownloadUrl}");

                using (var response = await _httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    await using var fileStream = File.Create(zipPath);
                    await response.Content.CopyToAsync(fileStream);
                }

                long sizeKb = new FileInfo(zipPath).Length / 1024;
                logCallback("SUCCESS", $"Đã tải xong RAMMap.zip ({sizeKb} KB).");

                // ── Bước 2: Giải nén ─────────────────────────────────────────────
                logCallback("INFO", "Đang giải nén...");
                string extractDir = Path.Combine(tempDir, "extracted");
                ZipFile.ExtractToDirectory(zipPath, extractDir);

                // ── Bước 3: Chọn đúng bản 32-bit / 64-bit theo hệ điều hành ──────
                // Zip chứa cả RAMMap.exe (32-bit) và RAMMap64.exe (64-bit).
                string preferredName = Environment.Is64BitOperatingSystem ? "RAMMap64.exe" : "RAMMap.exe";

                string? sourceExe =
                    Directory.EnumerateFiles(extractDir, preferredName, SearchOption.AllDirectories).FirstOrDefault()
                    ?? Directory.EnumerateFiles(extractDir, "RAMMap.exe", SearchOption.AllDirectories).FirstOrDefault();

                if (sourceExe == null)
                {
                    logCallback("ERROR", "Không tìm thấy file .exe hợp lệ bên trong RAMMap.zip vừa tải.");
                    return false;
                }

                // ── Bước 4: Copy vào đúng path đã cấu hình (giữ nguyên tên file) ─
                string? targetDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDir))
                    Directory.CreateDirectory(targetDir);

                File.Copy(sourceExe, targetPath, overwrite: true);

                logCallback("SUCCESS", $"Đã cài đặt RAMMap tại: {targetPath}");
                return true;
            }
            catch (Exception ex)
            {
                logCallback("ERROR", $"Tải RAMMap tự động thất bại: {ex.Message}");
                logCallback("INFO", "Vui lòng tải thủ công tại: https://learn.microsoft.com/sysinternals/downloads/rammap");
                return false;
            }
            finally
            {
                // Dọn dẹp thư mục tạm dù thành công hay thất bại
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, recursive: true);
                }
                catch { /* ignore cleanup errors */ }
            }
        }
    }
}
