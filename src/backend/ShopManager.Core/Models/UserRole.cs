namespace ShopManager.Core.Models
{
    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        Cashier = 3
    }

    public static class UserRoleExtensions
    {
        public static bool CanManageTierSettings(this UserRole role)
        {
            return role == UserRole.Admin || role == UserRole.Manager;
        }

        public static string ToRoleString(this UserRole role) => role switch
        {
            UserRole.Admin => "Admin",
            UserRole.Manager => "Manager",
            UserRole.Cashier => "Cashier",
            _ => "Cashier"
        };

        public static UserRole ParseUserRole(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "admin" => UserRole.Admin,
            "manager" => UserRole.Manager,
            "cashier" => UserRole.Cashier,
            _ => UserRole.Cashier
        };
    }
}
