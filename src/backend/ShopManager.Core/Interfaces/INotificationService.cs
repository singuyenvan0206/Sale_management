using System.Collections.Generic;
using System.Threading.Tasks;
using ShopManager.Core.Models;

namespace ShopManager.Core.Interfaces
{
    public interface INotificationService
    {
        Task AddNotificationAsync(string title, string message, string type = "Info", string? actionUrl = null);
        Task<IEnumerable<Notification>> GetUnreadNotificationsAsync();
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadAsync();
    }
}
