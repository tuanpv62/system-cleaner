// File: Program.cs - Điểm khởi đầu ứng dụng

using System.Windows.Forms;
using SystemCleaner.Forms;
using SystemCleaner.Services;

namespace SystemCleaner
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // WinForms thuần — không cần DevExpress skin
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

            // Khởi tạo Services (Dependency Injection thủ công)
            var cleanerService   = new CleanerService();
            var ramMapService    = new RamMapService();
            var schedulerService = new SchedulerService();
            var trayIconService  = new TrayIconService();

            Application.Run(new MainForm(cleanerService, ramMapService, schedulerService, trayIconService));
        }
    }
}
