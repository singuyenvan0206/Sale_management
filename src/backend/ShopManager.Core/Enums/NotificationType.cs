namespace ShopManager.Core.Enums
{
    public enum NotificationType
    {
        Info,
        Warning,
        Success,
        Error
    }

    public static class NotificationTypeExtensions
    {
        public static string ToDbString(this NotificationType type) => type switch
        {
            NotificationType.Info => "Info",
            NotificationType.Warning => "Warning",
            NotificationType.Success => "Success",
            NotificationType.Error => "Error",
            _ => "Info"
        };

        public static NotificationType ParseNotificationType(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "warning" => NotificationType.Warning,
            "success" => NotificationType.Success,
            "error" => NotificationType.Error,
            _ => NotificationType.Info
        };
    }
}
