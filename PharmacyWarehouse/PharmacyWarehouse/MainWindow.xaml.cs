using Microsoft.Extensions.DependencyInjection;
using PharmacyWarehouse.Models;
using PharmacyWarehouse.Pages;
using PharmacyWarehouse.Services;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;


namespace PharmacyWarehouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SystemInfoService _systemInfo;
        private readonly DispatcherTimer _statusTimer = new();

        public MainWindow()
        {
            InitializeComponent();
            _serviceProvider = App.ServiceProvider;
            _systemInfo = BaseDbService.SystemInfo;
            DataContext = _systemInfo;
            LoadDefaultPage();
            StartStatusUpdater();
        }
        private void StartStatusUpdater()
        {
            _statusTimer.Interval = TimeSpan.FromSeconds(10);
            _statusTimer.Tick += (s, e) => _systemInfo.UpdateInfo();
            _statusTimer.Start();
        }
        private void LoadDefaultPage()
        {
            var productsPage = _serviceProvider.GetService<ProductsPage>();
            MainFrame.Navigate(productsPage);
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            var productsPage = _serviceProvider.GetService<ProductsPage>();
            MainFrame.Navigate(productsPage);
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<CategoriesPage>();
            MainFrame.Navigate(page);
        }

        private void Suppliers_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<SuppliersPage>();
            MainFrame.Navigate(page);
        }

        private void Batches_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<BatchesPage>();
            MainFrame.Navigate(page);
        }

        private void Documents_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<DocumentsPage>();
            MainFrame.Navigate(page);
        }
        private void SalesReport_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<SalesReportPage>();
            MainFrame.Navigate(page);
        }

        private void Receipt_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<ReceiptPage>();
            MainFrame.Navigate(page);
        }
        private void Backup_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<BackupPage>();
            MainFrame.Navigate(page);
        }
        private void MovementReport_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<MovementReportPage>();
            MainFrame.Navigate(page);
        }

        private void WriteOff_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<WriteOffPage>();
            MainFrame.Navigate(page);
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<InventoryPage>();
            MainFrame.Navigate(page);
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<ReportsPage>();
            MainFrame.Navigate(page);
        }
        private void Stock_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<StockPage>();
            MainFrame.Navigate(page);
        }

        private void Notifications_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<NotificationsPage>();
            MainFrame.Navigate(page);
            _systemInfo.ResetNotificationCount();
        }

        private void ExpirationControl_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<ExpirationControlPage>();
            MainFrame.Navigate(page);
        }
        private void ExpiredReport_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<ExpiredReportPage>();
            MainFrame.Navigate(page);
        }

        private void AuditLog_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<AuditLogPage>();
            MainFrame.Navigate(page);
        }

        private void Transfer_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<TransferPage>();
            MainFrame.Navigate(page);
        }
        private void SupplierReport_Click(object sender, RoutedEventArgs e)
        {
            var page = _serviceProvider.GetService<SupplierReportPage>();
            MainFrame.Navigate(page);
        }
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Поиск товаров...")
            {
                SearchTextBox.Text = "";
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Поиск товаров...";
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string searchText = SearchTextBox.Text;

                if (!string.IsNullOrWhiteSpace(searchText) && searchText != "Поиск товаров...")
                {
                    var productsPage = _serviceProvider.GetService<ProductsPage>();
                    if (productsPage != null)
                    {
                        productsPage.SearchProduct = new Product { Name = searchText };
                        MainFrame.Navigate(productsPage);
                    }
                }

                SearchTextBox.Text = "Поиск товаров...";
                Keyboard.ClearFocus();
            }
        }
    }
}