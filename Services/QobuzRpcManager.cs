using System;
using System.Threading.Tasks;
using System.Timers;

namespace QobuzRPC.Services;

public class QobuzRpcManager : IDisposable
{
    private readonly QobuzMonitor _monitor;
    private readonly DiscordRpcService _discordRpc;
    private readonly MusicMetadataService _metadataService;
    
    private string _currentTitle = string.Empty;
    private (string Track, string Artist, string? AlbumArt)? _lastTrackInfo;
    private DateTime? _pausedTime;
    private readonly Timer _pauseCheckTimer;

    public event EventHandler<string>? StatusChanged;

    public QobuzRpcManager(string discordClientId)
    {
        _monitor = new QobuzMonitor();
        _discordRpc = new DiscordRpcService(discordClientId);
        _metadataService = new MusicMetadataService();
        
        _pauseCheckTimer = new Timer(1000);
        _pauseCheckTimer.Elapsed += CheckPauseTimeout;
        
        _monitor.TitleChanged += OnTitleChanged;
        _monitor.QobuzClosed += OnQobuzClosed;
    }

    public void SetLastFmApiKey(string? apiKey)
    {
        _metadataService.SetLastFmApiKey(apiKey);
    }

    public void Start()
    {
        _discordRpc.Initialize();
        _monitor.Start();
        _pauseCheckTimer.Start();
        UpdateStatus("Waiting for Qobuz...");
    }

    public void Stop()
    {
        _monitor.Stop();
        _pauseCheckTimer.Stop();
        _discordRpc.Clear();
        UpdateStatus("Stopped");
    }

    private async void OnTitleChanged(object? sender, string title)
    {
        if (_currentTitle == title)
            return;

        _currentTitle = title;

        if (string.IsNullOrEmpty(title) || title == "Qobuz")
        {
            // paused or nothing playing
            if (_lastTrackInfo.HasValue)
            {
                _pausedTime = DateTime.Now;
                var track = _lastTrackInfo.Value;
                _discordRpc.UpdatePresence(track.Track, track.Artist, track.AlbumArt, isPaused: true);
                UpdateStatus($"Paused: {track.Track}");
            }
            else
            {
                _discordRpc.SetIdle();
                UpdateStatus("Idle");
            }
        }
        else
        {
            // something's playing!
            _pausedTime = null;
            
            if (title.Contains(" - "))
            {
                var parts = title.Split(new[] { " - " }, 2, StringSplitOptions.None);
                var trackName = parts[0].Trim();
                var artistName = parts[1].Trim();

                UpdateStatus($"Loading: {trackName}...");
                
                // grab album art from last.fm or itunes
                var (albumArt, _) = await _metadataService.GetTrackInfoAsync(trackName, artistName);
                
                _lastTrackInfo = (trackName, artistName, albumArt);
                _discordRpc.UpdatePresence(trackName, artistName, albumArt);
                
                UpdateStatus($"Playing: {trackName} by {artistName}");
            }
        }
    }

    private void OnQobuzClosed(object? sender, EventArgs e)
    {
        _lastTrackInfo = null;
        _pausedTime = null;
        _discordRpc.SetIdle();
        UpdateStatus("Qobuz closed");
    }

    private void CheckPauseTimeout(object? sender, ElapsedEventArgs e)
    {
        // if paused for more than a minute, just show idle
        if (_pausedTime.HasValue && (DateTime.Now - _pausedTime.Value).TotalSeconds >= 60)
        {
            _lastTrackInfo = null;
            _pausedTime = null;
            _discordRpc.SetIdle();
            UpdateStatus("Idle");
        }
    }

    private void UpdateStatus(string status)
    {
        StatusChanged?.Invoke(this, status);
    }

    public void Dispose()
    {
        _monitor?.Dispose();
        _discordRpc?.Dispose();
        _pauseCheckTimer?.Dispose();
    }
}
