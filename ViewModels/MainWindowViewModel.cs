using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QobuzRPC.Services;

namespace QobuzRPC.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly QobuzRpcManager _rpcManager;
    private readonly SettingsService _settingsService;
    
    [ObservableProperty]
    private string _status = "Stopped";
    
    [ObservableProperty]
    private string _discordClientId = "1490007914461790399";
    
    [ObservableProperty]
    private string _lastFmApiKey = string.Empty;
    
    [ObservableProperty]
    private bool _isRunning;
    
    [ObservableProperty]
    private bool _isDisclaimerExpanded;

    public MainWindowViewModel()
    {
        _settingsService = new SettingsService();
        
        // load saved stuff from last time
        var settings = _settingsService.LoadSettings();
        _discordClientId = settings.DiscordClientId;
        _lastFmApiKey = settings.LastFmApiKey;
        
        _rpcManager = new QobuzRpcManager(_discordClientId);
        _rpcManager.StatusChanged += OnStatusChanged;
    }

    [RelayCommand]
    private void StartMonitoring()
    {
        if (IsRunning) return;
        
        // save before we start
        SaveCurrentSettings();
        
        _rpcManager.SetLastFmApiKey(string.IsNullOrWhiteSpace(LastFmApiKey) ? null : LastFmApiKey);
        _rpcManager.Start();
        IsRunning = true;
        Status = "Starting...";
    }
    
    [RelayCommand]
    private void ToggleDisclaimer()
    {
        IsDisclaimerExpanded = !IsDisclaimerExpanded;
    }

    [RelayCommand]
    private void StopMonitoring()
    {
        if (!IsRunning) return;
        
        // save when stopping too
        SaveCurrentSettings();
        
        _rpcManager.Stop();
        IsRunning = false;
        Status = "Stopped";
    }

    private void OnStatusChanged(object? sender, string status)
    {
        Status = status;
    }

    private void SaveCurrentSettings()
    {
        var settings = new AppSettings
        {
            DiscordClientId = DiscordClientId,
            LastFmApiKey = LastFmApiKey
        };
        
        _settingsService.SaveSettings(settings);
    }

    public void SaveSettings()
    {
        SaveCurrentSettings();
    }

    public AppSettings LoadSettings()
    {
        return _settingsService.LoadSettings();
    }
}
