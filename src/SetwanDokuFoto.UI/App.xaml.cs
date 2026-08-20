using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using SetwanDokuFoto.Core.Services;
using SetwanDokuFoto.DocxEngine;
using SetwanDokuFoto.PrintEngine;
using SetwanDokuFoto.Data;
using SetwanDokuFoto.UI.ViewModels;
using SetwanDokuFoto.UI.Views;

namespace SetwanDokuFoto.UI;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register Core Services (Dependency Injection)
                services.AddSingleton<IDocxExportService, DocxExportService>();
                services.AddSingleton<IPrintService, WpfPrintService>();
                services.AddSingleton<IProjectRepository, SqliteProjectRepository>();

                // Register ViewModels
                services.AddSingleton<MainViewModel>();

                // Register Windows
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
