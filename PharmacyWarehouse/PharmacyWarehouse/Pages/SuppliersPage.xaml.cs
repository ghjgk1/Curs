using Microsoft.Extensions.DependencyInjection;
using PharmacyWarehouse.Models;
using PharmacyWarehouse.Services;
using PharmacyWarehouse.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PharmacyWarehouse.Pages
{
    /// <summary>
    /// Логика взаимодействия для SuppliersPage.xaml
    /// </summary>
    public partial class SuppliersPage : Page
    {
        public SupplierService Service { get; set; }
        public Supplier SearchSupplier { get; set; } = new Supplier();
        public Supplier SelectedSupplier { get; set; }
        public int SuppliersCount => Service?.Suppliers?.Count ?? 0;
        private List<Supplier> _allSuppliers = new List<Supplier>();

        public SuppliersPage()
        {
            InitializeComponent();
            Service = App.ServiceProvider.GetService<SupplierService>();

            Loaded += SuppliersPage_Loaded;
        }

        private void SuppliersPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                Service.GetAll();
                _allSuppliers = Service.Suppliers.ToList();

                DataContext = this;

                UpdateDataGrid();

                ApplyFilters();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateDataGrid()
        {
            try
            {
                dgSuppliers.ItemsSource = null;
                dgSuppliers.ItemsSource = Service.Suppliers;
                dgSuppliers.UpdateLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении таблицы: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSuppliers.SelectedItem is Supplier selected)
            {
                SelectedSupplier = selected;
                UpdateSelectedSupplierInfo();
            }
        }

        #region CRUD Operations

        private void AddSupplier_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditSupplierWindow();
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void EditSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Выберите поставщика для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new AddEditSupplierWindow(SelectedSupplier);
            if (dialog.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void DeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Выберите поставщика для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Проверка на наличие связанных данных
            if (SelectedSupplier.Batches.Any() || SelectedSupplier.Documents.Any())
            {
                MessageBox.Show("Нельзя удалить поставщика, у которого есть связанные партии или документы.",
                    "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Удалить поставщика '{SelectedSupplier.Name}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Service.Remove(SelectedSupplier);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgSuppliers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSupplier_Click(sender, e);
        }

        #endregion

        #region Filtering Methods

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            try
            {
                var filteredSuppliers = _allSuppliers.AsEnumerable();

                // Фильтр по поиску
                if (!string.IsNullOrWhiteSpace(SearchSupplier?.Name))
                {
                    string searchTerm = SearchSupplier.Name.ToLower();
                    filteredSuppliers = filteredSuppliers.Where(s =>
                        s.Name.ToLower().Contains(searchTerm) ||
                        (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(searchTerm)) ||
                        (s.Phone != null && s.Phone.ToLower().Contains(searchTerm)));
                }

                // Фильтр по активности
                if (cmbStatus.SelectedItem is ComboBoxItem statusItem)
                {
                    string statusText = statusItem.Content.ToString();

                    filteredSuppliers = statusText switch
                    {
                        "Активные" => filteredSuppliers.Where(s => s.IsActive),
                        "Неактивные" => filteredSuppliers.Where(s => !s.IsActive),
                        "С партиями" => filteredSuppliers.Where(s => s.Batches.Any()),
                        "Без партий" => filteredSuppliers.Where(s => !s.Batches.Any()),
                        _ => filteredSuppliers
                    };
                }

                // Преобразуем в список
                var filteredList = filteredSuppliers.ToList();

                // Обновляем коллекцию в сервисе
                Service.Suppliers.Clear();
                foreach (var supplier in filteredList)
                {
                    Service.Suppliers.Add(supplier);
                }

                // Обновляем DataGrid
                UpdateDataGrid();

                // Обновляем статистику
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbStatus != null)
                {
                    cmbStatus.SelectedIndex = 0;
                }

                if (SearchSupplier != null)
                {
                    SearchSupplier.Name = string.Empty;
                }

                var bindingExpression = GetBindingExpression(DataContextProperty);
                if (bindingExpression != null)
                {
                    bindingExpression.UpdateTarget();
                }

                LoadData();

                MessageBox.Show("Фильтры сброшены", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе фильтров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyFilters();
            }
        }

        #endregion

        #region Statistics

        private void UpdateStatistics()
        {
            try
            {
                var displayedSuppliers = Service.Suppliers;
                int total = displayedSuppliers.Count;
                int active = displayedSuppliers.Count(s => s.IsActive);
                int withBatches = displayedSuppliers.Count(s => s.Batches.Any());
                int withDocuments = displayedSuppliers.Count(s => s.Documents.Any());

                txtTotalSuppliers.Text = total.ToString();
                txtActiveCount1.Text = $"Всего: {total}";
                txtActiveSuppliers.Text = $"Активных: {active}";
                txtWithBatches.Text = $"С партиями: {withBatches}";
                txtWithDocuments.Text = $"С документами: {withDocuments}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статистики: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Events for UI Elements

        private void UpdateSelectedSupplierInfo()
        {
            if (SelectedSupplier == null)
            {
                txtSelectedSupplier.Text = "Не выбран поставщик";
                txtSupplierPhone.Text = "Телефон: -";
                txtSupplierAddress.Text = "Адрес: -";

                // Очищаем таблицу последних поставок
                dgRecentDeliveries.ItemsSource = null;
                return;
            }

            // Обновляем контактную информацию
            txtSelectedSupplier.Text = SelectedSupplier.Name;
            txtSupplierPhone.Text = $"Телефон: {SelectedSupplier.Phone ?? "-"}";
            txtSupplierAddress.Text = $"Адрес: {SelectedSupplier.Address ?? "-"}";

            // Обновляем последние поставки
            UpdateRecentDeliveries();
        }

        private void UpdateRecentDeliveries()
        {
            if (SelectedSupplier == null || SelectedSupplier.Documents == null)
            {
                dgRecentDeliveries.ItemsSource = null;
                return;
            }

            var recentDeliveries = SelectedSupplier.Documents
                .OrderByDescending(d => d.Date)
                .Take(5)
                .Select(d => new
                {
                    Date = d.Date,
                    DocumentNumber = d.Number ?? "Без номера",
                    Amount = d.Amount,
                    Status = GetDocumentStatus(d)
                })
                .ToList();

            dgRecentDeliveries.ItemsSource = recentDeliveries.Any() ? recentDeliveries : null;

            if (!recentDeliveries.Any())
            {
                dgRecentDeliveries.ItemsSource = new List<object>
        {
            new { Date = "", DocumentNumber = "Нет поставок", Amount = 0m, Status = "" }
        };
            }
        }

        private string GetDocumentStatus(Document document)
        {
            return "Завершено"; // Временная заглушка
        }

        #endregion

        #endregion

        #region Additional Features

        private void ViewBatches_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Выберите поставщика для просмотра партий", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show($"Просмотр партий поставщика: {SelectedSupplier.Name}\nФункция будет реализована позже",
                "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewDocuments_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Выберите поставщика для просмотра документов", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBox.Show($"Просмотр документов поставщика: {SelectedSupplier.Name}\nФункция будет реализована позже",
                "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
