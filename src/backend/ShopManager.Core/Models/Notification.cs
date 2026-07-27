using System;
using ShopManager.Core.Enums;

namespace ShopManager.Core.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = NotificationType.Info.ToDbString();
        public string? ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
