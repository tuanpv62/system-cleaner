// File: Forms/SettingsForm.Designer.cs - UI của dialog cài đặt
#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SystemCleaner.Forms
{
    partial class SettingsForm
    {
        private IContainer? components = null;

        // ── Controls ─────────────────────────────────────────────────────────
        private TableLayoutPanel tableLayout;

        // Group 1: System
        private GroupBox grpSystem;
        private CheckBox chkStartWithWindows;

        // Group 2: Behavior
        private GroupBox grpBehavior;
        private CheckBox chkMinimizeToTray;
        private CheckBox chkShowNotifications;
        private CheckBox chkAutoScrollLog;

        // Group 3: Defaults
        private GroupBox      grpDefaults;
        private Label         lblDefaultInterval;
        private NumericUpDown numDefaultInterval;
        private Label         lblMinutes;

        // Buttons
        private Panel  pnlButtons;
        private Button btnSave;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();

            // Instances
            tableLayout          = new TableLayoutPanel();
            grpSystem            = new GroupBox();
            chkStartWithWindows  = new CheckBox();
            grpBehavior          = new GroupBox();
            chkMinimizeToTray    = new CheckBox();
            chkShowNotifications = new CheckBox();
            chkAutoScrollLog     = new CheckBox();
            grpDefaults          = new GroupBox();
            lblDefaultInterval   = new Label();
            numDefaultInterval   = new NumericUpDown();
            lblMinutes           = new Label();
            pnlButtons           = new Panel();
            btnSave              = new Button();
            btnCancel            = new Button();

            ((ISupportInitialize)numDefaultInterval).BeginInit();
            tableLayout.SuspendLayout();
            grpSystem.SuspendLayout();
            grpBehavior.SuspendLayout();
            grpDefaults.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();

            // ── tableLayout ─────────────────────────────────────────────────
            tableLayout.Dock        = DockStyle.Fill;
            tableLayout.Padding     = new Padding(10, 8, 10, 4);
            tableLayout.ColumnCount = 1;
            tableLayout.RowCount    = 4;
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));   // System
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 116F));  // Behavior
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));   // Defaults
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // Buttons
            tableLayout.Controls.Add(grpSystem,   0, 0);
            tableLayout.Controls.Add(grpBehavior, 0, 1);
            tableLayout.Controls.Add(grpDefaults, 0, 2);
            tableLayout.Controls.Add(pnlButtons,  0, 3);

            // ── grpSystem ───────────────────────────────────────────────────
            grpSystem.Text  = "System";
            grpSystem.Dock  = DockStyle.Fill;
            grpSystem.Font  = new Font("Segoe UI", 9F);
            grpSystem.Controls.Add(chkStartWithWindows);

            chkStartWithWindows.Text     = "Start with Windows";
            chkStartWithWindows.Location = new Point(14, 26);
            chkStartWithWindows.Size     = new Size(360, 22);
            chkStartWithWindows.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // ── grpBehavior ─────────────────────────────────────────────────
            grpBehavior.Text  = "Behavior";
            grpBehavior.Dock  = DockStyle.Fill;
            grpBehavior.Font  = new Font("Segoe UI", 9F);
            grpBehavior.Controls.Add(chkMinimizeToTray);
            grpBehavior.Controls.Add(chkShowNotifications);
            grpBehavior.Controls.Add(chkAutoScrollLog);

            chkMinimizeToTray.Text     = "Minimize to system tray instead of closing";
            chkMinimizeToTray.Location = new Point(14, 24);
            chkMinimizeToTray.Size     = new Size(360, 22);
            chkMinimizeToTray.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            chkShowNotifications.Text     = "Show tray balloon notifications";
            chkShowNotifications.Location = new Point(14, 52);
            chkShowNotifications.Size     = new Size(360, 22);
            chkShowNotifications.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            chkAutoScrollLog.Text     = "Auto-scroll Activity Log to latest entry";
            chkAutoScrollLog.Location = new Point(14, 80);
            chkAutoScrollLog.Size     = new Size(360, 22);
            chkAutoScrollLog.Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // ── grpDefaults ─────────────────────────────────────────────────
            grpDefaults.Text  = "Defaults";
            grpDefaults.Dock  = DockStyle.Fill;
            grpDefaults.Font  = new Font("Segoe UI", 9F);
            grpDefaults.Controls.Add(lblDefaultInterval);
            grpDefaults.Controls.Add(numDefaultInterval);
            grpDefaults.Controls.Add(lblMinutes);

            lblDefaultInterval.Text     = "Default schedule interval:";
            lblDefaultInterval.Location = new Point(14, 30);
            lblDefaultInterval.Size     = new Size(165, 20);
            lblDefaultInterval.AutoSize = false;

            numDefaultInterval.Location      = new Point(183, 27);
            numDefaultInterval.Size          = new Size(68, 26);
            numDefaultInterval.Minimum       = 1;
            numDefaultInterval.Maximum       = 1440;
            numDefaultInterval.Value         = 5;
            numDefaultInterval.DecimalPlaces = 0;

            lblMinutes.Text     = "minutes";
            lblMinutes.Location = new Point(256, 30);
            lblMinutes.Size     = new Size(60, 20);
            lblMinutes.AutoSize = false;

            // ── pnlButtons ──────────────────────────────────────────────────
            pnlButtons.Dock = DockStyle.Fill;
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnSave);

            btnSave.Text     = "Save";
            btnSave.Size     = new Size(90, 30);
            btnSave.Location = new Point(204, 8);
            btnSave.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Click   += btnSave_Click;

            btnCancel.Text     = "Cancel";
            btnCancel.Size     = new Size(90, 30);
            btnCancel.Location = new Point(302, 8);
            btnCancel.Anchor   = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Click   += btnCancel_Click;

            // ── Form ────────────────────────────────────────────────────────
            Name                = "SettingsForm";
            Text                = "Settings";
            ClientSize          = new Size(420, 330);
            FormBorderStyle     = FormBorderStyle.FixedDialog;
            MaximizeBox         = false;
            MinimizeBox         = false;
            StartPosition       = FormStartPosition.CenterParent;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            Font                = new Font("Segoe UI", 9F);
            Controls.Add(tableLayout);

            ((ISupportInitialize)numDefaultInterval).EndInit();
            grpSystem.ResumeLayout(false);
            grpBehavior.ResumeLayout(false);
            grpDefaults.ResumeLayout(false);
            pnlButtons.ResumeLayout(false);
            tableLayout.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
