using Microsoft.Extensions.DependencyInjection;
using PharmacyWarehouse.Pages;
using PharmacyWarehouse.Services;
using System.Windows;
using System.Windows.Documents;

namespace PharmacyWarehouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Регистрируем сервисы
            services.AddSingleton<ProductService>();
            services.AddSingleton<CategoryService>();
            services.AddSingleton<SupplierService>();
            services.AddSingleton<BatchService>();
            services.AddSingleton<DocumentService>();
            services.AddSingleton<NotificationService>();
            services.AddSingleton<InventoryService>();
            services.AddSingleton<WriteOffService>();

            // Регистрируем окна/страницы
            services.AddSingleton<MainWindow>();
            services.AddTransient<AuditLogPage>();
            services.AddTransient<BackupPage>();
            services.AddTransient<BatchesPage>();
            services.AddTransient<CategoriesPage>();
            services.AddTransient<DocumentsPage>();
            services.AddTransient<ExpirationControlPage>();
            services.AddTransient<ExpiredReportPage>();
            services.AddTransient<InventoryPage>();
            services.AddTransient<MovementReportPage>();
            services.AddTransient<NotificationsPage>();
            services.AddTransient<ProductsPage>();
            services.AddTransient<ReceiptPage>();
            services.AddTransient<ReportsPage>();
            services.AddTransient<SalesReportPage>();
            services.AddTransient<StockPage>();
            services.AddTransient<SuppliersPage>(); 
            services.AddTransient<SupplierReportPage>(); 
            services.AddTransient<TransferPage>();
            services.AddTransient<WriteOffPage>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetService<MainWindow>();
            mainWindow?.Show();

            base.OnStartup(e);
        }
    }
}
