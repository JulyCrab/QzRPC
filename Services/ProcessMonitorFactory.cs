using System;
using System.Runtime.InteropServices;

namespace QobuzRPC.Services;

public static class ProcessMonitorFactory
{
    public static IProcessMonitor Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsQobuzMonitor();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacOSQobuzMonitor();
        }
        else
        {
            throw new PlatformNotSupportedException("Only Windows and macOS are currently supported.");
        }
    }
}
