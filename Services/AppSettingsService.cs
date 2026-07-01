// File: Services/AppSettingsService.cs
// Quản lý toàn bộ cài đặt ứng dụng:
//   - Lưu/đọc từ app.config (ConfigurationManager)
//   - Registry HKCU\Run để "Start with Windows"

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace SystemCleaner.Services
{
    // ═══════════════════════════════════════════════════════
    //  MODEL
    // ═══════════════════════════════════════════════════════

    /// <summary>Toàn bộ cài đặt người dùng của ứng dụng.</summary>
    public sealed class AppSettingsModel
    {
        /// <summary>Tự khởi động cùng Windows (ghi vào Registry Run).</summary>
        public bool StartWithWindows { get; set; } = false;

        /// <summary>Ẩn xuống tray thay vì thoát khi đóng cửa sổ.</summary>
        public bool MinimizeToTray { get; set; } = true;

        /// <summary>Hiện thông báo balloon trên tray.</summary>
        public bool ShowNotifications { get; set; } = true;

        /// <summary>Tự cuộn Activity Log xuống dòng cuối sau mỗi dòng mới.</summary>
        public bool AutoScrollLog { get; set; } = true;

        /// <summary>Giá trị mặc định (phút) cho bộ đếm Auto Schedule.</summary>
        public int DefaultInterval { get; set; } = 5;
    }

    // ═══════════════════════════════════════════════════════
    //  INTERFACE
    // ═══════════════════════════════════════════════════════

    public interface IAppSettingsService
    {
        /// <summary>Bản sao cài đặt hiện tại đã được tải vào bộ nhớ.</summary>
        AppSettingsModel Current { get; }

        /// <summary>Đọc toàn bộ cài đặt từ app.config và Registry.</summary>
        void Load();

        /// <summary>Lưu <paramref name="model"/> vào app.config và Registry.</summary>
        void Save(AppSettingsModel model);
    }

    // ═══════════════════════════════════════════════════════
    //  IMPLEMENTATION
    // ═══════════════════════════════════════════════════════

    public sealed class AppSettingsService : IAppSettingsService
    {
        // ── Keys trong app.config ────────────────────────────────────────────
        private const string KeyMinimizeToTray    = "Setting_MinimizeToTray";
        private const string KeyShowNotifications = "Setting_ShowNotifications";
        private const string KeyAutoScrollLog     = "Setting_AutoScrollLog";
        private const string KeyDefaultInterval   = "Setting_DefaultInterval";

        // ── Registry ─────────────────────────────────────────────────────────
        private const string RegRunPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string RegAppName = "SystemCleaner";

        /// <inheritdoc/>
        public AppSettingsModel Current { get; private set; } = new();

        /// <summary>Khởi tạo và load cài đặt ngay lập tức.</summary>
        public AppSettingsService() => Load();

        // ─────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public void Load()
        {
            Current = new AppSettingsModel
            {
                StartWithWindows  = IsRegistryStartupSet(),
                MinimizeToTray    = ReadBool(KeyMinimizeToTray,    defaultValue: true),
                ShowNotifications = ReadBool(KeyShowNotifications,  defaultValue: true),
                AutoScrollLog     = ReadBool(KeyAutoScrollLog,      defaultValue: true),
                DefaultInterval   = ReadInt (KeyDefaultInterval,    defaultValue: 5, min: 1, max: 1440),
            };
        }

        /// <inheritdoc/>
        public void Save(AppSettingsModel model)
        {
            Current = model;

            // ── Lưu vào app.config ───────────────────────────────────────────
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            SetOrAdd(config, KeyMinimizeToTray,    model.MinimizeToTray.ToString());
            SetOrAdd(config, KeyShowNotifications, model.ShowNotifications.ToString());
            SetOrAdd(config, KeyAutoScrollLog,     model.AutoScrollLog.ToString());
            SetOrAdd(config, KeyDefaultInterval,   model.DefaultInterval.ToString());
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            // ── Startup Registry ─────────────────────────────────────────────
            if (model.StartWithWindows)
                SetRegistryStartup();
            else
                RemoveRegistryStartup();
        }

        // ── Helpers: app.config ──────────────────────────────────────────────

        private static bool ReadBool(string key, bool defaultValue)
        {
            var raw = ConfigurationManager.AppSettings[key];
            return raw is null ? defaultValue : bool.TryParse(raw, out var v) ? v : defaultValue;
        }

        private static int ReadInt(string key, int defaultValue, int min, int max)
        {
            var raw = ConfigurationManager.AppSettings[key];
            if (raw is null || !int.TryParse(raw, out var v)) return defaultValue;
            return Math.Clamp(v, min, max);
        }

        private static void SetOrAdd(Configuration config, string key, string value)
        {
            var settings = config.AppSettings.Settings;
            if (settings[key] != null)
                settings[key].Value = value;
            else
                settings.Add(key, value);
        }

        // ── Helpers: Registry ────────────────────────────────────────────────

        private static bool IsRegistryStartupSet()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegRunPath, writable: false);
                return key?.GetValue(RegAppName) is not null;
            }
            catch { return false; }
        }

        private static void SetRegistryStartup()
        {
            try
            {
                // Lấy đường dẫn exe đang chạy (publish single-file lấy MainModule.FileName)
                string exePath = Process.GetCurrentProcess().MainModule?.FileName
                                 ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SystemCleaner.exe");

                using var key = Registry.CurrentUser.OpenSubKey(RegRunPath, writable: true);
                key?.SetValue(RegAppName, $"\"{exePath}\"");
            }
            catch { /* Quyền ghi Registry có thể bị chặn bởi group policy */ }
        }

        private static void RemoveRegistryStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegRunPath, writable: true);
                key?.DeleteValue(RegAppName, throwOnMissingValue: false);
            }
            catch { }
        }
    }
}
