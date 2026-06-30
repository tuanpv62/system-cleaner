// File: Services/TrayIconService.cs - Quản lý icon khay hệ thống (system tray) hiển thị trạng thái Auto Schedule

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SystemCleaner.Services
{
    // ═══════════════════════════════════════════════════════
    //  DATA MODELS
    // ═══════════════════════════════════════════════════════

    /// <summary>Trạng thái hiển thị hiện tại của tray icon.</summary>
    public enum TrayState
    {
        /// <summary>Đang chờ tới chu kỳ chạy tiếp theo — hiển thị chấm xanh dương.</summary>
        Waiting,

        /// <summary>Đang thực thi tác vụ dọn dẹp — hiển thị chấm cam.</summary>
        Running,
    }

    // ═══════════════════════════════════════════════════════
    //  INTERFACE
    // ═══════════════════════════════════════════════════════

    /// <summary>Interface cho dịch vụ icon khay hệ thống (system tray).</summary>
    public interface ITrayIconService : IDisposable
    {
        /// <summary>Người dùng chọn "Mở ứng dụng" từ menu tray.</summary>
        event EventHandler? OpenRequested;

        /// <summary>Người dùng chọn "Chạy Clean ngay bây giờ" từ menu tray.</summary>
        event EventHandler? RunNowRequested;

        /// <summary>
        /// Người dùng đã xác nhận "Yes" ở hộp thoại thoát chương trình
        /// (hộp thoại được hiển thị nội bộ bởi chính service này khi chọn "Thoát").
        /// </summary>
        event EventHandler? ExitConfirmed;

        /// <summary>Hiện icon trên khay hệ thống.</summary>
        void Show();

        /// <summary>Ẩn icon khỏi khay hệ thống.</summary>
        void Hide();

        /// <summary>Đổi màu chấm trạng thái (xanh dương = đang chờ, cam = đang chạy).</summary>
        void SetState(TrayState state);

        /// <summary>
        /// Hiện thông báo dạng balloon (kiểu Windows/PowerShell notification) ở góc
        /// dưới phải màn hình. Lưu ý: Windows 10/11 tự quyết định thời lượng hiển
        /// thị thực tế theo cấu hình Action Center — tham số timeout chỉ là yêu cầu.
        /// </summary>
        void ShowNotification(string title, string message);
    }

    // ═══════════════════════════════════════════════════════
    //  IMPLEMENTATION
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Triển khai dịch vụ tray icon dùng <see cref="NotifyIcon"/> kết hợp icon
    /// hình tròn được vẽ động bằng GDI+ (không cần file .ico rời kèm theo).
    /// </summary>
    public sealed class TrayIconService : ITrayIconService
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>Thời lượng yêu cầu hiển thị balloon (ms) — xem ghi chú ở ShowNotification.</summary>
        private const int BalloonTimeoutMs = 2000;

        private readonly NotifyIcon       _notifyIcon;
        private readonly ContextMenuStrip _menu;
        private readonly Icon             _waitingIcon;
        private readonly Icon             _runningIcon;
        private bool _disposed;

        /// <inheritdoc/>
        public event EventHandler? OpenRequested;

        /// <inheritdoc/>
        public event EventHandler? RunNowRequested;

        /// <inheritdoc/>
        public event EventHandler? ExitConfirmed;

        /// <summary>Khởi tạo tray icon, menu, và 2 icon trạng thái (mặc định chưa hiển thị).</summary>
        public TrayIconService()
        {
            _waitingIcon = CreateDotIcon(Color.DodgerBlue);
            _runningIcon = CreateDotIcon(Color.DarkOrange);
            _menu        = BuildContextMenu();

            _notifyIcon = new NotifyIcon
            {
                Icon    = _waitingIcon,
                Text    = "SystemCleaner – Auto Schedule (đang chờ)",
                Visible = false,
                // Không gán ContextMenuStrip trực tiếp vào NotifyIcon: hành vi mặc
                // định của WinForms chỉ tự mở menu khi RIGHT-click. Yêu cầu ở đây là
                // LEFT-click, nên ta tự xử lý thủ công qua MouseClick bên dưới.
            };

            _notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }

        // ── Context menu ────────────────────────────────────────────────────

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            var miOpen = new ToolStripMenuItem("Mở ứng dụng");
            var miRun  = new ToolStripMenuItem("Chạy Clean ngay bây giờ");
            var miExit = new ToolStripMenuItem("Thoát");

            miOpen.Click += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);
            miRun.Click  += (_, _) => RunNowRequested?.Invoke(this, EventArgs.Empty);
            miExit.Click += (_, _) => HandleExitClick();

            menu.Items.Add(miOpen);
            menu.Items.Add(miRun);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(miExit);

            return menu;
        }

        private void NotifyIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            // Chỉ hiện menu khi click CHUỘT TRÁI, đúng theo yêu cầu.
            if (e.Button == MouseButtons.Left)
            {
                _menu.Show(Cursor.Position);
            }
        }

        private void HandleExitClick()
        {
            var result = MessageBox.Show(
                "Bạn có muốn thoát chương trình?",
                "Xác nhận thoát",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ExitConfirmed?.Invoke(this, EventArgs.Empty);
            }
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <inheritdoc/>
        public void Show() => _notifyIcon.Visible = true;

        /// <inheritdoc/>
        public void Hide() => _notifyIcon.Visible = false;

        /// <inheritdoc/>
        public void SetState(TrayState state)
        {
            if (state == TrayState.Running)
            {
                _notifyIcon.Icon = _runningIcon;
                _notifyIcon.Text = "SystemCleaner – Đang chạy dọn dẹp...";
            }
            else
            {
                _notifyIcon.Icon = _waitingIcon;
                _notifyIcon.Text = "SystemCleaner – Auto Schedule (đang chờ)";
            }
        }

        /// <inheritdoc/>
        public void ShowNotification(string title, string message)
        {
            // Balloon chỉ hiện được khi icon đang Visible trên khay hệ thống.
            if (!_notifyIcon.Visible) return;
            _notifyIcon.ShowBalloonTip(BalloonTimeoutMs, title, message, ToolTipIcon.Info);
        }

        // ── Icon generation (GDI+) ──────────────────────────────────────────

        /// <summary>Vẽ một icon hình tròn đặc màu <paramref name="color"/> bằng GDI+.</summary>
        private static Icon CreateDotIcon(Color color)
        {
            using var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using var brush = new SolidBrush(color);
                g.FillEllipse(brush, 3, 3, 26, 26);

                using var pen = new Pen(Color.White, 2f);
                g.DrawEllipse(pen, 3, 3, 26, 26);
            }

            // Lưu ý: Icon.FromHandle KHÔNG tự giải phóng HICON khi Dispose —
            // phải gọi DestroyIcon thủ công (xem Dispose() bên dưới).
            IntPtr hIcon = bmp.GetHicon();
            return Icon.FromHandle(hIcon);
        }

        // ── Cleanup ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _notifyIcon.Visible     =  false;
            _notifyIcon.MouseClick -=  NotifyIcon_MouseClick;
            _notifyIcon.Dispose();
            _menu.Dispose();

            // Giải phóng 2 HICON đã tạo thủ công bằng GetHicon()
            DestroyIcon(_waitingIcon.Handle);
            DestroyIcon(_runningIcon.Handle);
            _waitingIcon.Dispose();
            _runningIcon.Dispose();
        }
    }
}
