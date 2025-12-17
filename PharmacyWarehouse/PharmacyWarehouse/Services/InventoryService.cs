using System.Collections.ObjectModel;
using PharmacyWarehouse.Models;
using Microsoft.EntityFrameworkCore;
using PharmacyWarehouse.Data;

namespace PharmacyWarehouse.Services
{

    public class InventoryService
    {
        private readonly PharmacyWarehouseContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Inventory> Inventories { get; set; } = new();

        public InventoryService()
        {
            GetAll();
        }

        public int Commit() => _db.SaveChanges();

        public void Add(Inventory inventory)
        {
            var _inventory = new Inventory
            {
                InventoryDate = inventory.InventoryDate ?? DateOnly.FromDateTime(DateTime.Today),
                ResponsiblePerson = inventory.ResponsiblePerson,
                IsCompleted = inventory.IsCompleted ?? false
            };

            _db.Inventories.Add(_inventory);
            if (Commit() > 0)
                Inventories.Add(_inventory);
        }

        public void GetAll()
        {
            var inventories = _db.Inventories
                .Include(i => i.InventoryResults)
                    .ThenInclude(ir => ir.Product)
                .OrderByDescending(i => i.InventoryDate)
                .ToList();

            Inventories.Clear();
            foreach (var inventory in inventories)
            {
                Inventories.Add(inventory);
            }
        }

        public void Remove(Inventory inventory)
        {
            var existing = _db.Inventories.Find(inventory.Id);
            if (existing != null)
            {
                _db.Inventories.Remove(existing);
                if (Commit() > 0)
                    if (Inventories.Contains(inventory))
                        Inventories.Remove(inventory);
            }
        }

        public Inventory? GetById(int id)
        {
            return _db.Inventories
                .Include(i => i.InventoryResults)
                    .ThenInclude(ir => ir.Product)
                .FirstOrDefault(i => i.Id == id);
        }
    }
}
