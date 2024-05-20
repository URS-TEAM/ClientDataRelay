using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Hardcodet.Wpf.TaskbarNotification;
using InventoryUtility.Windows;
using Microsoft.Extensions.Hosting;

namespace InventoryUtility.Services;

/// <summary>
/// Managed host of the application.
/// </summary>
public class ApplicationHostService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationHostService(IServiceProvider serviceProvider)
    {
        // If you want, you can do something with these services at the beginning of loading the application.
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await HandleActivationAsync();
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public TaskbarIcon MyTaskbarIcon { get; set; }

    /// <summary>
    /// Creates main window during activation.
    /// </summary>
    private async Task HandleActivationAsync()
    {
        await Task.CompletedTask;

        MainWindow? mainWindow = mainWindow = _serviceProvider.GetService(typeof(MainWindow)) as MainWindow;


        ICommand OnTrayIconLeftClick = new RelayCommand(() =>
        {
            if (mainWindow != null) mainWindow.Show();
        });

        BitmapImage logo = new BitmapImage();
        logo.BeginInit();
        logo.UriSource = new Uri("pack://application:,,,/ClientDataRelay;component/Assets/logo-urs-globe.ico", UriKind.Absolute);
        logo.EndInit();
        MyTaskbarIcon = new() { IconSource = logo, LeftClickCommand = OnTrayIconLeftClick };
        Popup pu = new();
        pu.Child = MyTaskbarIcon;

        await Task.CompletedTask;
    }
}
