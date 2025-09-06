using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
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
            UserInfoTextBlock.Text = $"Chào mừng, {username} ({role})";
            LoadKpis();
            
            // Debug: Show role information
            System.Diagnostics.Debug.WriteLine($"DashboardWindow: Username={username}, Role={role}");
            
            // Hide user management for non-admin users
            if (role != "Admin")
            {
                System.Diagnostics.Debug.WriteLine("Hiding UserManagementBorder for non-admin user");
                UserManagementBorder.Visibility = Visibility.Collapsed;
                UserManagementNavItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Showing UserManagementBorder for admin user");
                UserManagementBorder.Visibility = Visibility.Visible;
                UserManagementNavItem.Visibility = Visibility.Visible;
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

                RevenueTodayText.Text = $"{revenueToday:N0}₫";
                Revenue30Text.Text = $"{revenue30:N0}₫";
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

        private void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if admin account exists
                string adminRole = DatabaseHelper.GetUserRole("admin");
                string debugInfo = $"Debug Info:\n" +
                                 $"Current User: {_currentUsername}\n" +
                                 $"Current Role: {_currentRole}\n" +
                                 $"Admin Role in DB: {adminRole}\n" +
                                 $"UserManagementBorder Visible: {UserManagementBorder.Visibility}";
                
                MessageBox.Show(debugInfo, "Debug Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Debug Error: {ex.Message}", "Debug Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", 
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
                if (tab == "🏠 Trang Chủ" || tab == "Home")
                {
                    MainContentHost.Visibility = System.Windows.Visibility.Collapsed;
                    DefaultContentScrollViewer.Visibility = System.Windows.Visibility.Visible;
                    return;
                }

                // For other tabs, open the corresponding window content modally in the host
                System.Windows.Window? contentWindow = tab switch
                {
                    "📦 Sản Phẩm" or "Products" => new ProductManagementWindow(),
                    "📂 Danh Mục" or "Categories" => new CategoryManagementWindow(),
                    "👥 Khách Hàng" or "Customers" => new CustomerManagementWindow(),
                    "🧾 Hóa Đơn" or "Invoices" => new InvoiceManagementWindow(),
                    "📊 Báo Cáo" or "Reports" => new ReportsWindow(),
                    "👤 Quản Lý Người Dùng" or "User Management" => new UserManagementWindow(),
                    "⚙️ Cài Đặt" or "Settings" => new SettingsWindow(),
                    "🚪 Đăng Xuất" or "Logout" => null,
                    _ => null
                };

                if (tab == "🚪 Đăng Xuất" || tab == "Logout")
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
                // Tạo menu lựa chọn demo
                var demoWindow = new Window
                {
                    Title = "Demo Hóa Đơn",
                    Width = 500,
                    Height = 400,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White)
                };

                var mainGrid = new Grid();
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Header
                var headerBorder = new Border
                {
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(28, 181, 224)),
                    Padding = new Thickness(20),
                    CornerRadius = new CornerRadius(0, 0, 0, 0)
                };

                var headerText = new TextBlock
                {
                    Text = "🎯 Chọn Mẫu Demo Hóa Đơn",
                    FontSize = 20,
                    FontWeight = System.Windows.FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.White,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };

                headerBorder.Child = headerText;
                Grid.SetRow(headerBorder, 0);

                // Content
                var contentPanel = new StackPanel
                {
                    Margin = new Thickness(30),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };

                var descriptionText = new TextBlock
                {
                    Text = "Chọn một trong các mẫu hóa đơn dưới đây để xem demo:",
                    FontSize = 14,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    Margin = new Thickness(0, 0, 0, 20),
                    TextAlignment = System.Windows.TextAlignment.Center
                };

                contentPanel.Children.Add(descriptionText);

                // Demo buttons
                var demos = new[]
                {
                    new { Title = "🖥️ Demo Công Nghệ", Description = "Laptop, chuột, tai nghe...", Action = new Action(() => InvoiceDemo.ShowDemoInvoice()) },
                    new { Title = "🍜 Demo Thực Phẩm", Description = "Cơm tấm, phở, bánh mì...", Action = new Action(() => InvoiceDemo.ShowFoodStoreDemo()) },
                    new { Title = "📱 Demo Điện Tử", Description = "iPhone, AirPods, phụ kiện...", Action = new Action(() => InvoiceDemo.ShowElectronicsDemo()) }
                };

                foreach (var demo in demos)
                {
                    var demoButton = new Button
                    {
                        Content = CreateDemoButtonContent(demo.Title, demo.Description),
                        Background = System.Windows.Media.Brushes.Transparent,
                        BorderThickness = new Thickness(2),
                        BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(28, 181, 224)),
                        Margin = new Thickness(0, 0, 0, 15),
                        Padding = new Thickness(20),
                        Cursor = System.Windows.Input.Cursors.Hand
                    };

                    demoButton.Click += (s, args) =>
                    {
                        demoWindow.Close();
                        demo.Action();
                    };

                    contentPanel.Children.Add(demoButton);
                }

                Grid.SetRow(contentPanel, 1);

                // Footer
                var footerBorder = new Border
                {
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245)),
                    Padding = new Thickness(20),
                    CornerRadius = new CornerRadius(0, 0, 0, 0)
                };

                var closeButton = new Button
                {
                    Content = "Đóng",
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(108, 117, 125)),
                    Foreground = System.Windows.Media.Brushes.White,
                    Padding = new Thickness(20, 10, 20, 10),
                    BorderThickness = new Thickness(0),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                closeButton.Click += (s, args) => demoWindow.Close();
                footerBorder.Child = closeButton;
                Grid.SetRow(footerBorder, 2);

                mainGrid.Children.Add(headerBorder);
                mainGrid.Children.Add(contentPanel);
                mainGrid.Children.Add(footerBorder);

                demoWindow.Content = mainGrid;
                demoWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị demo hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private StackPanel CreateDemoButtonContent(string title, string description)
        {
            var panel = new StackPanel();
            
            var titleText = new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = System.Windows.FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(28, 181, 224)),
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var descText = new TextBlock
            {
                Text = description,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Gray,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            panel.Children.Add(titleText);
            panel.Children.Add(descText);

            return panel;
        }
    }
}
