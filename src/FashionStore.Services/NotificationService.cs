using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;

namespace FashionStore.Services
{
    public class NotificationService : INotificationService
    {
        private static readonly List<Notification> _notifications = new();
        private static int _nextId = 1;

        public Task AddNotificationAsync(string title, string message, string type = "Info", string? actionUrl = null)
        {
            _notifications.Insert(0, new Notification
            {
                Id = _nextId++,
                Title = title,
                Message = message,
                Type = type,
                ActionUrl = actionUrl,
                IsRead = false,
                CreatedDate = DateTime.Now
            });
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Notification>> GetUnreadNotificationsAsync()
        {
            return Task.FromResult(_notifications.Where(n => !n.IsRead).AsEnumerable());
        }

        public Task MarkAsReadAsync(int id)
        {
            var n = _notifications.FirstOrDefault(x => x.Id == id);
            if (n != null) n.IsRead = true;
            return Task.CompletedTask;
        }

        public Task MarkAllAsReadAsync()
        {
            foreach (var n in _notifications) n.IsRead = true;
            return Task.CompletedTask;
        }
    }
}
