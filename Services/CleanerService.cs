// File: Services/CleanerService.cs - Dịch vụ dọn dẹp hệ thống (Temp, Recycle Bin, Prefetch)

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SystemCleaner.Services
{
    // ═══════════════════════════════════════════════════════
    //  DATA MODELS
    // ═══════════════════════════════════════════════════════

    /// <summary>Tùy chọn cho tác vụ dọn dẹp hệ thống.</summary>
    public sealed class CleanOptions
    {
        /// <summary>Dọn C:\Windows\Temp</summary>
        public bool CleanTempWindows { get; set; }

        /// <summary>Làm trống Recycle Bin (SHEmptyRecycleBin API)</summary>
        public bool CleanRecycleBin { get; set; }

        /// <summary>Dọn C:\Windows\Prefetch (chỉ file *.pf)</summary>
        public bool CleanPrefetch { get; set; }

        /// <summary>Dọn %TEMP% của user hiện tại</summary>
        public bool CleanUserTemp { get; set; }

        /// <summary>Kiểm tra có ít nhất một task được chọn không.</summary>
        public bool HasAnyTask =>
            CleanTempWindows || CleanRecycleBin || CleanPrefetch || CleanUserTemp;
    }

    /// <summary>Kết quả trả về sau khi dọn dẹp.</summary>
    public sealed class CleanResult
    {
        /// <summary>Tổng số file đã xóa thành công.</summary>
        public int FilesDeleted { get; set; }

        /// <summary>Tổng số file bị bỏ qua (đang dùng / không có quyền).</summary>
        public int FilesSkipped { get; set; }

        /// <summary>Danh sách thông báo lỗi (không gây crash).</summary>
        public List<string> Errors { get; } = [];
    }

    // ═══════════════════════════════════════════════════════
    //  INTERFACE
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Interface cho dịch vụ dọn dẹp hệ thống.
    /// logCallback nhận (level, message) — ví dụ: ("INFO", "Deleted: C:\...")
    /// </summary>
    public interface ICleanerService
    {
        Task<CleanResult> CleanAsync(CleanOptions options, Action<string, string> logCallback);
    }

    // ═══════════════════════════════════════════════════════
    //  IMPLEMENTATION
    // ═══════════════════════════════════════════════════════

    /// <summary>Triển khai dịch vụ dọn dẹp hệ thống.</summary>
    public sealed class CleanerService : ICleanerService
    {
        // ── P/Invoke: SHEmptyRecycleBin ──────────────────────────────────────
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern uint SHEmptyRecycleBin(
            IntPtr hwnd,
            string? pszRootPath,
            uint   dwFlags);

        /// <summary>
        /// Flags cho SHEmptyRecycleBin:
        /// SHERB_NOCONFIRMATION (0x01) | SHERB_NOPROGRESSUI (0x02) | SHERB_NOSOUND (0x04)
        /// </summary>
        private const uint SHERB_SILENT = 0x00000007;

        // ────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<CleanResult> CleanAsync(CleanOptions options, Action<string, string> logCallback)
        {
            var result = new CleanResult();

            // Chạy toàn bộ logic trên thread pool để không block UI
            await Task.Run(() =>
            {
                try
                {
                    if (options.CleanTempWindows)
                        ExecuteClean(@"C:\Windows\Temp", null, "Windows\\Temp", result, logCallback);

                    if (options.CleanRecycleBin)
                        EmptyRecycleBin(result, logCallback);

                    if (options.CleanPrefetch)
                        ExecuteClean(@"C:\Windows\Prefetch", "*.pf", "Windows\\Prefetch", result, logCallback);

                    if (options.CleanUserTemp)
                    {
                        string userTemp = Path.GetTempPath();
                        ExecuteClean(userTemp, null, "%TEMP%", result, logCallback);
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(ex.Message);
                    logCallback("ERROR", $"Lỗi tổng quát trong CleanAsync: {ex.Message}");
                }
            });

            return result;
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>Dọn sạch nội dung trong <paramref name="folderPath"/>, KHÔNG xóa folder gốc.</summary>
        private static void ExecuteClean(
            string folderPath,
            string? filePattern,
            string displayName,
            CleanResult result,
            Action<string, string> log)
        {
            log("INFO", $"Bắt đầu dọn [{displayName}]: {folderPath}");

            if (!Directory.Exists(folderPath))
            {
                log("WARNING", $"Thư mục không tồn tại: {folderPath}");
                return;
            }

            int localDeleted = 0;
            int localSkipped = 0;

            // ── Xóa files ──
            string pattern = filePattern ?? "*";
            try
            {
                foreach (string filePath in Directory.EnumerateFiles(folderPath, pattern, SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Delete(filePath);
                        localDeleted++;
                        result.FilesDeleted++;
                        log("INFO", $"Deleted: {filePath}");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        localSkipped++;
                        result.FilesSkipped++;
                        log("WARNING", $"Skipped (access denied): {filePath}");
                    }
                    catch (IOException)
                    {
                        localSkipped++;
                        result.FilesSkipped++;
                        log("WARNING", $"Skipped (in use): {filePath}");
                    }
                    catch (Exception ex)
                    {
                        localSkipped++;
                        result.FilesSkipped++;
                        result.Errors.Add(ex.Message);
                        log("WARNING", $"Skipped (error): {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                log("ERROR", $"Lỗi khi liệt kê files trong {folderPath}: {ex.Message}");
            }

            // ── Xóa đệ quy subfolders (chỉ khi không filter theo pattern) ──
            if (filePattern == null)
            {
                try
                {
                    foreach (string subDir in Directory.EnumerateDirectories(folderPath))
                    {
                        try
                        {
                            Directory.Delete(subDir, recursive: true);
                            log("INFO", $"Deleted dir: {subDir}");
                        }
                        catch (UnauthorizedAccessException)
                        {
                            log("WARNING", $"Skipped dir (access denied): {subDir}");
                        }
                        catch (IOException)
                        {
                            log("WARNING", $"Skipped dir (in use): {subDir}");
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(ex.Message);
                            log("WARNING", $"Skipped dir (error): {subDir}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log("ERROR", $"Lỗi khi liệt kê subdirectories: {ex.Message}");
                }
            }

            log("SUCCESS", $"Cleaned [{displayName}]: {localDeleted} file xóa, {localSkipped} file bỏ qua.");
        }

        /// <summary>Làm trống Recycle Bin qua SHEmptyRecycleBin API.</summary>
        private static void EmptyRecycleBin(CleanResult result, Action<string, string> log)
        {
            log("INFO", "Đang làm trống Recycle Bin...");
            try
            {
                // null = tất cả drives; SHERB_SILENT = không hiện dialog, không phát âm thanh
                uint hr = SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_SILENT);

                // 0x800704C7 = ERROR_CANCELLED (Recycle Bin rỗng) → bỏ qua
                if (hr == 0 || hr == 0x800704C7)
                    log("SUCCESS", "Recycle Bin đã được làm trống.");
                else
                    log("WARNING", $"SHEmptyRecycleBin trả về HRESULT: 0x{hr:X8}");
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                log("ERROR", $"Không thể làm trống Recycle Bin: {ex.Message}");
            }
        }
    }
}
