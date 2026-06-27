using System;

namespace QobuzRPC.Services;

public interface IProcessMonitor : IDisposable
{
    event EventHandler<string>? TitleChanged;
    event EventHandler? QobuzClosed;
    
    void Start();
    void Stop();
}
