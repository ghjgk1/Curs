using System.Collections.ObjectModel;
using PharmacyWarehouse.Models;
using Microsoft.EntityFrameworkCore;
using PharmacyWarehouse.Data;

namespace PharmacyWarehouse.Services
{
    public class BatchService
    {
        private readonly PharmacyWarehouseContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Batch> Batches { get; set; } = new();

        public BatchService()
        {
            GetAll();
        }

        public int Commit() => _db.SaveChanges();

        public void Add(Batch batch)
        {
            var _batch = new Batch
            {
                ProductId = batch.ProductId,
                SupplierId = batch.SupplierId,
                BatchNumber = batch.BatchNumber,
                ExpirationDate = batch.ExpirationDate,
                PurchasePrice = batch.PurchasePrice,
                SellingPrice = batch.SellingPrice,
                Quantity = batch.Quantity,
                ArrivalDate = batch.ArrivalDate ?? DateOnly.FromDateTime(DateTime.Today)
            };

            _db.Batches.Add(_batch);
            if (Commit() > 0)
            {
                Batches.Add(_batch);
                CheckStockNotifications(batch.ProductId);
            }
        }

        public void GetAll()
        {
            var batches = _db.Batches
                .Include(b => b.Product)
                .Include(b => b.Supplier)
                .ToList();

            Batches.Clear();
            foreach (var batch in batches)
            {
                Batches.Add(batch);
            }
        }

        public void Remove(Batch batch)
        {
            var existing = _db.Batches.Find(batch.Id);
            if (existing != null)
            {
                int productId = existing.ProductId;

                _db.Batches.Remove(existing);
                if (Commit() > 0)
                {
                    if (Batches.Contains(batch))
                        Batches.Remove(batch);

                    CheckStockNotifications(productId);
                }
            }
        }

        public Batch? GetById(int id)
        {
            return _db.Batches
                .Include(b => b.Product)
                .Include(b => b.Supplier)
                .FirstOrDefault(b => b.Id == id);
        }

        public List<Batch> GetByProduct(int productId)
        {
            return _db.Batches
                .Where(b => b.ProductId == productId)
                .Include(b => b.Supplier)
                .OrderBy(b => b.ExpirationDate)
                .ToList();
        }

        private void CheckStockNotifications(int productId)
        {
            var product = _db.Products
                .Include(p => p.Batches)
                .FirstOrDefault(p => p.Id == productId);

            if (product != null && product.MinRemainder.HasValue)
            {
                int totalStock = product.Batches.Sum(b => b.Quantity);
                if (totalStock <= product.MinRemainder)
                {
                    // Можно добавить логику уведомлений здесь
                    // Например, создание Notification через NotificationService
                }
            }
        }

        public void Update(Batch batch)
        {
            var existing = _db.Batches
                .Include(b => b.Product)
                .Include(b => b.Supplier)
                .FirstOrDefault(b => b.Id == batch.Id);

            if (existing != null)
            {
                // Сохраняем старый ProductId для проверки уведомлений
                int oldProductId = existing.ProductId;

                existing.ProductId = batch.ProductId;
                existing.SupplierId = batch.SupplierId;
                existing.BatchNumber = batch.BatchNumber;
                existing.ExpirationDate = batch.ExpirationDate;
                existing.PurchasePrice = batch.PurchasePrice;
                existing.SellingPrice = batch.SellingPrice;
                existing.Quantity = batch.Quantity;
                existing.ArrivalDate = batch.ArrivalDate;
                existing.IsActive = batch.IsActive;

                _db.SaveChanges();

                // Проверяем уведомления для старого и нового товара
                if (oldProductId != batch.ProductId)
                {
                    CheckStockNotifications(oldProductId);
                }
                CheckStockNotifications(batch.ProductId);
            }
        }

        public ObservableCollection<Batch> GetExpiredBatches()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var batches = _db.Batches
                .Where(b => b.ExpirationDate <= today && b.Quantity > 0 && b.IsActive)
                .Include(b => b.Product)
                .Include(b => b.Supplier)
                .ToList();

            var collection = new ObservableCollection<Batch>();
            foreach (var batch in batches)
            {
                collection.Add(batch);
            }
            return collection;
        }

        public ObservableCollection<Batch> GetExpiringBatches(int days = 30)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var warningDate = today.AddDays(days);

            var batches = _db.Batches
                .Where(b => b.ExpirationDate > today &&
                           b.ExpirationDate <= warningDate &&
                           b.Quantity > 0 &&
                           b.IsActive)
                .Include(b => b.Product)
                .Include(b => b.Supplier)
                .ToList();

            var collection = new ObservableCollection<Batch>();
            foreach (var batch in batches)
            {
                collection.Add(batch);
            }
            return collection;
        }

        public ObservableCollection<Batch> GetActiveBatches()
        {
            var batches = _db.Batches
                .Where(b => b.Quantity > 0 && b.IsActive)
                .Include(b => b.Product)
                .Include(b => b.Supplier)
                .ToList();

            var collection = new ObservableCollection<Batch>();
            foreach (var batch in batches)
            {
                collection.Add(batch);
            }
            return collection;
        }

        public decimal GetTotalBatchesValue()
        {
            return _db.Batches
                .Where(b => b.IsActive)
                .Sum(b => b.PurchasePrice * b.Quantity);
        }

        public int GetTotalBatchesCount()
        {
            return _db.Batches.Count(b => b.IsActive);
        }

        public void PartialWriteOff(Batch batch, int quantity)
        {
            var existing = _db.Batches.Find(batch.Id);
            if (existing != null && existing.Quantity >= quantity)
            {
                existing.Quantity -= quantity;
                if (existing.Quantity == 0)
                {
                    existing.IsActive = false;
                }

                _db.SaveChanges();

                CheckStockNotifications(batch.ProductId);

                GetAll();
            }
        }

        public void FullWriteOff(Batch batch)
        {
            PartialWriteOff(batch, batch.Quantity);
        }
    }
}
