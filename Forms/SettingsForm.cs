// File: Forms/SettingsForm.cs - Dialog cài đặt ứng dụng

using System;
using System.Windows.Forms;
using SystemCleaner.Services;

namespace SystemCleaner.Forms
{
    /// <summary>
    /// Dialog cài đặt hiển thị khi người dùng nhấn nút ⚙ trên form chính.
    /// Sử dụng <see cref="ShowDialog"/> (modal). Kết quả lưu ở <see cref="ResultSettings"/>
    /// nếu người dùng nhấn "Save" (DialogResult.OK).
    /// </summary>
    public partial class SettingsForm : Form
    {
        // ── Kết quả trả về cho MainForm sau khi người dùng nhấn Save ─────────
        /// <summary>null nếu người dùng huỷ. Chứa model mới nếu nhấn Save.</summary>
        public AppSettingsModel? ResultSettings { get; private set; }

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Khởi tạo SettingsForm và điền các control theo cài đặt hiện tại.
        /// </summary>
        /// <param name="current">Cài đặt đang được áp dụng từ <see cref="IAppSettingsService"/>.</param>
        public SettingsForm(AppSettingsModel current)
        {
            InitializeComponent();
            PopulateControls(current);
        }

        // ── Populate ─────────────────────────────────────────────────────────

        private void PopulateControls(AppSettingsModel s)
        {
            chkStartWithWindows.Checked  = s.StartWithWindows;
            chkMinimizeToTray.Checked    = s.MinimizeToTray;
            chkShowNotifications.Checked = s.ShowNotifications;
            chkAutoScrollLog.Checked     = s.AutoScrollLog;
            numDefaultInterval.Value     = Math.Clamp(s.DefaultInterval, 1, 1440);
        }

        // ── Button handlers ──────────────────────────────────────────────────

        private void btnSave_Click(object sender, EventArgs e)
        {
            ResultSettings = new AppSettingsModel
            {
                StartWithWindows  = chkStartWithWindows.Checked,
                MinimizeToTray    = chkMinimizeToTray.Checked,
                ShowNotifications = chkShowNotifications.Checked,
                AutoScrollLog     = chkAutoScrollLog.Checked,
                DefaultInterval   = (int)numDefaultInterval.Value,
            };

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ResultSettings = null;
            DialogResult   = DialogResult.Cancel;
            Close();
        }
    }
}
