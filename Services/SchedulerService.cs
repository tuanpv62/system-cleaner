// File: Services/SchedulerService.cs - Dịch vụ lập lịch tự động dùng WinForms Timer (UI-thread safe)

using System;
using System.Threading.Tasks;
// Alias tường minh để tránh xung đột với System.Threading.Timer
// (System.Threading được đưa vào ngầm định qua ImplicitUsings của SDK WinForms).
using Timer = System.Windows.Forms.Timer;

namespace SystemCleaner.Services
{
    // ═══════════════════════════════════════════════════════
    //  INTERFACE
    // ═══════════════════════════════════════════════════════

    /// <summary>Interface cho dịch vụ lập lịch.</summary>
    public interface ISchedulerService
    {
        /// <summary>Scheduler hiện đang chạy hay không.</summary>
        bool IsRunning { get; }

        /// <summary>Interval hiện tại (phút).</summary>
        int IntervalMinutes { get; }

        /// <summary>Bắt đầu lập lịch với interval và job được chỉ định.</summary>
        void Start(int intervalMinutes, Func<Task> job);

        /// <summary>Dừng lập lịch.</summary>
        void Stop();
    }

    // ═══════════════════════════════════════════════════════
    //  IMPLEMENTATION
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Triển khai lập lịch dùng <see cref="System.Windows.Forms.Timer"/> để đảm bảo
    /// callback luôn chạy trên UI thread — an toàn khi cập nhật controls trực tiếp.
    ///
    /// <para>
    /// Gọi <see cref="Start"/> từ UI thread (ví dụ button Click) để Timer được tạo đúng
    /// thread-affinity của WinForms message loop.
    /// </para>
    /// </summary>
    public sealed class SchedulerService : ISchedulerService, IDisposable
    {
        // ── State ────────────────────────────────────────────────────────────
        private Timer?      _timer;
        private Func<Task>? _job;
        private bool        _disposed;

        /// <inheritdoc/>
        public bool IsRunning { get; private set; }

        /// <inheritdoc/>
        public int IntervalMinutes { get; private set; }

        // ────────────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        /// <remarks>
        /// Phải gọi từ UI thread. Timer được tạo lazily tại đây nên chắc chắn
        /// có message-loop context đúng.
        /// </remarks>
        public void Start(int intervalMinutes, Func<Task> job)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(SchedulerService));

            // Dừng timer cũ (nếu có) trước khi start mới
            StopInternal();

            IntervalMinutes = Math.Clamp(intervalMinutes, 1, 1440);
            _job            = job;
            IsRunning       = true;

            // Tạo System.Windows.Forms.Timer trên UI thread
            _timer          = new Timer();
            _timer.Interval = IntervalMinutes * 60 * 1_000; // milliseconds
            _timer.Tick    += Timer_Tick;
            _timer.Start();
        }

        /// <inheritdoc/>
        public void Stop()
        {
            StopInternal();
        }

        // ── Timer callback ───────────────────────────────────────────────────

        /// <summary>
        /// Chạy trên UI thread (System.Windows.Forms.Timer đảm bảo điều này).
        /// Dừng timer trong lúc job chạy để tránh concurrent run.
        /// </summary>
        private async void Timer_Tick(object? sender, EventArgs e)
        {
            // Dừng timer ngay lập tức để tránh tick chồng chéo trong khi job chạy
            _timer?.Stop();

            try
            {
                if (_job != null)
                    await _job();
            }
            catch (Exception ex)
            {
                // Không để exception lan ra async void (sẽ unhandled)
                System.Diagnostics.Debug.WriteLine($"[SchedulerService] Job error: {ex.Message}");
            }
            finally
            {
                // Khởi động lại timer chỉ khi người dùng chưa Stop
                if (IsRunning && _timer != null)
                    _timer.Start();
            }
        }

        // ── Internals ────────────────────────────────────────────────────────

        private void StopInternal()
        {
            IsRunning = false;
            _job      = null;

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                StopInternal();
                _disposed = true;
            }
        }
    }
}
