using System;
using Avalonia.Controls;
using SukiUI.Controls;
using QobuzRPC.ViewModels;
using QobuzRPC.Services;

namespace QobuzRPC.Views;

public partial class MainWindow : SukiWindow
{
    private readonly SettingsService _settingsService;
    
    public MainWindow()
    {
        InitializeComponent();
        _settingsService = new SettingsService();
        
        // Load window size and position
        var settings = _settingsService.LoadSettings();
        
        if (!double.IsNaN(settings.WindowWidth) && settings.WindowWidth > 0)
            Width = settings.WindowWidth;
        
        if (!double.IsNaN(settings.WindowHeight) && settings.WindowHeight > 0)
            Height = settings.WindowHeight;
        
        if (!double.IsNaN(settings.WindowX) && !double.IsNaN(settings.WindowY))
        {
            Position = new Avalonia.PixelPoint((int)settings.WindowX, (int)settings.WindowY);
        }
        
        // Save settings when window closes
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            // Get current settings from ViewModel
            var settings = new AppSettings
            {
                DiscordClientId = viewModel.DiscordClientId,
                LastFmApiKey = viewModel.LastFmApiKey,
                WindowWidth = Width,
                WindowHeight = Height,
                WindowX = Position.X,
                WindowY = Position.Y
            };
            
            _settingsService.SaveSettings(settings);
        }
    }
}