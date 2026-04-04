using System;
using DiscordRPC;
using DiscordRPC.Logging;

namespace QobuzRPC.Services;

public class DiscordRpcService : IDisposable
{
    private DiscordRpcClient? _client;
    private readonly string _clientId;

    public DiscordRpcService(string clientId)
    {
        _clientId = clientId;
    }

    public void Initialize()
    {
        _client = new DiscordRpcClient(_clientId);
        _client.Logger = new ConsoleLogger { Level = LogLevel.Warning };
        _client.Initialize();
    }

    public void UpdatePresence(string trackName, string artistName, string? albumArtUrl, bool isPaused = false)
    {
        if (_client == null || !_client.IsInitialized)
            return;

        var presence = new RichPresence
        {
            Details = trackName,
            State = isPaused ? $"by {artistName} • Paused" : $"by {artistName}",
            Assets = new Assets
            {
                LargeImageKey = albumArtUrl ?? "qobuz",
                LargeImageText = albumArtUrl != null ? $"{trackName} - {artistName}" : "Qobuz"
            }
        };

        // only show elapsed time when actually playing
        if (!isPaused)
        {
            presence.Timestamps = new Timestamps
            {
                Start = DateTime.UtcNow
            };
        }

        _client.SetPresence(presence);
    }

    public void SetIdle()
    {
        if (_client == null || !_client.IsInitialized)
            return;

        var presence = new RichPresence
        {
            State = "Idle",
            Assets = new Assets
            {
                LargeImageKey = "qobuz",
                LargeImageText = "Qobuz"
            }
        };

        _client.SetPresence(presence);
    }

    public void Clear()
    {
        _client?.ClearPresence();
    }

    public void Dispose()
    {
        Clear();
        _client?.Dispose();
    }
}
