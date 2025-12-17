using System.Collections.ObjectModel;
using PharmacyWarehouse.Models;
using Microsoft.EntityFrameworkCore;
using PharmacyWarehouse.Data;

namespace PharmacyWarehouse.Services
{
    public class ProductService
    {
        private readonly PharmacyWarehouseContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Product> Products { get; set; } = new();

        public ProductService()
        {
            GetAll();
        }

        public int Commit() => _db.SaveChanges();

        public void Add(Product product)
        {
            var _product = new Product
            {
                Name = product.Name,
                CategoryId = product.CategoryId,
                ReleaseForm = product.ReleaseForm,
                Manufacturer = product.Manufacturer,
                UnitOfMeasure = product.UnitOfMeasure,
                MinRemainder = product.MinRemainder,
                RequiresPrescription = product.RequiresPrescription,
                Description = product.Description
            };

            _db.Products.Add(_product);
            if (Commit() > 0)
                Products.Add(_product);
        }
        public List<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Products.ToList();

            return _db.Products
                .Where(p => p.Name.Contains(searchTerm) ||
                           (p.Manufacturer != null && p.Manufacturer.Contains(searchTerm)))
                .ToList();
        }
        public void GetAll()
        {
            var products = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Batches)
                .ToList();

            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        public void Remove(Product product)
        {
            var existing = _db.Products.Find(product.Id);
            if (existing != null)
            {
                _db.Products.Remove(existing);
                if (Commit() > 0)
                    if (Products.Contains(product))
                        Products.Remove(product);
            }
        }

        public Product? GetById(int id)
        {
            return _db.Products
                .Include(p => p.Category)
                .Include(p => p.Batches)
                .FirstOrDefault(p => p.Id == id);
        }
        public void Update(Product product)
        {
            var existing = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Batches)
                .FirstOrDefault(p => p.Id == product.Id);

            if (existing != null)
            {
                existing.Name = product.Name;
                existing.CategoryId = product.CategoryId;
                existing.ReleaseForm = product.ReleaseForm;
                existing.Manufacturer = product.Manufacturer;
                existing.UnitOfMeasure = product.UnitOfMeasure;
                existing.MinRemainder = product.MinRemainder;
                existing.RequiresPrescription = product.RequiresPrescription;
                existing.Description = product.Description;
                _db.Entry(existing).State = EntityState.Modified;

                if (Commit() > 0)
                {
                    var index = Products.IndexOf(Products.FirstOrDefault(p => p.Id == product.Id));
                    if (index >= 0)
                    {
                        Products[index] = existing;
                    }
                }
            }
        }

        public List<Product> GetProductsByStatus(string status)
        {
            return status switch
            {
                "С низким остатком" => Products.Where(p => p.IsLowStock).ToList(),
                "Нет в наличии" => Products.Where(p => p.IsOutOfStock).ToList(),
                "Истекает срок" => Products.Where(p => p.HasExpiringBatches).ToList(),
                "Просроченные" => Products.Where(p => p.HasExpiredBatches).ToList(),
                _ => Products.ToList()
            };
        }

        public (int total, int lowStock, int outOfStock, int expiring) GetStatistics()
        {
            var total = Products.Count;
            var lowStock = Products.Count(p => p.IsLowStock);
            var outOfStock = Products.Count(p => p.IsOutOfStock);
            var expiring = Products.Count(p => p.HasExpiringBatches);

            return (total, lowStock, outOfStock, expiring);
        }
    }
}
