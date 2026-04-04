using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace QobuzRPC.Services;

public class QobuzMonitor : IDisposable
{
    private const string ProcessName = "Qobuz";
    private readonly Timer _timer;
    private IntPtr _qobuzHandle = IntPtr.Zero;
    
    public event EventHandler<string>? TitleChanged;
    public event EventHandler? QobuzClosed;
    
    private string _lastTitle = string.Empty;

    public QobuzMonitor()
    {
        _timer = new Timer(1000); // Check every second
        _timer.Elapsed += OnTimerElapsed;
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_qobuzHandle == IntPtr.Zero || !IsWindow(_qobuzHandle))
        {
            _qobuzHandle = FindQobuzWindow();
            if (_qobuzHandle == IntPtr.Zero)
            {
                if (!string.IsNullOrEmpty(_lastTitle))
                {
                    _lastTitle = string.Empty;
                    QobuzClosed?.Invoke(this, EventArgs.Empty);
                }
                return;
            }
        }

        var title = GetWindowTitle(_qobuzHandle);
        if (title != _lastTitle)
        {
            _lastTitle = title;
            TitleChanged?.Invoke(this, title);
        }
    }

    private IntPtr FindQobuzWindow()
    {
        var processes = Process.GetProcessesByName(ProcessName);
        foreach (var process in processes)
        {
            var handle = process.MainWindowHandle;
            if (handle != IntPtr.Zero && IsWindowVisible(handle))
            {
                var title = GetWindowTitle(handle);
                if (!string.IsNullOrEmpty(title))
                    return handle;
            }
        }
        return IntPtr.Zero;
    }

    private string GetWindowTitle(IntPtr handle)
    {
        var length = GetWindowTextLength(handle);
        if (length == 0) return string.Empty;

        var builder = new StringBuilder(length + 1);
        GetWindowText(handle, builder, builder.Capacity);
        return builder.ToString();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    // Windows API
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);
}
