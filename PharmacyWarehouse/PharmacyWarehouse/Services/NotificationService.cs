using System.Collections.ObjectModel;
using PharmacyWarehouse.Models;
using Microsoft.EntityFrameworkCore;
using PharmacyWarehouse.Data;

namespace PharmacyWarehouse.Services
{
    public class NotificationService
    {
        private readonly PharmacyWarehouseContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Notification> Notifications { get; set; } = new();

        public NotificationService()
        {
            GetAll();
        }

        public int Commit() => _db.SaveChanges();

        public void Add(Notification notification)
        {
            var _notification = new Notification
            {
                Type = notification.Type,
                ProductId = notification.ProductId,
                BatchId = notification.BatchId,
                Message = notification.Message,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            _db.Notifications.Add(_notification);
            if (Commit() > 0)
                Notifications.Add(_notification);
        }

        public void GetAll()
        {
            var notifications = _db.Notifications
                .Include(n => n.Product)
                .Include(n => n.Batch)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();

            Notifications.Clear();
            foreach (var notification in notifications)
            {
                Notifications.Add(notification);
            }
        }

        public void Remove(Notification notification)
        {
            var existing = _db.Notifications.Find(notification.Id);
            if (existing != null)
            {
                _db.Notifications.Remove(existing);
                if (Commit() > 0)
                    if (Notifications.Contains(notification))
                        Notifications.Remove(notification);
            }
        }

        public int GetUnreadCount()
        {
            return _db.Notifications.Count(n => n.IsRead == false);
        }
    }
}
