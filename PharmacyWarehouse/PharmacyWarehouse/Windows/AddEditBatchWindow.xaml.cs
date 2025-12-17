using Microsoft.Extensions.DependencyInjection;
using PharmacyWarehouse.Models;
using PharmacyWarehouse.Services;
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
using System.Windows.Shapes;

namespace PharmacyWarehouse.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddEditBatchWindow.xaml
    /// </summary>
    public partial class AddEditBatchWindow : Window
    {
        private readonly BatchService _batchService;
        private readonly ProductService _productService;
        private readonly SupplierService _supplierService;
        private readonly Batch _batch;
        private bool _isEditMode = false;


        public AddEditBatchWindow(Batch batch = null)
        {
            InitializeComponent();

            _batchService = App.ServiceProvider.GetService<BatchService>();
            _productService = App.ServiceProvider.GetService<ProductService>();
            _supplierService = App.ServiceProvider.GetService<SupplierService>();

            if (batch != null)
            {
                _batch = batch;
                _isEditMode = true;

                // Создаем новый объект для редактирования
                _batch = new Batch
                {
                    Id = batch.Id,
                    BatchNumber = batch.BatchNumber,
                    ProductId = batch.ProductId,
                    SupplierId = batch.SupplierId,
                    ExpirationDate = batch.ExpirationDate,
                    PurchasePrice = batch.PurchasePrice,
                    SellingPrice = batch.SellingPrice,
                    Quantity = batch.Quantity,
                    ArrivalDate = batch.ArrivalDate,
                    IsActive = batch.IsActive
                };
            }
            else
            {
                _batch = new Batch();
            }

            DataContext = _batch;

            LoadData();

            if (_isEditMode)
            {
                LoadBatchData();
            }

                GenerateBatchNumber();
        }

        private void GenerateBatchNumber()
        {
            if (_batch.Id == 0)
            {
                string batchNumber = $"П-{DateTime.Now:yyyyMMdd-HHmmss}";

                txtBatchNumber.Text = batchNumber;
            }
        }

        private void LoadData()
        {
            _productService.GetAll();
            _supplierService.GetAll();

            cmbProduct.ItemsSource = _productService.Products;
            cmbSupplier.ItemsSource = _supplierService.Suppliers;

            dpArrivalDate.SelectedDate = DateTime.Today;
            dpExpirationDate.SelectedDate = DateTime.Today.AddYears(2);
        }

        private void LoadBatchData()
        {
            try
            {
                // Загружаем полные данные партии из БД
                var fullBatch = _batchService.GetById(_batch.Id);
                if (fullBatch != null)
                {
                    _batch.Product = fullBatch.Product;
                    _batch.Supplier = fullBatch.Supplier;

                    cmbProduct.GetBindingExpression(ComboBox.SelectedValueProperty)?.UpdateTarget();
                    cmbSupplier.GetBindingExpression(ComboBox.SelectedValueProperty)?.UpdateTarget();

                    txtBatchNumber.Text = _batch.BatchNumber;
                    txtBatchNumber.IsReadOnly = true;
                    txtQuantity.Text = _batch.Quantity.ToString();
                    txtPurchasePrice.Text = _batch.PurchasePrice.ToString("N2");
                    txtSellingPrice.Text = _batch.SellingPrice.ToString("N2");

                    dpArrivalDate.SelectedDate = _batch.ArrivalDate?.ToDateTime(TimeOnly.MinValue);
                    dpExpirationDate.SelectedDate = _batch.ExpirationDate.ToDateTime(TimeOnly.MinValue);

                    chkIsActive.IsChecked = _batch.IsActive;
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить данные партии", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных партии: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var selectedProduct = cmbProduct.SelectedItem as Product;
                var selectedSupplier = cmbSupplier.SelectedItem as Supplier;

                if (_batch.Id == 0) // Новая партия
                {
                    var newBatch = new Batch
                    {
                        BatchNumber = txtBatchNumber.Text,
                        ProductId = selectedProduct.Id,
                        SupplierId = selectedSupplier.Id,
                        Quantity = int.Parse(txtQuantity.Text),
                        PurchasePrice = decimal.Parse(txtPurchasePrice.Text),
                        SellingPrice = decimal.Parse(txtSellingPrice.Text),
                        ArrivalDate = dpArrivalDate.SelectedDate.HasValue ?
                            DateOnly.FromDateTime(dpArrivalDate.SelectedDate.Value) :
                            DateOnly.FromDateTime(DateTime.Today),
                        ExpirationDate = DateOnly.FromDateTime(dpExpirationDate.SelectedDate.Value),
                        IsActive = chkIsActive.IsChecked ?? true
                    };

                    _batchService.Add(newBatch);
                }
                else // Редактирование
                {
                    _batch.BatchNumber = txtBatchNumber.Text;
                    _batch.ProductId = selectedProduct.Id;
                    _batch.SupplierId = selectedSupplier.Id;
                    _batch.Quantity = int.Parse(txtQuantity.Text);
                    _batch.PurchasePrice = decimal.Parse(txtPurchasePrice.Text);
                    _batch.SellingPrice = decimal.Parse(txtSellingPrice.Text);
                    _batch.ArrivalDate = dpArrivalDate.SelectedDate.HasValue ?
                        DateOnly.FromDateTime(dpArrivalDate.SelectedDate.Value) :
                        _batch.ArrivalDate;
                    _batch.ExpirationDate = DateOnly.FromDateTime(dpExpirationDate.SelectedDate.Value);
                    _batch.IsActive = chkIsActive.IsChecked ?? true;

                    _batchService.Update(_batch);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtBatchNumber.Text))
            {
                MessageBox.Show("Введите номер партии", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (cmbProduct.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (cmbSupplier.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(txtPurchasePrice.Text, out decimal purchasePrice) || purchasePrice <= 0)
            {
                MessageBox.Show("Введите корректную цену закупки", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!decimal.TryParse(txtSellingPrice.Text, out decimal sellingPrice) || sellingPrice <= 0)
            {
                MessageBox.Show("Введите корректную цену продажи", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!dpExpirationDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите срок годности", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text + e.Text;

            e.Handled = !decimal.TryParse(newText, out _);
        }
    }
}
