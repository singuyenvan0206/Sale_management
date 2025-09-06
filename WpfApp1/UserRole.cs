using System;

namespace WpfApp1
{
    /// <summary>
    /// Enum định nghĩa các vai trò người dùng trong hệ thống POS
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Quản trị viên - có toàn quyền
        /// </summary>
        Admin = 1,
        
        /// <summary>
        /// Quản lý - có hầu hết quyền trừ quản lý người dùng
        /// </summary>
        Manager = 2,
        
        /// <summary>
        /// Thu ngân - chỉ có quyền bán hàng và xem cơ bản
        /// </summary>
        Cashier = 3
    }

    /// <summary>
    /// Extension methods cho UserRole
    /// </summary>
    public static class UserRoleExtensions
    {
        /// <summary>
        /// Kiểm tra xem role có quyền quản lý sản phẩm không
        /// </summary>
        public static bool CanManageProducts(this UserRole role)
        {
            return role == UserRole.Admin || role == UserRole.Manager;
        }

        /// <summary>
        /// Kiểm tra xem role có quyền quản lý danh mục không
        /// </summary>
        public static bool CanManageCategories(this UserRole role)
        {
            return role == UserRole.Admin || role == UserRole.Manager;
        }

        /// <summary>
        /// Kiểm tra xem role có quyền quản lý khách hàng không
        /// </summary>
        public static bool CanManageCustomers(this UserRole role)
        {
            return true; // Tất cả role đều có thể quản lý khách hàng
        }

        /// <summary>
        /// Kiểm tra xem role có quyền tạo hóa đơn không
        /// </summary>
        public static bool CanCreateInvoices(this UserRole role)
        {
            return true; // Tất cả role đều có thể tạo hóa đơn
        }

        /// <summary>
        /// Kiểm tra xem role có quyền xem báo cáo không
        /// </summary>
        public static bool CanViewReports(this UserRole role)
        {
            return role == UserRole.Admin || role == UserRole.Manager;
        }

        /// <summary>
        /// Kiểm tra xem role có quyền quản lý người dùng không
        /// </summary>
        public static bool CanManageUsers(this UserRole role)
        {
            return role == UserRole.Admin;
        }

        /// <summary>
        /// Kiểm tra xem role có quyền cấu hình hệ thống không
        /// </summary>
        public static bool CanManageSettings(this UserRole role)
        {
            return role == UserRole.Admin;
        }

        /// <summary>
        /// Lấy tên hiển thị của role
        /// </summary>
        public static string GetDisplayName(this UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Quản trị viên",
                UserRole.Manager => "Quản lý",
                UserRole.Cashier => "Thu ngân",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Lấy mô tả quyền hạn của role
        /// </summary>
        public static string GetDescription(this UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Có toàn quyền quản lý hệ thống, người dùng và dữ liệu",
                UserRole.Manager => "Quản lý sản phẩm, báo cáo và hoạt động bán hàng",
                UserRole.Cashier => "Thực hiện giao dịch bán hàng và quản lý khách hàng",
                _ => "Không có quyền hạn"
            };
        }
    }
}
