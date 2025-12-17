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
    /// Логика взаимодействия для AddEditSupplierWindow.xaml
    /// </summary>
    public partial class AddEditSupplierWindow : Window
    {
            public Supplier Supplier { get; set; }
            public string WindowTitle { get; set; }
            private SupplierService _service;

            public AddEditSupplierWindow()
            {
                InitializeComponent();
                Supplier = new Supplier();
                WindowTitle = "Добавить поставщика";
                _service = App.ServiceProvider.GetService<SupplierService>();
                DataContext = this;
            }

            public AddEditSupplierWindow(Supplier supplier)
            {
                InitializeComponent();
                Supplier = supplier;
                WindowTitle = "Редактировать поставщика";
                _service = App.ServiceProvider.GetService<SupplierService>();
                DataContext = this;
            }

            private void Save_Click(object sender, RoutedEventArgs e)
            {
                if (string.IsNullOrWhiteSpace(Supplier.Name))
                {
                    MessageBox.Show("Название поставщика обязательно для заполнения",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    if (Supplier.Id == 0) // Новый поставщик
                    {
                        _service.Add(Supplier);
                    }
                    else // Редактирование
                    {
                        var existing = _service.GetById(Supplier.Id);
                        if (existing != null)
                        {
                            existing.Name = Supplier.Name;
                            existing.Phone = Supplier.Phone;
                            existing.ContactPerson = Supplier.ContactPerson;
                            existing.Address = Supplier.Address;

                            _service.Commit();
                        }
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
        }
    }
