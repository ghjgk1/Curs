using System.Collections.ObjectModel;
using PharmacyWarehouse.Models;
using Microsoft.EntityFrameworkCore;
using PharmacyWarehouse.Data;

namespace PharmacyWarehouse.Services
{

    public class WriteOffService
    {
        private readonly PharmacyWarehouseContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<WriteOff> WriteOffs { get; set; } = new();

        public WriteOffService()
        {
            GetAll();
        }

        public int Commit() => _db.SaveChanges();

        public void Add(WriteOff writeOff)
        {
            var _writeOff = new WriteOff
            {
                DocumentId = writeOff.DocumentId,
                Reason = writeOff.Reason,
                Comment = writeOff.Comment
            };

            _db.WriteOffs.Add(_writeOff);
            if (Commit() > 0)
                WriteOffs.Add(_writeOff);
        }

        public void Update(WriteOff writeOff)
        {
            var existing = _db.WriteOffs.Find(writeOff.Id);
            if (existing != null)
            {
                existing.Reason = writeOff.Reason;
                existing.Comment = writeOff.Comment;

                if (Commit() > 0)
                {
                    var index = WriteOffs.IndexOf(existing);
                    if (index != -1)
                    {
                        WriteOffs[index] = existing;
                    }
                }
            }
        }

        public void GetAll()
        {
            var writeOffs = _db.WriteOffs
                .Include(w => w.Document)
                    .ThenInclude(d => d.DocumentLines)
                        .ThenInclude(dl => dl.Batch)
                            .ThenInclude(b => b.Product)
                .OrderByDescending(w => w.Document.Date)
                .ToList();

            WriteOffs.Clear();
            foreach (var writeOff in writeOffs)
            {
                WriteOffs.Add(writeOff);
            }
        }

        public void Remove(WriteOff writeOff)
        {
            var existing = _db.WriteOffs.Find(writeOff.Id);
            if (existing != null)
            {
                // Удаляем связанный документ
                if (existing.Document != null)
                {
                    _db.Documents.Remove(existing.Document);
                }

                _db.WriteOffs.Remove(existing);
                if (Commit() > 0)
                    if (WriteOffs.Contains(writeOff))
                        WriteOffs.Remove(writeOff);
            }
        }

        public WriteOff? GetById(int id)
        {
            return _db.WriteOffs
                .Include(w => w.Document)
                    .ThenInclude(d => d.DocumentLines)
                        .ThenInclude(dl => dl.Batch)
                            .ThenInclude(b => b.Product)
                .FirstOrDefault(w => w.Id == id);
        }

        public ObservableCollection<WriteOff> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return WriteOffs;

            var filtered = WriteOffs
                .Where(w =>
                    w.Reason.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    w.Comment != null && w.Comment.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    w.Document.Number.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var result = new ObservableCollection<WriteOff>();
            foreach (var item in filtered)
                result.Add(item);

            return result;
        }
    }
}
