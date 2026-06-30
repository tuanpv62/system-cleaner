// File: Forms/MainForm.cs - Logic chính của form (event handlers, logging, job runner)

using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SystemCleaner.Services;

namespace SystemCleaner.Forms
{
    /// <summary>
    /// Form chính của ứng dụng SystemCleaner.
    /// Sử dụng WinForms thuần (.NET 8), không cần DevExpress.
    /// Nhận services qua constructor (manual dependency injection).
    /// </summary>
    public partial class MainForm : Form
    {
        // ═══════════════════════════════════════════════════════
        //  FIELDS
        // ═══════════════════════════════════════════════════════

        private readonly ICleanerService  _cleanerService;
        private readonly IRamMapService   _ramMapService;
        private readonly SchedulerService _schedulerService;
        private readonly ITrayIconService _trayIconService;

        /// <summary>Cờ ngăn concurrent run (job chồng job).</summary>
        private volatile bool _isJobRunning = false;

        /// <summary>true khi thoát được xác nhận thật sự từ tray menu (bypass minimize-to-tray).</summary>
        private bool _isExiting = false;

        private const int MAX_LOG_LINES = 500;
        private const int TRIM_LINES    = 100;

        /// <summary>Path mặc định nếu app.config chưa có giá trị RamMapPath.</summary>
        private const string DefaultRamMapPath = @"C:\Program Files\SysinternalsSuite\RamMap.exe";

        // ═══════════════════════════════════════════════════════
        //  CONSTRUCTOR
        // ═══════════════════════════════════════════════════════

        /// <summary>Khởi tạo MainForm với dependency injection thủ công.</summary>
        public MainForm(
            ICleanerService  cleanerService,
            IRamMapService   ramMapService,
            SchedulerService schedulerService,
            ITrayIconService trayIconService)
        {
            _cleanerService   = cleanerService;
            _ramMapService    = ramMapService;
            _schedulerService = schedulerService;
            _trayIconService  = trayIconService;

            InitializeComponent();
            LoadSettings();
            SetInitialState();
            WireTrayIconEvents();

            // Đăng ký Resize ở đây (thay vì Designer.cs) để giữ Designer.cs thuần layout
            Resize += MainForm_Resize;
        }

        /// <summary>Gắn các sự kiện phát ra từ tray icon vào hành động tương ứng trên form.</summary>
        private void WireTrayIconEvents()
        {
            _trayIconService.OpenRequested += (_, _) => RestoreFromTray();
            _trayIconService.ExitConfirmed += (_, _) => ExitApplicationFromTray();

            _trayIconService.RunNowRequested += async (_, _) =>
            {
                _trayIconService.SetState(TrayState.Running);
                try
                {
                    await ExecuteRunNowAsync();
                }
                finally
                {
                    // Chỉ quay lại "đang chờ" nếu scheduler vẫn còn chạy (tray vẫn hiển thị)
                    if (_schedulerService.IsRunning)
                        _trayIconService.SetState(TrayState.Waiting);
                }
            };
        }

        // ═══════════════════════════════════════════════════════
        //  FORM EVENTS
        // ═══════════════════════════════════════════════════════

        /// <summary>Chạy khi form hiển thị lần đầu.</summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            Log("INFO", "══════════════════════════════════════════════");
            Log("INFO", "  SystemCleaner khởi động  (quyền Administrator)");
            Log("INFO", "══════════════════════════════════════════════");
            Log("INFO", $"RamMap path: {txtRamMapPath.Text}");

            if (!string.IsNullOrEmpty(txtRamMapPath.Text) && !File.Exists(txtRamMapPath.Text))
            {
                Log("WARNING", "RamMap.exe không tìm thấy tại path trên.");
                Log("WARNING", "Tải xuống: https://learn.microsoft.com/sysinternals/downloads/rammap");
            }
        }

        /// <summary>Dọn dẹp khi form đóng — hoặc ẩn xuống tray nếu Auto Schedule đang chạy.</summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Nếu Auto Schedule đang chạy và người dùng bấm nút X (không phải thoát
            // đã xác nhận từ tray) → ẩn xuống tray, KHÔNG đóng app thật sự, để vẫn
            // chạy ngầm theo đúng lịch đã đặt. CloseReason.UserClosing loại trừ các
            // trường hợp Windows tắt máy/đăng xuất (khi đó vẫn phải đóng bình thường).
            if (!_isExiting && _schedulerService.IsRunning && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel      = true;
                ShowInTaskbar = false;
                Hide();

                _trayIconService.ShowNotification(
                    "SystemCleaner",
                    "Ứng dụng vẫn chạy ngầm. Click vào icon khay hệ thống để mở lại.");
                return;
            }

