// File: Forms/MainForm.Designer.cs - UI thuần WinForms .NET 8
#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SystemCleaner.Forms
{
    partial class MainForm
    {
        private IContainer? components = null;

        // ── ⚙ Settings button ────────────────────────────────────────────────
        private Button btnSettings;

        // ── Layout ──────────────────────────────────────────────────────────
        private TableLayoutPanel tableLayoutMain;
        private Panel            panelTopGroups;

        // ── Group 1: System Cleanup Tasks ───────────────────────────────────
        private GroupBox grpCleanup;
        private CheckBox chkTempWindows;
        private CheckBox chkRecycleBin;
        private CheckBox chkPrefetch;
        private CheckBox chkUserTemp;

        // ── Group 2: RAMMap Options ──────────────────────────────────────────
        private GroupBox grpRamMap;
        private Label    lblRamMapPath;
        private TextBox  txtRamMapPath;
        private Button   btnBrowse;
        private CheckBox chkWorkingSets;
        private CheckBox chkSystemWorkingSet;
        private CheckBox chkModifiedPageList;
        private CheckBox chkStandbyList;
        private CheckBox chkPriority0Standby;

        // ── Group 3: Auto Schedule ───────────────────────────────────────────
        private GroupBox      grpSchedule;
        private Label         lblRunEvery;
        private NumericUpDown spinInterval;
        private Label         lblMinutes;
        private Button        btnRunNow;
        private Button        btnStartAuto;
        private Button        btnStopAuto;

        // ── Group 4: Activity Log ────────────────────────────────────────────
        private GroupBox    grpLog;
        private RichTextBox memoLog;
        private Button      btnClearLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();

            btnSettings          = new Button();
            tableLayoutMain      = new TableLayoutPanel();
            panelTopGroups       = new Panel();
            grpCleanup           = new GroupBox();
            chkTempWindows       = new CheckBox();
            chkRecycleBin        = new CheckBox();
            chkPrefetch          = new CheckBox();
            chkUserTemp          = new CheckBox();
            grpRamMap            = new GroupBox();
            lblRamMapPath        = new Label();
            txtRamMapPath        = new TextBox();
            btnBrowse            = new Button();
            chkWorkingSets       = new CheckBox();
            chkSystemWorkingSet  = new CheckBox();
            chkModifiedPageList  = new CheckBox();
            chkStandbyList       = new CheckBox();
            chkPriority0Standby  = new CheckBox();
            grpSchedule          = new GroupBox();
            lblRunEvery          = new Label();
            spinInterval         = new NumericUpDown();
            lblMinutes           = new Label();
            btnRunNow            = new Button();
            btnStartAuto         = new Button();
            btnStopAuto          = new Button();
            grpLog               = new GroupBox();
            memoLog              = new RichTextBox();
            btnClearLog          = new Button();

            // ── SuspendLayout ────────────────────────────────────────────────
            ((ISupportInitialize)spinInterval).BeginInit();
            tableLayoutMain.SuspendLayout();
            panelTopGroups.SuspendLayout();
            grpCleanup.SuspendLayout();
            grpRamMap.SuspendLayout();
            grpSchedule.SuspendLayout();
            grpLog.SuspendLayout();
            SuspendLayout();

            // ═══════════════════════════════════════════════════════════════
            //  ⚙ SETTINGS BUTTON
            //  Đặt trong tableLayoutMain cell (0,0) — TableLayoutPanel tự
            //  quản lý kích thước cell → Anchor=Right luôn đúng, không phụ
            //  thuộc vào thời điểm form layout giống cách đặt thẳng lên form.
            // ═══════════════════════════════════════════════════════════════
            btnSettings.Name      = "btnSettings";
            btnSettings.Text      = "⚙";
            btnSettings.Font      = new Font("Segoe UI", 13F);
            btnSettings.Size      = new Size(36, 28);
            btnSettings.Anchor    = AnchorStyles.Top | AnchorStyles.Right;
            btnSettings.Margin    = new Padding(0, 3, 8, 0);
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.FlatAppearance.BorderSize         = 1;
            btnSettings.FlatAppearance.BorderColor        = Color.LightGray;
            btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 220, 220);
            btnSettings.FlatAppearance.MouseDownBackColor = Color.FromArgb(190, 190, 190);
            btnSettings.Cursor    = Cursors.Hand;
            btnSettings.Click    += btnSettings_Click;

            // ═══════════════════════════════════════════════════════════════
            //  tableLayoutMain — 4 hàng:
            //    row 0: toolbar (34px)  ← nút ⚙ ở đây
            //    row 1: top-groups (238px)
            //    row 2: schedule (78px)
            //    row 3: log (fill)
            // ═══════════════════════════════════════════════════════════════
            tableLayoutMain.Name        = "tableLayoutMain";
            tableLayoutMain.Dock        = DockStyle.Fill;
            tableLayoutMain.Padding     = new Padding(6, 4, 6, 6);
            tableLayoutMain.ColumnCount = 1;
            tableLayoutMain.RowCount    = 4;
            tableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));   // toolbar
            tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 238F));  // groups
            tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));   // schedule
            tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // log
            tableLayoutMain.Controls.Add(btnSettings,    0, 0);  // ← nút ⚙ vào row 0
            tableLayoutMain.Controls.Add(panelTopGroups, 0, 1);
            tableLayoutMain.Controls.Add(grpSchedule,    0, 2);
            tableLayoutMain.Controls.Add(grpLog,         0, 3);

            // ═══════════════════════════════════════════════════════════════
            //  panelTopGroups
            // ═══════════════════════════════════════════════════════════════
            panelTopGroups.Name = "panelTopGroups";
            panelTopGroups.Dock = DockStyle.Fill;
            panelTopGroups.Controls.Add(grpRamMap);
            panelTopGroups.Controls.Add(grpCleanup);

            // ═══════════════════════════════════════════════════════════════
            //  GROUP 1 — System Cleanup Tasks
            // ═══════════════════════════════════════════════════════════════
            grpCleanup.Name  = "grpCleanup";
            grpCleanup.Text  = "System Cleanup Tasks";
            grpCleanup.Dock  = DockStyle.Left;
            grpCleanup.Width = 362;
            grpCleanup.Font  = new Font("Segoe UI", 9F);
            grpCleanup.Controls.Add(chkTempWindows);
            grpCleanup.Controls.Add(chkRecycleBin);
            grpCleanup.Controls.Add(chkPrefetch);
            grpCleanup.Controls.Add(chkUserTemp);

            chkTempWindows.Name     = "chkTempWindows";
            chkTempWindows.Text     = "Clean Temp Windows  (C:\\Windows\\Temp)";
            chkTempWindows.Location = new Point(14, 30);
            chkTempWindows.Size     = new Size(334, 22);
            chkTempWindows.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            chkRecycleBin.Name     = "chkRecycleBin";
            chkRecycleBin.Text     = "Clean Recycle Bin   (SHEmptyRecycleBin)";
            chkRecycleBin.Location = new Point(14, 58);
            chkRecycleBin.Size     = new Size(334, 22);
            chkRecycleBin.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            chkPrefetch.Name     = "chkPrefetch";
            chkPrefetch.Text     = "Clean Prefetch      (C:\\Windows\\Prefetch *.pf)";
            chkPrefetch.Location = new Point(14, 86);
            chkPrefetch.Size     = new Size(334, 22);
            chkPrefetch.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            chkUserTemp.Name     = "chkUserTemp";
            chkUserTemp.Text     = "Clean %TEMP%        (User Temp Folder)";
            chkUserTemp.Location = new Point(14, 114);
            chkUserTemp.Size     = new Size(334, 22);
            chkUserTemp.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // ═══════════════════════════════════════════════════════════════
            //  GROUP 2 — RAMMap Options
            // ═══════════════════════════════════════════════════════════════
            grpRamMap.Name = "grpRamMap";
            grpRamMap.Text = "RAMMap Options";
            grpRamMap.Dock = DockStyle.Fill;
            grpRamMap.Font = new Font("Segoe UI", 9F);
            grpRamMap.Controls.Add(chkPriority0Standby);
            grpRamMap.Controls.Add(chkStandbyList);
            grpRamMap.Controls.Add(chkModifiedPageList);
            grpRamMap.Controls.Add(chkSystemWorkingSet);
            grpRamMap.Controls.Add(chkWorkingSets);
            grpRamMap.Controls.Add(btnBrowse);
            grpRamMap.Controls.Add(txtRamMapPath);
            grpRamMap.Controls.Add(lblRamMapPath);

            lblRamMapPath.Name     = "lblRamMapPath";
            lblRamMapPath.Text     = "RAMMap Path:";
            lblRamMapPath.Location = new Point(12, 30);
            lblRamMapPath.Size     = new Size(88, 20);
            lblRamMapPath.AutoSize = false;

            txtRamMapPath.Name     = "txtRamMapPath";
            txtRamMapPath.Location = new Point(104, 27);
            txtRamMapPath.Size     = new Size(370, 23);
            txtRamMapPath.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            btnBrowse.Name     = "btnBrowse";
            btnBrowse.Text     = "Browse...";
            btnBrowse.Location = new Point(484, 26);
            btnBrowse.Size     = new Size(82, 26);
            btnBrowse.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Click   += btnBrowse_Click;

            chkWorkingSets.Name     = "chkWorkingSets";
            chkWorkingSets.Text     = "Empty Working Sets          (-Ew)";
            chkWorkingSets.Location = new Point(12, 62);
            chkWorkingSets.Size     = new Size(550, 22);
            chkWorkingSets.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            chkSystemWorkingSet.Name     = "chkSystemWorkingSet";
            chkSystemWorkingSet.Text     = "Empty System Working Set    (-Es)";
            chkSystemWorkingSet.Location = new Point(12, 90);
            chkSystemWorkingSet.Size     = new Size(550, 22);
            chkSystemWorkingSet.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            chkModifiedPageList.Name     = "chkModifiedPageList";
            chkModifiedPageList.Text     = "Empty Modified Page List    (-Em)";
            chkModifiedPageList.Location = new Point(12, 118);
            chkModifiedPageList.Size     = new Size(550, 22);
            chkModifiedPageList.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            chkStandbyList.Name     = "chkStandbyList";
            chkStandbyList.Text     = "Empty Standby List          (-Et)";
            chkStandbyList.Location = new Point(12, 146);
            chkStandbyList.Size     = new Size(550, 22);
            chkStandbyList.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            chkPriority0Standby.Name     = "chkPriority0Standby";
            chkPriority0Standby.Text     = "Empty Priority 0 Standby    (-E0)";
            chkPriority0Standby.Location = new Point(12, 174);
            chkPriority0Standby.Size     = new Size(550, 22);
            chkPriority0Standby.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // ═══════════════════════════════════════════════════════════════
            //  GROUP 3 — Auto Schedule
            // ═══════════════════════════════════════════════════════════════
            grpSchedule.Name = "grpSchedule";
            grpSchedule.Text = "Auto Schedule";
            grpSchedule.Dock = DockStyle.Fill;
            grpSchedule.Font = new Font("Segoe UI", 9F);
            grpSchedule.Controls.Add(lblRunEvery);
            grpSchedule.Controls.Add(spinInterval);
            grpSchedule.Controls.Add(lblMinutes);
            grpSchedule.Controls.Add(btnRunNow);
            grpSchedule.Controls.Add(btnStartAuto);
            grpSchedule.Controls.Add(btnStopAuto);

            lblRunEvery.Name     = "lblRunEvery";
            lblRunEvery.Text     = "Run every:";
            lblRunEvery.Location = new Point(12, 32);
            lblRunEvery.Size     = new Size(68, 20);
            lblRunEvery.AutoSize = false;

            spinInterval.Name          = "spinInterval";
            spinInterval.Location      = new Point(84, 29);
            spinInterval.Size          = new Size(72, 26);
            spinInterval.Minimum       = 1;
            spinInterval.Maximum       = 1440;
            spinInterval.Increment     = 1;
            spinInterval.DecimalPlaces = 0;
            spinInterval.Value         = 5;

            lblMinutes.Name     = "lblMinutes";
            lblMinutes.Text     = "minutes";
            lblMinutes.Location = new Point(161, 32);
            lblMinutes.Size     = new Size(55, 20);
            lblMinutes.AutoSize = false;

            btnRunNow.Name     = "btnRunNow";
            btnRunNow.Text     = "\u25B6  Run Now";
            btnRunNow.Location = new Point(234, 28);
            btnRunNow.Size     = new Size(118, 28);
            btnRunNow.Click   += btnRunNow_Click;

            btnStartAuto.Name     = "btnStartAuto";
            btnStartAuto.Text     = "\u23F1  Start Auto";
            btnStartAuto.Location = new Point(360, 28);
            btnStartAuto.Size     = new Size(118, 28);
            btnStartAuto.Click   += btnStartAuto_Click;

            btnStopAuto.Name     = "btnStopAuto";
            btnStopAuto.Text     = "\u23F9  Stop Auto";
            btnStopAuto.Location = new Point(486, 28);
            btnStopAuto.Size     = new Size(118, 28);
            btnStopAuto.Click   += btnStopAuto_Click;

            // ═══════════════════════════════════════════════════════════════
            //  GROUP 4 — Activity Log
            // ═══════════════════════════════════════════════════════════════
            grpLog.Name = "grpLog";
            grpLog.Text = "Activity Log";
            grpLog.Dock = DockStyle.Fill;
            grpLog.Font = new Font("Segoe UI", 9F);
            grpLog.Controls.Add(memoLog);
            grpLog.Controls.Add(btnClearLog);

            btnClearLog.Name     = "btnClearLog";
            btnClearLog.Text     = "\U0001F5D1  Clear Log";
            btnClearLog.Size     = new Size(108, 26);
            btnClearLog.Location = new Point(850, 24);
            btnClearLog.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnClearLog.Click   += btnClearLog_Click;

            memoLog.Name       = "memoLog";
            memoLog.Location   = new Point(10, 56);
            memoLog.Size       = new Size(940, 300);
            memoLog.Anchor     = AnchorStyles.Top | AnchorStyles.Bottom
                               | AnchorStyles.Left | AnchorStyles.Right;
            memoLog.Font       = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            memoLog.ReadOnly   = true;
            memoLog.ScrollBars = RichTextBoxScrollBars.Both;
            memoLog.WordWrap   = false;
            memoLog.BackColor  = SystemColors.Window;

            // ═══════════════════════════════════════════════════════════════
            //  FORM
            // ═══════════════════════════════════════════════════════════════
            Name                = "MainForm";
            Text                = "SystemCleaner  \u2013  System Cleanup Tool";
            ClientSize          = new Size(980, 760);
            MinimumSize         = new Size(820, 600);
            StartPosition       = FormStartPosition.CenterScreen;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            Font                = new Font("Segoe UI", 9F);
            Controls.Add(tableLayoutMain);
            Load        += MainForm_Load;
            FormClosing += MainForm_FormClosing;

            // ── ResumeLayout ─────────────────────────────────────────────────
            ((ISupportInitialize)spinInterval).EndInit();
            grpCleanup.ResumeLayout(false);
            grpRamMap.ResumeLayout(false);
            grpSchedule.ResumeLayout(false);
            grpLog.ResumeLayout(false);
            panelTopGroups.ResumeLayout(false);
            tableLayoutMain.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
