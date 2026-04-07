using InventorySystem.Interface;
using InventorySystem.Services;
using InventorySystem.View;
using InventorySystem.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceCollection services = new ServiceCollection();
        private ServiceProvider _serviceProvider;

        public App()
        {
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<ICSVService, CSVService>();

            services.AddSingleton<SpreadSheetViewModel>();
            services.AddSingleton<ISqlQueryBuilder , SqlQueryBuilder>();
            services.AddSingleton<ISelectionService, SelectionService>();
            services.AddSingleton<IStringService, StringService>();
            services.AddSingleton<ISelectionService, SelectionService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IWindowFactory, WindowFactory>();

            services.AddTransient<AddViewModel>();
            services.AddTransient<AddWindow>();
            services.AddTransient<UpdateViewModel>();
            services.AddTransient<UpdateWindow>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider.Dispose();
            base.OnExit(e);
        }
    }
}