            _schedulerService.Stop();
            _schedulerService.Dispose();
            _trayIconService.Hide();
            _trayIconService.Dispose();
        }

        /// <summary>Minimize-to-tray: khi thu nhỏ form lúc Auto Schedule đang chạy, ẩn khỏi taskbar.</summary>
        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && _schedulerService.IsRunning)
            {
                Hide();
                ShowInTaskbar = false;
            }
        }

        /// <summary>Khôi phục form từ tray về trạng thái hiển thị bình thường (gọi từ menu "Mở ứng dụng").</summary>
        private void RestoreFromTray()
        {
            ShowInTaskbar = true;
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        /// <summary>Thoát chương trình thật sự — gọi từ tray sau khi người dùng đã xác nhận Yes.</summary>
        private void ExitApplicationFromTray()
        {
            _isExiting = true;
            Close();
        }

        // ═══════════════════════════════════════════════════════
        //  SETTINGS
        // ═══════════════════════════════════════════════════════

        /// <summary>Đọc cài đặt từ app.config và áp dụng lên UI.</summary>
        private void LoadSettings()
        {
            string path        = ConfigurationManager.AppSettings["RamMapPath"] ?? DefaultRamMapPath;
            txtRamMapPath.Text = path;
        }

        /// <summary>Lưu đường dẫn RamMap vào app.config ngay lập tức.</summary>
        private void SaveRamMapPath(string path)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (config.AppSettings.Settings["RamMapPath"] != null)
                    config.AppSettings.Settings["RamMapPath"].Value = path;
                else
                    config.AppSettings.Settings.Add("RamMapPath", path);

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                Log("WARNING", $"Không thể lưu cài đặt vào app.config: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════
        //  UI STATE HELPERS
        // ═══════════════════════════════════════════════════════

        /// <summary>Đặt trạng thái ban đầu cho các control.</summary>
        private void SetInitialState()
        {
            chkTempWindows.Checked = true;
            chkRecycleBin.Checked  = true;
            btnStopAuto.Enabled    = false;
        }

        // ═══════════════════════════════════════════════════════
        //  LOGGING
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Ghi một dòng log vào RichTextBox với timestamp và level.
        /// Thread-safe: tự động BeginInvoke về UI thread nếu cần.
        /// </summary>
        private void Log(string level, string message)
        {
            if (memoLog.InvokeRequired)
            {
                memoLog.BeginInvoke(() => Log(level, message));
                return;
            }

            string timestamp  = DateTime.Now.ToString("HH:mm:ss");
            string bracketLvl = ("[" + level + "]").PadRight(9); // căn đều 9 ký tự
            string line       = $"[{timestamp}] {bracketLvl} {message}";

            // ── Giới hạn 500 dòng: xóa 100 dòng đầu khi vượt ──────────────
            if (memoLog.Lines.Length >= MAX_LOG_LINES)
            {
                var keepLines = memoLog.Lines.Skip(TRIM_LINES).ToArray();
                memoLog.Text  = string.Join(Environment.NewLine, keepLines);
            }

            // AppendText tự động scroll xuống cuối trong RichTextBox
            memoLog.AppendText((memoLog.TextLength > 0 ? Environment.NewLine : "") + line);
            memoLog.ScrollToCaret();
        }

        // ═══════════════════════════════════════════════════════
        //  OPTIONS BUILDERS
        // ═══════════════════════════════════════════════════════

        private CleanOptions GetCleanOptions() => new()
        {
            CleanTempWindows = chkTempWindows.Checked,
            CleanRecycleBin  = chkRecycleBin.Checked,
            CleanPrefetch    = chkPrefetch.Checked,
            CleanUserTemp    = chkUserTemp.Checked,
        };

        private RamMapOptions GetRamMapOptions() => new()
        {
            EmptyWorkingSets      = chkWorkingSets.Checked,
            EmptySystemWorkingSet = chkSystemWorkingSet.Checked,
            EmptyModifiedPageList = chkModifiedPageList.Checked,
            EmptyStandbyList      = chkStandbyList.Checked,
            EmptyPriority0Standby = chkPriority0Standby.Checked,
        };

        // ═══════════════════════════════════════════════════════
        //  CORE JOB RUNNER
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Thực thi toàn bộ tác vụ dọn dẹp + RamMap theo options đã chọn.
        /// Gọi từ cả Run Now và Scheduled run.
        /// </summary>
        private async Task RunJobInternal()
        {
            var cleanOpts = GetCleanOptions();
            var ramOpts   = GetRamMapOptions();

            // ── Kiểm tra có ít nhất 1 task ────────────────────────────────
            if (!cleanOpts.HasAnyTask && !ramOpts.HasAnyTask)
            {
                MessageBox.Show(
                    this,
                    "Please select at least one task to run.",
                    "Chưa chọn tác vụ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Action<string, string> logger = (lvl, msg) => Log(lvl, msg);

            Log("INFO", "────────────────── Bắt đầu thực hiện ──────────────────");

            try
            {
                // ── Phần 1: System Cleanup ─────────────────────────────────
                if (cleanOpts.HasAnyTask)
                {
                    Log("INFO", "[ CLEANUP ] Đang dọn dẹp hệ thống...");
                    var result = await _cleanerService.CleanAsync(cleanOpts, logger);
                    Log("SUCCESS",
                        $"[ CLEANUP ] Hoàn tất: {result.FilesDeleted} file xóa, " +
                        $"{result.FilesSkipped} file bỏ qua" +
                        (result.Errors.Count > 0 ? $", {result.Errors.Count} lỗi" : ""));
                }

                // ── Phần 2: RamMap ─────────────────────────────────────────
                if (ramOpts.HasAnyTask)
                {
                    string ramPath = string.IsNullOrWhiteSpace(txtRamMapPath.Text)
                        ? DefaultRamMapPath
                        : txtRamMapPath.Text;

                    bool ramMapReady = File.Exists(ramPath);

                    // ── Không tìm thấy RamMap.exe → hỏi tải tự động ──────────
                    if (!ramMapReady)
                    {
                        Log("WARNING", $"RamMap không tìm thấy tại: {ramPath}");

                        var choice = MessageBox.Show(
                            this,
                            $"Không tìm thấy RamMap.exe tại:\n{ramPath}\n\n" +
                            "Bạn có muốn tự động tải về từ Microsoft Sysinternals không? (~700 KB)",
                            "RamMap không tìm thấy",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (choice == DialogResult.Yes)
                        {
                            Log("INFO", "[ RAMMAP  ] Đang tự động tải RamMap...");
                            ramMapReady = await _ramMapService.TryAutoDownloadAsync(ramPath, logger);

                            // Tải xong tại đúng path đang cấu hình → lưu lại để chắc chắn
                            if (ramMapReady)
                                SaveRamMapPath(ramPath);
                        }
                        else
                        {
                            Log("INFO", "Người dùng đã hủy tải. Bỏ qua tác vụ RamMap.");
                        }
                    }

                    // ── Chạy RamMap nếu đã sẵn sàng (có sẵn hoặc vừa tải xong) ─
                    if (ramMapReady)
                    {
                        Log("INFO", "[ RAMMAP  ] Đang chạy RamMap...");
                        await _ramMapService.RunAsync(ramOpts, ramPath, logger);
                        Log("SUCCESS", "[ RAMMAP  ] Hoàn tất tất cả lệnh RamMap.");
                    }
                    else
                    {
                        Log("WARNING", "[ RAMMAP  ] Bỏ qua do RamMap không khả dụng.");
                    }
                }

                Log("SUCCESS", "────────────────── Hoàn tất tất cả tác vụ ──────────────────");
            }
            catch (FileNotFoundException ex)
            {
                Log("ERROR", $"Không tìm thấy RamMap: {ex.Message}");
                MessageBox.Show(this, ex.Message, "RamMap không tìm thấy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Log("ERROR", $"Lỗi không mong đợi [{ex.GetType().Name}]: {ex.Message}");
            }
        }

        // ═══════════════════════════════════════════════════════
        //  BUTTON EVENT HANDLERS
        // ═══════════════════════════════════════════════════════

        /// <summary>Run Now — chạy ngay một lần, disable cả 3 buttons trong khi chạy.</summary>
        private async void btnRunNow_Click(object sender, EventArgs e) => await ExecuteRunNowAsync();

        /// <summary>
        /// Logic dùng chung cho "Run Now": được gọi từ cả button trên form lẫn từ
        /// menu tray ("Chạy Clean ngay bây giờ"), tránh trùng lặp code.
        /// </summary>
        private async Task ExecuteRunNowAsync()
        {
            if (_isJobRunning) return;
            _isJobRunning = true;

            btnRunNow.Enabled    = false;
            btnStartAuto.Enabled = false;
            btnStopAuto.Enabled  = false;

            try
            {
                await RunJobInternal();
            }
            finally
            {
                _isJobRunning = false;
                bool schedulerOn = _schedulerService.IsRunning;
                btnRunNow.Enabled    = true;
                btnStartAuto.Enabled = !schedulerOn;
                btnStopAuto.Enabled  = schedulerOn;
            }
        }

        /// <summary>Start Auto — khởi động timer với interval từ NumericUpDown.</summary>
        private void btnStartAuto_Click(object sender, EventArgs e)
        {
            int minutes = (int)spinInterval.Value;
            minutes = Math.Clamp(minutes, 1, 1440);

            btnStartAuto.Enabled = false;
            btnStopAuto.Enabled  = true;
            btnRunNow.Enabled    = true;

            Log("INFO", $"Auto scheduler bắt đầu. Interval: {minutes} phút.");

            // ── Bật tray icon: từ giờ app có thể chạy ngầm dưới nền Windows ──
            _trayIconService.Show();
            _trayIconService.SetState(TrayState.Waiting);
            _trayIconService.ShowNotification(
                "SystemCleaner",
                $"Auto Schedule đã bật — chạy mỗi {minutes} phút.");

            _schedulerService.Start(minutes, async () =>
            {
                if (_isJobRunning)
                {
                    Log("WARNING", "[Scheduler] Job đang chạy, bỏ qua lần này.");
                    return;
                }
                _isJobRunning = true;

                btnRunNow.Enabled    = false;
                btnStartAuto.Enabled = false;

                // ── Chuyển tray sang "đang chạy" (chấm cam) + thông báo 2s ───
                _trayIconService.SetState(TrayState.Running);
                _trayIconService.ShowNotification(
                    "SystemCleaner",
                    $"Đang chạy dọn dẹp tự động lúc {DateTime.Now:HH:mm:ss}.");

                try
                {
                    Log("INFO", "[Scheduler] ═══ Đang thực hiện scheduled run...");
                    await RunJobInternal();
                }
                finally
                {
                    _isJobRunning        = false;
                    btnRunNow.Enabled    = true;
                    btnStartAuto.Enabled = false; // scheduler vẫn chạy

                    // ── Quay lại "đang chờ" (chấm xanh dương) ─────────────────
                    _trayIconService.SetState(TrayState.Waiting);
                }
            });
        }

        /// <summary>Stop Auto — dừng timer.</summary>
        private void btnStopAuto_Click(object sender, EventArgs e)
        {
            _schedulerService.Stop();
            btnStartAuto.Enabled = true;
            btnStopAuto.Enabled  = false;
            btnRunNow.Enabled    = true;
            Log("INFO", "Auto scheduler đã dừng.");

            // ── Tắt tray icon: không còn schedule nào để theo dõi nữa ────────
            _trayIconService.Hide();
        }

        /// <summary>Browse — mở OpenFileDialog để chọn RamMap.exe.</summary>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title       = "Chọn RamMap.exe",
                Filter      = "RamMap.exe|RamMap.exe|Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                FilterIndex = 1,
            };

            string currentPath = txtRamMapPath.Text;
            if (!string.IsNullOrEmpty(currentPath))
            {
                string? dir = Path.GetDirectoryName(currentPath);
                if (Directory.Exists(dir)) dlg.InitialDirectory = dir;
            }

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                txtRamMapPath.Text = dlg.FileName;
                SaveRamMapPath(dlg.FileName);
                Log("INFO", $"RamMap path cập nhật: {dlg.FileName}");

                if (!File.Exists(dlg.FileName))
                    Log("WARNING", "File được chọn không tồn tại.");
            }
        }

        /// <summary>Clear Log — xóa toàn bộ nội dung log.</summary>
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            memoLog.Clear();
            Log("INFO", "Log đã được xóa.");
        }
    }
}
