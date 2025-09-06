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
            
            // Set default selection
            DashboardNavItem.IsSelected = true;
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



        private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavList.SelectedItem is ListBoxItem selectedItem)
            {
                string tab = selectedItem.Content.ToString();
                
                System.Windows.Window? contentWindow = tab switch
                {
                    "📊 Dashboard" => null, // Stay on current dashboard
                    "🧾 Tạo Hóa Đơn" => new InvoiceManagementWindow(),
                    "👥 Khách Hàng" => new CustomerManagementWindow(),
                    "📦 Sản Phẩm" => new ProductManagementWindow(),
                    "📂 Danh Mục" => new CategoryManagementWindow(),
                    "🔍 Tìm Kiếm" => new ProductSearchWindow(),
                    "📈 Báo Cáo" => new ReportsWindow(),
                    _ => null
                };
                
                if (contentWindow != null)
                {
                    contentWindow.ShowDialog();
                    LoadKpis(); // Refresh data after closing window
                    LoadRecentInvoices();
                }
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
