using Microsoft.Extensions.DependencyInjection;
using PharmacyWarehouse.Models;
using PharmacyWarehouse.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PharmacyWarehouse.Windows
{
    public partial class WriteOffBatchWindow : Window
    {
        private readonly Batch _batch;
        private readonly BatchService _batchService;
        private readonly DocumentService _documentService;
        private readonly WriteOffService _writeOffService;
        private readonly ProductService _productService;
        private readonly SupplierService _supplierService;

        private int _maxQuantity;
        private decimal _purchasePrice;
        private decimal _sellingPrice;
        private Product _product;
        private Supplier _supplier;

        public WriteOffBatchWindow(Batch batch)
        {
            InitializeComponent();

            if (batch == null)
            {
                MessageBox.Show("Партия не выбрана", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            _batch = batch;
            _batchService = App.ServiceProvider.GetService<BatchService>();
            _documentService = App.ServiceProvider.GetService<DocumentService>();
            _writeOffService = App.ServiceProvider.GetService<WriteOffService>();
            _productService = App.ServiceProvider.GetService<ProductService>();
            _supplierService = App.ServiceProvider.GetService<SupplierService>();

            LoadBatchData();
        }

        private void LoadBatchData()
        {
            try
            {
                // Загружаем связанные данные с базой
                var batchWithDetails = _batchService.GetById(_batch.Id);
                if (batchWithDetails == null)
                {
                    MessageBox.Show("Партия не найдена в базе данных", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // Сохраняем загруженные данные
                _product = batchWithDetails.Product;
                _supplier = batchWithDetails.Supplier;
                _purchasePrice = batchWithDetails.PurchasePrice;
                _maxQuantity = batchWithDetails.Quantity;

                // Заполняем информацию о партии
                txtBatchNumberDisplay.Text = batchWithDetails.BatchNumber;
                txtProductName.Text = _product?.Name ?? "Не указано";
                txtSupplierName.Text = _supplier?.Name ?? "Не указано";
                txtExpirationDate.Text = batchWithDetails.ExpirationDate.ToString("dd.MM.yyyy");

                // Показываем статус срока годности
                UpdateExpirationStatus(batchWithDetails);

                txtAvailableQuantity.Text = _maxQuantity.ToString("N0");

                // Обновляем цену закупки (убираем продажную цену)
                if (txtPurchasePrice != null)
                    txtPurchasePrice.Text = $"{_purchasePrice:N2} руб./шт.";

                // Устанавливаем начальное значение для списания
                if (_maxQuantity > 0)
                {
                    // По умолчанию предлагаем списать все (для просроченных) или 1 единицу
                    txtQuantity.Text = batchWithDetails.IsExpired ? _maxQuantity.ToString() : "1";
                }
                else
                {
                    txtQuantity.Text = "0";
                    btnWriteOff.IsEnabled = false;
                }

                // Генерируем номер документа
                txtDocumentNumber.Text = GenerateDocumentNumber();

                // Проверяем предупреждения
                CheckWarnings();

                // Обновляем расчеты
                UpdateCalculations();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных партии: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void UpdateExpirationStatus(Batch batch)
        {
            if (txtExpirationStatus == null) return;

            if (batch.IsExpired)
            {
                txtExpirationStatus.Text = "(ПРОСРОЧЕНО)";
                txtExpirationStatus.Foreground = System.Windows.Media.Brushes.Red;
                txtExpirationStatus.FontWeight = FontWeights.Bold;
                txtExpirationStatus.ToolTip = "Партия с истекшим сроком годности. Требуется срочное списание!";
            }
            else if (batch.IsExpiringSoon)
            {
                txtExpirationStatus.Text = $"(Истекает через {batch.DaysUntilExpiration} дн.)";
                txtExpirationStatus.Foreground = System.Windows.Media.Brushes.Orange;
                txtExpirationStatus.FontWeight = FontWeights.SemiBold;
                txtExpirationStatus.ToolTip = $"Срок годности истекает {batch.ExpirationDate:dd.MM.yyyy}";
            }
            else
            {
                txtExpirationStatus.Text = "(В норме)";
                txtExpirationStatus.Foreground = System.Windows.Media.Brushes.Green;
                txtExpirationStatus.ToolTip = $"Срок годности до {batch.ExpirationDate:dd.MM.yyyy}";
            }
        }

        private string GenerateDocumentNumber()
        {
            try
            {
                var today = DateTime.Today;
                int count = _documentService.Documents
                    .Count(d => d.Date.HasValue &&
                               d.Date.Value.Year == today.Year &&
                               d.Date.Value.Month == today.Month) + 1;

                return $"СП-{today:yyyyMM}-{count:D3}";
            }
            catch
            {
                return $"СП-{DateTime.Today:yyyyMMdd}-001";
            }
        }

        private void CheckWarnings()
        {
            try
            {
                var batch = _batchService.GetById(_batch.Id);
                if (batch == null) return;

                // Проверка на просрочку
                if (batch.IsExpired)
                {
                    warningBorder.Visibility = Visibility.Visible;
                    txtWarningMessage.Text = "Эта партия просрочена! Списание должно быть выполнено по причине 'Просрочка'.";

                    // Автоматически выбираем причину "Просрочка"
                    foreach (ComboBoxItem item in cmbWriteOffReason.Items)
                    {
                        if (item.Content.ToString() == "Просрочка")
                        {
                            cmbWriteOffReason.SelectedItem = item;
                            break;
                        }
                    }
                }
                // Проверка на истекающий срок
                else if (batch.IsExpiringSoon)
                {
                    warningBorder.Visibility = Visibility.Visible;
                    txtWarningMessage.Text = $"Срок годности истекает через {batch.DaysUntilExpiration} дней.";
                }

                // Проверка на отсутствие остатка
                if (_maxQuantity <= 0)
                {
                    ShowError("В этой партии нет товара для списания");
                    btnWriteOff.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке предупреждений: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCalculations()
        {
            try
            {
                if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
                    return;

                // Ограничиваем максимальным количеством
                if (quantity > _maxQuantity)
                {
                    quantity = _maxQuantity;
                    txtQuantity.Text = quantity.ToString();
                }

                // 1. Сумма списания (убыток)
                decimal writeOffAmount = quantity * _purchasePrice;

                // 2. Остаток после списания
                int remainingAfterWriteOff = _maxQuantity - quantity;

                // 3. Обновляем UI
                if (txtPurchasePrice != null)
                    txtPurchasePrice.Text = $"{_purchasePrice:N2} руб./шт.";

                if (txtWriteOffAmount != null)
                {
                    txtWriteOffAmount.Text = $"{writeOffAmount:N2} руб.";
                    txtWriteOffAmount.Foreground = System.Windows.Media.Brushes.Red;
                    txtWriteOffAmount.ToolTip = $"Общий убыток: {writeOffAmount:N2} руб.";
                }

                if (txtRemainingAfterWriteOff != null)
                    txtRemainingAfterWriteOff.Text = $"{remainingAfterWriteOff:N0} шт.";

                // 4. Обновляем общий итог (переименуйте TextBlock в XAML на txtTotalWriteOff)
                if (txtTotalWriteOff != null)
                {
                    txtTotalWriteOff.Text = $"{writeOffAmount:N2} руб.";
                    txtTotalWriteOff.Foreground = System.Windows.Media.Brushes.Red;
                }

                // 5. Активируем кнопку только если есть что списывать
                if (btnWriteOff != null)
                    btnWriteOff.IsEnabled = quantity > 0 && quantity <= _maxQuantity;

                // 6. Скрываем ошибку если все в порядке
                if (quantity <= _maxQuantity && quantity > 0 && errorBorder != null)
                    errorBorder.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при расчетах: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            try
            {
                errorBorder.Visibility = Visibility.Collapsed;

                // Проверка количества
                if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
                {
                    ShowError("Введите корректное количество для списания (больше 0)");
                    return false;
                }

                if (quantity > _maxQuantity)
                {
                    ShowError($"Нельзя списать больше чем доступно. Максимум: {_maxQuantity} шт.");
                    return false;
                }

                // Проверка номера документа
                if (string.IsNullOrWhiteSpace(txtDocumentNumber.Text))
                {
                    ShowError("Введите номер документа");
                    return false;
                }

                // Проверка причины списания
                if (cmbWriteOffReason.SelectedItem == null)
                {
                    ShowError("Выберите причину списания");
                    return false;
                }

                // Для просроченных партий проверяем причину
                var batch = _batchService.GetById(_batch.Id);
                if (batch != null && batch.IsExpired)
                {
                    string selectedReason = (cmbWriteOffReason.SelectedItem as ComboBoxItem)?.Content?.ToString();
                    if (selectedReason != "Просрочка")
                    {
                        ShowError("Просроченные партии должны списываться по причине 'Просрочка'");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка валидации: {ex.Message}");
                return false;
            }
        }

        private void ShowError(string message)
        {
            txtErrorMessage.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }

        private void WriteOff_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                int quantity = int.Parse(txtQuantity.Text);
                string reason = (cmbWriteOffReason.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Продажа";
                string comment = txtComment.Text;
                string documentNumber = txtDocumentNumber.Text;

                // Создаем документ списания
                var document = new Document
                {
                    Number = documentNumber,
                    Type = "Списание",
                    Date = DateOnly.FromDateTime(DateTime.Today),
                    Counterparty = _supplier?.Name ?? "Не указан",
                    Amount = quantity * _purchasePrice
                };

                // Добавляем документ
                _documentService.Add(document);

                // Получаем созданный документ с Id
                var savedDocument = _documentService.Documents.LastOrDefault(d => d.Number == documentNumber);
                if (savedDocument == null)
                {
                    throw new Exception("Не удалось создать документ");
                }

                // Создаем запись о списании
                var writeOff = new WriteOff
                {
                    DocumentId = savedDocument.Id,
                    Reason = reason,
                    Comment = comment
                };

                _writeOffService.Add(writeOff);

                // Обновляем остаток в партии
                var batchToUpdate = _batchService.GetById(_batch.Id);
                if (batchToUpdate != null)
                {
                    batchToUpdate.Quantity -= quantity;
                    if (batchToUpdate.Quantity == 0)
                    {
                        batchToUpdate.IsActive = false;
                    }
                    _batchService.Update(batchToUpdate);
                }

                // Показываем сообщение об успехе
                MessageBox.Show($"Списание успешно выполнено!\n\n" +
                              $"Документ: {documentNumber}\n" +
                              $"Партия: {_batch.BatchNumber}\n" +
                              $"Товар: {_product?.Name}\n" +
                              $"Списано: {quantity} шт.\n" +
                              $"Стоимость закупки: {quantity * _purchasePrice:N2} руб.\n" +
                              $"Причина: {reason}",
                    "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при списании: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnMaxQuantity_Click(object sender, RoutedEventArgs e)
        {
            txtQuantity.Text = _maxQuantity.ToString();
            UpdateCalculations();
        }

        private void TxtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCalculations();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }

            // Проверяем, чтобы после ввода получилось валидное число
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            if (!string.IsNullOrEmpty(newText) && !int.TryParse(newText, out int result))
            {
                e.Handled = true;
            }
        }

        private void TxtDocumentNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtDocumentNumber.Text == "Авто-генерация...")
            {
                txtDocumentNumber.Text = "";
            }
        }

        private void TxtDocumentNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDocumentNumber.Text))
            {
                txtDocumentNumber.Text = GenerateDocumentNumber();
            }
        }
    }
}