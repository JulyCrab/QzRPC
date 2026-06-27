using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace QobuzRPC.Services;

public class MacOSQobuzMonitor : IProcessMonitor
{
    private const string ProcessName = "Qobuz";
    private readonly Timer _timer;
    private int _lastProcessId = 0;
    
    public event EventHandler<string>? TitleChanged;
    public event EventHandler? QobuzClosed;
    
    private string _lastTitle = string.Empty;

    public MacOSQobuzMonitor()
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
        var process = FindQobuzProcess();
        
        if (process == null)
        {
            if (_lastProcessId != 0)
            {
                _lastProcessId = 0;
                _lastTitle = string.Empty;
                QobuzClosed?.Invoke(this, EventArgs.Empty);
            }
            return;
        }

        _lastProcessId = process.Id;
        var title = GetWindowTitle(process);
        
        if (title != _lastTitle)
        {
            _lastTitle = title;
            TitleChanged?.Invoke(this, title);
        }
    }

    private Process? FindQobuzProcess()
    {
        var processes = Process.GetProcessesByName(ProcessName);
        return processes.FirstOrDefault();
    }

    private string GetWindowTitle(Process process)
    {
        try
        {
            // Use AppleScript to get the window title
            var startInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/osascript",
                Arguments = $"-e 'tell application \"System Events\" to get name of first window of process \"{ProcessName}\"'",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var scriptProcess = Process.Start(startInfo);
            if (scriptProcess != null)
            {
                var output = scriptProcess.StandardOutput.ReadToEnd().Trim();
                scriptProcess.WaitForExit();
                
                if (scriptProcess.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    return output;
                }
            }
        }
        catch
        {
            // Fallback: just return "Qobuz" if we can't get the title
        }

        return "Qobuz";
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
