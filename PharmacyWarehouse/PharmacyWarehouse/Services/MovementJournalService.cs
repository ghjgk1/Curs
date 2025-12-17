using System.Collections.ObjectModel;
using PharmacyWarehouse.Models;
using Microsoft.EntityFrameworkCore;
using PharmacyWarehouse.Data;

namespace PharmacyWarehouse.Services
{

    public class MovementJournalService
    {
        private readonly PharmacyWarehouseContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<MovementJournal> MovementJournals { get; set; } = new();

        public MovementJournalService()
        {
            GetAll();
        }

        public int Commit() => _db.SaveChanges();

        public void Add(MovementJournal journal)
        {
            var _journal = new MovementJournal
            {
                OperationDate = journal.OperationDate ?? DateTime.Now,
                OperationType = journal.OperationType,
                DocumentId = journal.DocumentId,
                BatchId = journal.BatchId,
                QuantityChange = journal.QuantityChange,
                RemainingQuantity = journal.RemainingQuantity,
                UserName = journal.UserName ?? Environment.UserName
            };

            _db.MovementJournals.Add(_journal);
            if (Commit() > 0)
                MovementJournals.Add(_journal);
        }

        public void GetAll()
        {
            var journals = _db.MovementJournals
                .Include(j => j.Document)
                .Include(j => j.Batch)
                    .ThenInclude(b => b.Product)
                .OrderByDescending(j => j.OperationDate)
                .ToList();

            MovementJournals.Clear();
            foreach (var journal in journals)
            {
                MovementJournals.Add(journal);
            }
        }

        public void Remove(MovementJournal journal)
        {
            var existing = _db.MovementJournals.Find(journal.Id);
            if (existing != null)
            {
                _db.MovementJournals.Remove(existing);
                if (Commit() > 0)
                    if (MovementJournals.Contains(journal))
                        MovementJournals.Remove(journal);
            }
        }

        public List<MovementJournal> GetByProduct(int productId)
        {
            return _db.MovementJournals
                .Include(j => j.Document)
                .Include(j => j.Batch)
                    .ThenInclude(b => b.Product)
                .Where(j => j.Batch.ProductId == productId)
                .OrderByDescending(j => j.OperationDate)
                .ToList();
        }
    }
}
