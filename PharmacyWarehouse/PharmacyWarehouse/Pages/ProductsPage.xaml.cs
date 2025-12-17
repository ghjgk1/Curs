using Microsoft.Extensions.DependencyInjection;
using PharmacyWarehouse.Models;
using PharmacyWarehouse.Services;
using PharmacyWarehouse.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PharmacyWarehouse.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        public ProductService service { get; set; }
        public Product SearchProduct { get; set; } = new Product();
        public Product SelectedProduct { get; set; }
        public int ProductsCount => service?.Products?.Count ?? 0;
        private CategoryService _categoryService;
        private List<Product> _allProducts = new List<Product>();

        public ProductsPage()
        {
            InitializeComponent();
            service = App.ServiceProvider.GetService<ProductService>();
            _categoryService = App.ServiceProvider.GetService<CategoryService>();

            service.GetAll();
            _categoryService.GetAll();

            Loaded += ProductsPage_Loaded;
        }

        private void ProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                service.GetAll();
                _allProducts = service.Products.ToList();
                
                LoadCategories();

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
                dgProducts.ItemsSource = null;

                dgProducts.ItemsSource = service.Products;

                dgProducts.UpdateLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении таблицы: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCategories()
        {
            try
            {
                cmbCategory.Items.Clear();

                var allCategoriesItem = new { Id = 0, Name = "Все категории" };
                cmbCategory.Items.Add(allCategoriesItem);

                _categoryService.GetAll();

                foreach (var category in _categoryService.Categories)
                {
                    cmbCategory.Items.Add(category);
                }

                cmbCategory.DisplayMemberPath = "Name";

                cmbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void DgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selected)
            {
                SelectedProduct = selected;
            }
        }

        #region CRUD Operations

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditProductWindow();
            if (dialog.ShowDialog() == true)
            {
                _categoryService.GetAll();
                LoadData();
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Выберите товар для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new AddEditProductWindow(SelectedProduct);
            if (dialog.ShowDialog() == true)
            {
                _categoryService.GetAll();
                LoadData();
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Выберите товар для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedProduct.Batches.Any())
            {
                MessageBox.Show("Нельзя удалить товар, у которого есть партии.\nСначала удалите все партии этого товара.",
                    "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Удалить товар '{SelectedProduct.Name}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    service.Remove(SelectedProduct);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DgProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditProduct_Click(sender, e);
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
                var filteredProducts = _allProducts.AsEnumerable();

                // Фильтр по поиску
                if (!string.IsNullOrWhiteSpace(SearchProduct?.Name))
                {
                    string searchTerm = SearchProduct.Name.ToLower();
                    filteredProducts = filteredProducts.Where(p =>
                        p.Name.ToLower().Contains(searchTerm) ||
                        (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(searchTerm)) ||
                        (p.ReleaseForm != null && p.ReleaseForm.ToLower().Contains(searchTerm)));
                }

                // Фильтр по категории
                if (cmbCategory.SelectedItem != null)
                {
                    var selectedItem = cmbCategory.SelectedItem;

                    var type = selectedItem.GetType();

                    var idProperty = type.GetProperty("Id");

                    if (idProperty != null)
                    {
                        var idValue = idProperty.GetValue(selectedItem);

                        if (idValue != null && idValue is int categoryId)
                        {
                            if (categoryId > 0) 
                            {
                                filteredProducts = filteredProducts.Where(p => p.CategoryId == categoryId);
                            }
                        }
                    }
                    else if (selectedItem is Category category)
                    {
                        filteredProducts = filteredProducts.Where(p => p.CategoryId == category.Id);
                    }
                }

                // Фильтр по статусу
                if (cmbStatus.SelectedItem is ComboBoxItem statusItem)
                {
                    string statusText = statusItem.Content.ToString();

                    filteredProducts = statusText switch
                    {
                        "С низким остатком" => filteredProducts.Where(p => p.IsLowStock),
                        "Нет в наличии" => filteredProducts.Where(p => p.IsOutOfStock),
                        "Истекает срок" => filteredProducts.Where(p => p.HasExpiringBatches),
                        "Просроченные" => filteredProducts.Where(p => p.HasExpiredBatches),
                        _ => filteredProducts
                    };
                }

                // Преобразуем в список
                var filteredList = filteredProducts.ToList();

                // Обновляем коллекцию в сервисе
                service.Products.Clear();
                foreach (var product in filteredList)
                {
                    service.Products.Add(product);
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
                if (cmbCategory != null)
                {
                    var allCategoriesItem = cmbCategory.Items
                                    .OfType<ComboBoxItem>()
                                    .FirstOrDefault(item => item.Content.ToString() == "Все категории");

                    if (allCategoriesItem != null)
                    {
                        cmbCategory.SelectedItem = allCategoriesItem;
                    }
                    else
                    {
                        cmbCategory.SelectedIndex = 0;
                    }
                }

                if (cmbStatus != null)
                {
                    cmbStatus.SelectedIndex = 0; 
                }

                if (SearchProduct != null)
                {
                    SearchProduct.Name = string.Empty;
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

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
            {
                ApplyFilters();
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

        #region Search Methods (обратная совместимость)

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplySearch();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplySearch();
        }

        private void ApplySearch()
        {
            ApplyFilters();
        }

        private void ApplyStatusFilter()
        {
            ApplyFilters();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        #endregion

        #region Statistics

        private void UpdateStatistics()
        {
            try
            {
                var displayedProducts = service.Products;
                int total = displayedProducts.Count;
                int lowStock = displayedProducts.Count(p => p.IsLowStock);
                int outOfStock = displayedProducts.Count(p => p.IsOutOfStock);
                int expiring = displayedProducts.Count(p => p.HasExpiringBatches);
                int expired = displayedProducts.Count(p => p.HasExpiredBatches);
                decimal totalValue = displayedProducts.Sum(p => p.TotalValue);

                txtTotalProducts.Text = total.ToString();
                txtTotalProducts.Text = $"{totalValue:N2} шт.";

                txtActiveCount.Text = $"Найдено: {total}";
                txtLowStockCount.Text = $"Низкий остаток: {lowStock}";
                txtExpiringCount.Text = $"Истекает срок: {expiring}";

                // Обновляем счетчик просроченных
                var txtExpiredCount = this.FindName("txtExpiredCount") as TextBlock;
                if (txtExpiredCount != null)
                {
                    txtExpiredCount.Text = $"Просрочено: {expired}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статистики: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Additional Features

        //private void StockReport_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var reportWindow = new StockReportWindow(service.Products.ToList());
        //        reportWindow.ShowDialog();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при создании отчета: {ex.Message}",
        //            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //private void Print_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        var printDialog = new PrintDialog();
        //        if (printDialog.ShowDialog() == true)
        //        {
        //            printDialog.PrintVisual(dgProducts, "Список товаров");
        //            MessageBox.Show("Печать отправлена на принтер",
        //                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при печати: {ex.Message}",
        //            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //private void Import_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new Microsoft.Win32.OpenFileDialog
        //    {
        //        Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*",
        //        Title = "Импорт товаров из Excel"
        //    };

        //    if (dialog.ShowDialog() == true)
        //    {
        //        try
        //        {
        //            MessageBox.Show($"Импорт из файла: {dialog.FileName}\nФункция будет реализована позже",
        //                "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Ошибка при импорте: {ex.Message}",
        //                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}

        //private void Export_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new Microsoft.Win32.SaveFileDialog
        //    {
        //        Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
        //        FileName = $"Товары_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
        //        Title = "Экспорт товаров в Excel"
        //    };

        //    if (dialog.ShowDialog() == true)
        //    {
        //        try
        //        {
        //            MessageBox.Show($"Экспорт в файл: {dialog.FileName}\nФункция будет реализована позже",
        //                "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Ошибка при экспорте: {ex.Message}",
        //                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}

        //private void MassUpdatePrices_Click(object sender, RoutedEventArgs e)
        //{
        //    if (service.Products.Count == 0)
        //    {
        //        MessageBox.Show("Нет товаров для обновления",
        //            "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        //        return;
        //    }

        //    var dialog = new MassUpdatePricesWindow(service.Products.ToList());
        //    if (dialog.ShowDialog() == true)
        //    {
        //        LoadData();
        //    }
        //}

        //private void MassUpdateStock_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("Функция массового обновления остатков будет реализована позже",
        //        "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        //}

        //private void GenerateBarcodes_Click(object sender, RoutedEventArgs e)
        //{
        //    if (SelectedProduct == null && service.Products.Count > 0)
        //    {
        //        var result = MessageBox.Show("Не выбран конкретный товар. Сгенерировать штрих-коды для всех товаров?",
        //            "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        //        if (result == MessageBoxResult.Yes)
        //        {
        //            MessageBox.Show($"Генерация штрих-кодов для {service.Products.Count} товаров\nФункция будет реализована позже",
        //                "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //    }
        //    else if (SelectedProduct != null)
        //    {
        //        MessageBox.Show($"Генерация штрих-кода для товара: {SelectedProduct.Name}\nФункция будет реализована позже",
        //            "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //}

        #endregion

    }
}
