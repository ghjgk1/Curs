using System.Collections.ObjectModel;
using PharmacyWarehouse.Models;
using Microsoft.EntityFrameworkCore;
using PharmacyWarehouse.Data;

namespace PharmacyWarehouse.Services
{

    public class DocumentService
    {
        private readonly PharmacyWarehouseContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Document> Documents { get; set; } = new();

        public DocumentService()
        {
            GetAll();
        }

        public int Commit() => _db.SaveChanges();

        public void Add(Document document)
        {
            var _document = new Document
            {
                Number = document.Number,
                Type = document.Type,
                Date = document.Date ?? DateOnly.FromDateTime(DateTime.Today),
                SupplierId = document.SupplierId,
                Counterparty = document.Counterparty,
                Amount = document.Amount ?? 0
            };

            _db.Documents.Add(_document);
            if (Commit() > 0)
                Documents.Add(_document);
        }

        public void GetAll()
        {
            var documents = _db.Documents
                .Include(d => d.Supplier)
                .Include(d => d.DocumentLines)
                    .ThenInclude(dl => dl.Batch)
                        .ThenInclude(b => b.Product)
                .ToList();

            Documents.Clear();
            foreach (var document in documents)
            {
                Documents.Add(document);
            }
        }

        public void Remove(Document document)
        {
            var existing = _db.Documents.Find(document.Id);
            if (existing != null)
            {
                _db.Documents.Remove(existing);
                if (Commit() > 0)
                    if (Documents.Contains(document))
                        Documents.Remove(document);
            }
        }

        public Document? GetById(int id)
        {
            return _db.Documents
                .Include(d => d.Supplier)
                .Include(d => d.DocumentLines)
                    .ThenInclude(dl => dl.Batch)
                        .ThenInclude(b => b.Product)
                .FirstOrDefault(d => d.Id == id);
        }

        public string GenerateWriteOffNumber()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int count = Documents.Count(d => d.Type == "Списание" &&
                                             d.Date.HasValue &&
                                             d.Date.Value.Year == today.Year &&
                                             d.Date.Value.Month == today.Month) + 1;

            return $"СП-{today:yyyyMM}-{count:D3}";
        }

        public ObservableCollection<Document> GetWriteOffDocuments()
        {
            var documents = _db.Documents
                .Where(d => d.Type == "Списание")
                .Include(d => d.Supplier)
                .Include(d => d.DocumentLines)
                    .ThenInclude(dl => dl.Batch)
                        .ThenInclude(b => b.Product)
                .Include(d => d.WriteOffs)
                .OrderByDescending(d => d.Date)
                .ToList();

            var collection = new ObservableCollection<Document>();
            foreach (var document in documents)
            {
                collection.Add(document);
            }
            return collection;
        }
    }
}
