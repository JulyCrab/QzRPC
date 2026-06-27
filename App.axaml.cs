using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using QobuzRPC.ViewModels;
using QobuzRPC.Views;

namespace QobuzRPC;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Since Avalonia 12 the DataAnnotations validation plugin is disabled by
            // default, so the old manual BindingPlugins.DataValidators.Remove(...)
            // workaround is no longer needed.
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
