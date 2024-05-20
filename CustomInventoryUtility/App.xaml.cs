using InventoryUtility.Models;
using InventoryUtility.Models.Functionalities;
using InventoryUtility.Models.Summaries;
using InventoryUtility.Services;
using InventoryUtility.Services.Sql;
using InventoryUtility.ViewModels;
using InventoryUtility.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sqlite;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Wpf.Ui;

namespace InventoryUtility
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly IHost _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c =>
            {
                c.SetBasePath(AppContext.BaseDirectory);
            })
            .ConfigureServices((context, services) =>
            {
                // App Host
                services.AddHostedService<ApplicationHostService>();

                // Services
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<IContentDialogService, ContentDialogService>();

                services.AddSingleton<SqlConnectionService>();
                services.AddSingleton<DBData>();
                services.AddSingleton<StoreDataTransferExecutions>();

                services.AddTransient<StoreDataFetchService>();

                // Registering IFunctionality with StoreDataTransferFunctionality implementation
                services.AddSingleton<IFunctionality>(provider =>
                    new StoreDataTransferFunctionality(
                        "Store Data Manual Transfer",
                        "Transfer Data",
                        provider.GetRequiredService<IContentDialogService>(),
                        provider.GetRequiredService<StoreDataFetchService>()
                    )
                );

                // Register SchedulerService and resolve its dependencies correctly
                services.AddHostedService<SchedulerService>(provider =>
                    new SchedulerService(
                        provider.GetRequiredService<DBData>(),
                        provider.GetRequiredService<SqlConnectionService>(),
                        provider.GetRequiredService<StoreDataTransferExecutions>(),
                        provider.GetRequiredService<IFunctionality>() as StoreDataTransferFunctionality
                    )
                );

                // ViewModels
                services.AddTransient<MainViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
                // services.AddTransient<Views.Item>();

            }).Build();



        public App()
        {
            Connection.InitializeDatabase();
            CultureInfo vCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = vCulture;
            Thread.CurrentThread.CurrentUICulture = vCulture;
            CultureInfo.DefaultThreadCurrentCulture = vCulture;
            CultureInfo.DefaultThreadCurrentUICulture = vCulture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement), new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _host.Start();
        }

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            var service = _host.Services.GetService(typeof(T)) as T;
            return service!;
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            _host.StopAsync().Wait();

            _host.Dispose();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

    }
}
