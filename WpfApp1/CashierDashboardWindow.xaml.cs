using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class CashierDashboardWindow : Window
    {
        private string _currentUsername;
        private UserRole _currentRole;

        public CashierDashboardWindow(string username, UserRole role)
        {
            InitializeComponent();
            _currentUsername = username;
            _currentRole = role;
            UserInfoTextBlock.Text = $"Xin chào, {username} ({role.GetDisplayName()})";
            LoadKpis();
            LoadRecentInvoices();
        }

        private void LoadKpis()
        {
            try
            {
                var todayStart = DateTime.Today;
                var todayEnd = todayStart.AddDays(1).AddTicks(-1);

                // Today's sales
                decimal todaySales = DatabaseHelper.GetRevenueBetween(todayStart, todayEnd);
                TodaySalesText.Text = todaySales.ToString("N0") + " VND";

                // Today's invoices
                int todayInvoices = DatabaseHelper.GetInvoiceCountBetween(todayStart, todayEnd);
                TodayInvoicesText.Text = todayInvoices.ToString();

                // Total products
                int totalProducts = DatabaseHelper.GetTotalProducts();
                TotalProductsText.Text = totalProducts.ToString();

                // Total customers
                int totalCustomers = DatabaseHelper.GetTotalCustomers();
                TotalCustomersText.Text = totalCustomers.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRecentInvoices()
        {
            try
            {
                var todayStart = DateTime.Today;
                var todayEnd = DateTime.Now;
                
                var recentInvoices = DatabaseHelper.QueryInvoices(todayStart, todayEnd, null, "")
                    .OrderByDescending(i => i.CreatedDate)
                    .Take(10)
                    .Select(i => new
                    {
                        i.Id,
                        i.CreatedDate,
                        CustomerName = string.IsNullOrEmpty(i.CustomerName) ? "Khách lẻ" : i.CustomerName,
                        i.Total
                    })
                    .ToList();

                RecentInvoicesGrid.ItemsSource = recentInvoices;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải hóa đơn gần đây: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InvoiceManagement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var invoiceWindow = new InvoiceManagementWindow();
                invoiceWindow.ShowDialog();
                LoadKpis(); // Refresh data after invoice operations
                LoadRecentInvoices();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở quản lý hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CustomerManagement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var customerWindow = new CustomerManagementWindow();
                customerWindow.ShowDialog();
                LoadKpis(); // Refresh data
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở quản lý khách hàng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProductSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Tạo một window tìm kiếm sản phẩm đơn giản cho cashier
                var searchWindow = new ProductSearchWindow();
                searchWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở tìm kiếm sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuickInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Tạo hóa đơn nhanh với khách hàng mặc định
                var invoiceWindow = new InvoiceManagementWindow();
                invoiceWindow.ShowDialog();
                LoadKpis();
                LoadRecentInvoices();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tạo hóa đơn nhanh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            // Refresh data when window is shown
            LoadKpis();
            LoadRecentInvoices();
        }
    }
}
