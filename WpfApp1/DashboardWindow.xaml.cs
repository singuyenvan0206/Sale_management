using System.Windows;
using System;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace WpfApp1
{
    public partial class DashboardWindow : Window
    {
        private string _currentUsername;
        private string _currentRole;

        public DashboardWindow(string username, string role)
        {
            InitializeComponent();
            _currentUsername = username;
            _currentRole = role;
            UserInfoTextBlock.Text = $"Welcome, {username} ({role})";
            LoadKpis();
            
            // Hide user management for non-admin users
            if (role != "Admin")
            {
                UserManagementBorder.Visibility = Visibility.Collapsed;
            }

            // Ensure initial UI state after load
            this.Dispatcher.BeginInvoke(() =>
            {
                if (DefaultContentScrollViewer != null)
                {
                    DefaultContentScrollViewer.Visibility = System.Windows.Visibility.Visible;
                }
                if (MainContentHost != null)
                {
                    MainContentHost.Visibility = System.Windows.Visibility.Collapsed;
                    MainContentHost.Content = null;
                }
                if (NavList != null)
                {
                    NavList.SelectedIndex = 0;
                }
            });
        }

        private void LoadKpis()
        {
            try
            {
                var todayStart = System.DateTime.Today;
                var todayEnd = todayStart.AddDays(1).AddTicks(-1);
                var monthStart = System.DateTime.Today.AddDays(-30);
                var monthEnd = todayEnd;

                decimal revenueToday = DatabaseHelper.GetRevenueBetween(todayStart, todayEnd);
                decimal revenue30 = DatabaseHelper.GetRevenueBetween(monthStart, monthEnd);
                int invoicesToday = DatabaseHelper.GetInvoiceCountBetween(todayStart, todayEnd);
                int totalCustomers = DatabaseHelper.GetTotalCustomers();
                int totalProducts = DatabaseHelper.GetTotalProducts();

                RevenueTodayText.Text = $"${revenueToday:F2}";
                Revenue30Text.Text = $"${revenue30:F2}";
                InvoicesTodayText.Text = invoicesToday.ToString();
                CountsText.Text = $"{totalCustomers} / {totalProducts}";
                LoadHomeCharts(monthStart, monthEnd);
            }
            catch
            {
                // ignore UI KPI errors
            }
        }

        private void LoadHomeCharts(DateTime monthStart, DateTime monthEnd)
        {
            // Revenue last 30 days line
            var revenuePoints = DatabaseHelper.GetRevenueByDay(monthStart, monthEnd);
            var revenueModel = new PlotModel();
            revenueModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "MM-dd", IsZoomEnabled = false, IsPanEnabled = false });
            revenueModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, IsZoomEnabled = false, IsPanEnabled = false });
            var line = new LineSeries { MarkerType = MarkerType.Circle };
            foreach (var (day, amount) in revenuePoints)
            {
                line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(day), (double)amount));
            }
            revenueModel.Series.Add(line);
            if (HomeRevenuePlot != null) HomeRevenuePlot.Model = revenueModel;

            // Revenue by category pie
            var byCat = DatabaseHelper.GetRevenueByCategory(monthStart, monthEnd, 8);
            var catModel = new PlotModel();
            var pie = new OxyPlot.Series.PieSeries { InsideLabelPosition = 0.7 };
            foreach (var (name, revenue) in byCat)
            {
                pie.Slices.Add(new OxyPlot.Series.PieSlice(name, (double)revenue));
            }
            catModel.Series.Add(pie);
            if (HomeCategoryPie != null) HomeCategoryPie.Model = catModel;
        }

        private void ProductManagement_Click(object sender, RoutedEventArgs e)
        {
            var productWindow = new ProductManagementWindow();
            productWindow.Owner = this;
            productWindow.ShowDialog();
        }

        private void CategoryManagement_Click(object sender, RoutedEventArgs e)
        {
            var categoryWindow = new CategoryManagementWindow();
            categoryWindow.Owner = this;
            categoryWindow.ShowDialog();
        }

        private void CustomerManagement_Click(object sender, RoutedEventArgs e)
        {
            var customerWindow = new CustomerManagementWindow();
            customerWindow.Owner = this;
            customerWindow.ShowDialog();
        }

        private void InvoiceManagement_Click(object sender, RoutedEventArgs e)
        {
            var invoiceWindow = new InvoiceManagementWindow();
            invoiceWindow.Owner = this;
            invoiceWindow.ShowDialog();
        }

        private void ReportsManagement_Click(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new ReportsWindow();
            reportsWindow.Owner = this;
            reportsWindow.ShowDialog();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void NavList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MainContentHost == null || DefaultContentScrollViewer == null)
            {
                return; // Controls not ready yet (can happen during InitializeComponent)
            }
            if (NavList.SelectedItem is System.Windows.Controls.ListBoxItem item)
            {
                string? tab = item.Content?.ToString();
                if (string.IsNullOrWhiteSpace(tab)) return;

                // Home resets to dashboard content
                if (tab == "Home")
                {
                    MainContentHost.Visibility = System.Windows.Visibility.Collapsed;
                    DefaultContentScrollViewer.Visibility = System.Windows.Visibility.Visible;
                    return;
                }

                // For other tabs, open the corresponding window content modally in the host
                System.Windows.Window? contentWindow = tab switch
                {
                    "Products" => new ProductManagementWindow(),
                    "Categories" => new CategoryManagementWindow(),
                    "Customers" => new CustomerManagementWindow(),
                    "Invoices" => new InvoiceManagementWindow(),
                    "Reports" => new ReportsWindow(),
                    "Settings" => new SettingsWindow(),
                    "Logout" => null,
                    _ => null
                };

                if (tab == "Logout")
                {
                    LogoutButton_Click(this, new RoutedEventArgs());
                    return;
                }

                if (contentWindow != null)
                {
                    // Re-parent the window content into the ContentControl
                    var content = contentWindow.Content as System.Windows.UIElement;
                    contentWindow.Content = null;
                    MainContentHost.Content = content;
                    DefaultContentScrollViewer.Visibility = System.Windows.Visibility.Collapsed;
                    MainContentHost.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void UserManagement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userManagementWindow = new UserManagementWindow();
                userManagementWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở quản lý người dùng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DemoInvoice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InvoiceDemo.ShowDemoInvoice();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing demo invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
