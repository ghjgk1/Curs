using Microsoft.Extensions.DependencyInjection;
using PharmacyWarehouse.Models;
using PharmacyWarehouse.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Логика взаимодействия для AddEditProductWindow.xaml
    /// </summary>
    public partial class AddEditProductWindow : Window
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private bool _isEditMode = false;

        private Product _product;
        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
            }
        }

        public ObservableCollection<Category> Categories { get; set; }
        public ObservableCollection<string> ReleaseForms { get; set; }
        public ObservableCollection<string> Units { get; set; }

        public string Title => _isEditMode ? $"Редактирование: {Product?.Name}" : "Новый товар";

        public event PropertyChangedEventHandler PropertyChanged;

        public AddEditProductWindow(Product product = null)
        {
            InitializeComponent();
            DataContext = this;

            _productService = App.ServiceProvider.GetService<ProductService>();
            _categoryService = App.ServiceProvider.GetService<CategoryService>();

            InitializeLists();

            // Режим редактирования
            if (product != null && product.Id > 0)
            {
                _isEditMode = true;
                var existingProduct = _productService.GetById(product.Id);
                if (existingProduct != null)
                {
                    Product = new Product
                    {
                        Id = existingProduct.Id,
                        Name = existingProduct.Name,
                        CategoryId = existingProduct.CategoryId,
                        ReleaseForm = existingProduct.ReleaseForm,
                        Manufacturer = existingProduct.Manufacturer,
                        UnitOfMeasure = existingProduct.UnitOfMeasure,
                        MinRemainder = existingProduct.MinRemainder,
                        RequiresPrescription = existingProduct.RequiresPrescription,
                        Description = existingProduct.Description
                    };
                }
                else
                {
                    Product = new Product();
                }
            }
            else
            {
                Product = new Product
                {
                    MinRemainder = 10,
                };
            }
        }

        private void InitializeLists()
        {
            // Категории
            Categories = new ObservableCollection<Category>(_categoryService.Categories);

            // Предопределенные формы выпуска
            ReleaseForms = new ObservableCollection<string>
            {
                "Таблетки",
                "Капсулы",
                "Сироп",
                "Мазь",
                "Крем",
                "Инъекции",
                "Капли",
                "Спрей",
                "Порошок",
                "Раствор",
                "Суспензия",
                "Гель",
                "Пластырь",
                "Суппозитории"
            };

            // Единицы измерения
            Units = new ObservableCollection<string>
            {
                "шт.",
                "упак.",
                "фл.",
                "тюб.",
                "бан.",
                "мл",
                "г",
                "мг",
                "л",
                "кг"
            };
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(Product.Name))
            {
                MessageBox.Show("Введите название товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (Product.MinRemainder < 0)
            {
                MessageBox.Show("Минимальный остаток не может быть отрицательным", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    _productService.Update(Product);
                }
                else
                {
                    _productService.Add(Product);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
