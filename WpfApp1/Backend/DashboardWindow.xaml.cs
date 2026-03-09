using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace WpfApp1
{
    public partial class DashboardWindow : Window
    {
        private string _currentUsername;
        private string _currentRole;
        private bool _isRefreshing = false;
        public static event Action? OnDashboardRefreshNeeded;
        private static DashboardWindow? _currentInstance;

        public DashboardWindow(string username, string role)
        {
            InitializeComponent();
            _currentUsername = username;
            _currentRole = role;
            _currentInstance = this;

            // Subscribe to refresh event
            OnDashboardRefreshNeeded += HandleDashboardRefreshNeeded;

            // Add cleanup when window is closing
            this.Closing += DashboardWindow_Closing;
            
            // Set current user in application resources for other windows to access
            Application.Current.Resources["CurrentUser"] = username;
            
            // Get employee name for display
            string employeeName = DatabaseHelper.GetEmployeeName(username);
            UserInfoTextBlock.Text = $"Chào mừng, {employeeName} ({role})";
            LoadKpis();

            // Initialize auto-refresh timer - DISABLED: Now using event-driven updates
            // InitializeRefreshTimer();
            
            // Apply role-based visibility for unified dashboard
            ApplyRoleVisibility(ParseRole(role));

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

        private static UserRole ParseRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return UserRole.Cashier;
            return role.Trim().ToLower() switch
            {
                "admin" => UserRole.Admin,
                "manager" => UserRole.Manager,
                "cashier" => UserRole.Cashier,
                _ => UserRole.Cashier
            };
        }

        private void ApplyRoleVisibility(UserRole role)
        {
            if (UserManagementNavItem != null) UserManagementNavItem.Visibility = Visibility.Visible;
            if (ProductsNavItem != null) ProductsNavItem.Visibility = Visibility.Visible;
            if (CategoriesNavItem != null) CategoriesNavItem.Visibility = Visibility.Visible;
            if (ProductSearchNavItem != null) ProductSearchNavItem.Visibility = Visibility.Collapsed;
            if (CustomersNavItem != null) CustomersNavItem.Visibility = Visibility.Visible;
            if (InvoicesNavItem != null) InvoicesNavItem.Visibility = Visibility.Visible;
            if (ReportsNavItem != null) ReportsNavItem.Visibility = Visibility.Visible;
            if (SettingsNavItem != null) SettingsNavItem.Visibility = Visibility.Visible;

            switch (role)
            {
                case UserRole.Admin:
                    // Admin sees everything
                    break;
                case UserRole.Manager:


                    if (UserManagementNavItem != null) UserManagementNavItem.Visibility = Visibility.Collapsed;
                    break;
                default:

                    if (ReportsNavItem != null) ReportsNavItem.Visibility = Visibility.Collapsed;
                    if (UserManagementNavItem != null) UserManagementNavItem.Visibility = Visibility.Collapsed;
                    if (SettingsNavItem != null) SettingsNavItem.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void StopRefreshTimer()
        {
            // Timer functionality removed - now using event-driven updates
            // This method is kept for cleanup purposes
        }

        private void LoadKpis()
        {
            try
            {
                var todayStart = System.DateTime.Today;
                var todayEnd = todayStart.AddDays(1).AddTicks(-1);
                var monthStart = System.DateTime.Today.AddDays(-30); // 30 ngày gần nhất
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
            revenueModel.Axes.Add(new LinearAxis { 
                Position = AxisPosition.Left, 
                Title = "Doanh thu (VND)", 
                StringFormat = "#,##0", 
                Minimum = 0, 
                IsZoomEnabled = false, 
                IsPanEnabled = false 
            });
            var line = new LineSeries { MarkerType = MarkerType.Circle };
            foreach (var (day, amount) in revenuePoints)
            {
                line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(day), (double)amount));
            }
            revenueModel.Series.Add(line);
            if (HomeRevenuePlot != null) HomeRevenuePlot.Model = revenueModel;

            // Revenue by category pie - lấy TOÀN BỘ database (không giới hạn thời gian)
            var categoryStartDate = System.DateTime.MinValue;
            var categoryEndDate = System.DateTime.MaxValue;
            var byCat = DatabaseHelper.GetRevenueByCategory(categoryStartDate, categoryEndDate, 10000);
            var catModel = new PlotModel();
            var pie = new OxyPlot.Series.PieSeries { InsideLabelPosition = 0.7 };
            foreach (var (name, revenue) in byCat)
            {
                pie.Slices.Add(new OxyPlot.Series.PieSlice(name, (double)revenue));
            }
            catModel.Series.Add(pie);
            if (HomeCategoryPie != null) HomeCategoryPie.Model = catModel;

            // Top 10 Customers (Bar Chart)
            var topCustomers = DatabaseHelper.GetTopCustomers(10);
            var customerModel = new PlotModel();
            customerModel.Axes.Add(new CategoryAxis 
            { 
                Position = AxisPosition.Left,
                IsZoomEnabled = false,
                IsPanEnabled = false
            });
            customerModel.Axes.Add(new LinearAxis 
            { 
                Position = AxisPosition.Bottom, 
                Title = "Tổng chi tiêu (VND)", 
                StringFormat = "#,##0", 
                IsZoomEnabled = false, 
                IsPanEnabled = false 
            });
            var barSeries = new BarSeries { FillColor = OxyColors.Blue };
            var labels = new List<string>();
            foreach (var (name, spent) in topCustomers)
            {
                labels.Add(name.Length > 15 ? name.Substring(0, 15) + "..." : name);
                barSeries.Items.Add(new BarItem((double)spent));
            }
            if (topCustomers.Count > 0)
            {
                ((CategoryAxis)customerModel.Axes[0]).Labels.AddRange(labels);
                customerModel.Series.Add(barSeries);
            }
            if (TopCustomersPlot != null) TopCustomersPlot.Model = customerModel;

            // Top 10 Selling Products (Bar Chart)
            var topProducts = DatabaseHelper.GetTopProducts(10);
            var productsModel = new PlotModel();
            productsModel.Axes.Add(new CategoryAxis 
            { 
                Position = AxisPosition.Left,
                IsZoomEnabled = false,
                IsPanEnabled = false
            });
            productsModel.Axes.Add(new LinearAxis 
            { 
                Position = AxisPosition.Bottom, 
                Title = "Số lượng bán ra", 
                IsZoomEnabled = false, 
                IsPanEnabled = false 
            });
            var productsBarSeries = new BarSeries { FillColor = OxyColors.Orange };
            var productsLabels = new List<string>();
            foreach (var (name, qty, revenue) in topProducts)
            {
                productsLabels.Add(name.Length > 15 ? name.Substring(0, 15) + "..." : name);
                productsBarSeries.Items.Add(new BarItem(qty));
            }
            if (topProducts.Count > 0)
            {
                ((CategoryAxis)productsModel.Axes[0]).Labels.AddRange(productsLabels);
                productsModel.Series.Add(productsBarSeries);
            }
            if (TopProductsPlot != null) TopProductsPlot.Model = productsModel;

            // Load Low Stock Products Alert
            LoadLowStockAlert();
        }

        private void LoadLowStockAlert()
        {
            try
            {
                var lowStock = DatabaseHelper.GetLowStockProducts(10);
                var count = lowStock.Count;
                
                if (LowStockAlertCount != null)
                {
                    LowStockAlertCount.Text = count.ToString();
                }
                
                if (LowStockAlertButton != null)
                {
                    LowStockAlertButton.IsEnabled = count > 0;
                }
            }
            catch
            {
                // Silent failure
            }
        }
        
        private void LowStockAlertButton_Click(object sender, RoutedEventArgs e)
        {
            // Create and show low stock window
            var lowStockWindow = new Window
            {
                Title = "⚠️ Danh Sách Sản Phẩm Sắp Hết",
                Width = 800,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };
            
            var grid = new Grid();
            lowStockWindow.Content = grid;
            
            // Add header
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            
            var header = new Border
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 243, 205)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 193, 7)),
                BorderThickness = new System.Windows.Thickness(0, 0, 0, 2)
            };
            grid.Children.Add(header);
            Grid.SetRow(header, 0);
            
            var headerText = new TextBlock
            {
                Text = "⚠️ Danh Sách Sản Phẩm Sắp Hết (Tồn kho ≤ 10)",
                FontSize = 18,
                FontWeight = System.Windows.FontWeights.Bold,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            header.Child = headerText;
            
            // Add DataGrid
            var dataGrid = new DataGrid
            {
                AutoGenerateColumns = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                IsReadOnly = true,
                GridLinesVisibility = DataGridGridLinesVisibility.None,
                BorderThickness = new System.Windows.Thickness(0),
                RowHeaderWidth = 0,
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                CanUserReorderColumns = false,
                CanUserResizeRows = false,
                SelectionMode = DataGridSelectionMode.Single,
                Margin = new System.Windows.Thickness(0)
            };
            
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "STT", Width = 50, Binding = new System.Windows.Data.Binding("Index") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Tên Sản Phẩm", Width = new DataGridLength(1, DataGridLengthUnitType.Star), Binding = new System.Windows.Data.Binding("ProductName") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Danh Mục", Width = 150, Binding = new System.Windows.Data.Binding("CategoryName") });
            dataGrid.Columns.Add(new DataGridTextColumn { Header = "Tồn Kho", Width = 100, Binding = new System.Windows.Data.Binding("StockQuantity") });
            
            grid.Children.Add(dataGrid);
            Grid.SetRow(dataGrid, 2);
            
            // Load data
            try
            {
                var lowStock = DatabaseHelper.GetLowStockProducts(10);
                var items = lowStock.Select((p, index) => new LowStockItem
                {
                    Index = index + 1,
                    ProductName = p.ProductName,
                    CategoryName = p.CategoryName,
                    StockQuantity = p.StockQuantity
                }).ToList();
                
                dataGrid.ItemsSource = items;
            }
            catch { }
            
            lowStockWindow.ShowDialog();
        }

        private void ProductManagement_Click(object sender, RoutedEventArgs e)
        {
            var productWindow = new ProductManagementWindow();
            ShowOwnedDialog(productWindow);
        }

        private void CategoryManagement_Click(object sender, RoutedEventArgs e)
        {
            var categoryWindow = new CategoryManagementWindow();
            ShowOwnedDialog(categoryWindow);
        }

        private void CustomerManagement_Click(object sender, RoutedEventArgs e)
        {
            var customerWindow = new CustomerManagementWindow();
            ShowOwnedDialog(customerWindow);
        }

        private void InvoiceManagement_Click(object sender, RoutedEventArgs e)
        {
            var invoiceWindow = new InvoiceManagementWindow();
            ShowOwnedDialog(invoiceWindow);
        }
        private void VoucherManagement_Click(object sender, RoutedEventArgs e)
        {
            var voucherWindow = new VoucherManagementWindow();
            ShowOwnedDialog(voucherWindow);
        }
        private void SupplierManagement_Click(object sender, RoutedEventArgs e)
        {
            var supplierWindow = new SupplierManagementWindow();
            ShowOwnedDialog(supplierWindow);
        }
        private void ReportsManagement_Click(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new ReportsWindow();
            ShowOwnedDialog(reportsWindow);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            ShowOwnedDialog(settingsWindow);
        }

        private void DashboardWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Unsubscribe from refresh event to prevent memory leaks
                OnDashboardRefreshNeeded -= HandleDashboardRefreshNeeded;

                // Stop and cleanup timer
                StopRefreshTimer();

                // Clear any resources or connections
                Application.Current.Resources.Remove("CurrentUser");
            }
            catch { }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new MainWindow();
                Application.Current.MainWindow = loginWindow;
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
                    // Ensure the right content host container is hidden when returning Home
                    if (MainContentScrollViewer != null)
                    {
                        MainContentScrollViewer.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    return;
                }

                // For other tabs, open the corresponding window content modally in the host
                System.Windows.Window? contentWindow = tab switch
                {
                    "📦 Sản Phẩm" or "Products" => new ProductManagementWindow(),
                    "📂 Danh Mục" or "Categories" => new CategoryManagementWindow(),
                    "🏭 Nhà Cung Cấp" or "Suppliers" => new SupplierManagementWindow(),
                    "🎟️ Mã Giảm Giá" or "Vouchers" => new VoucherManagementWindow(),
                    // Product Search UI removed
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
                    // Make sure the ScrollViewer hosting MainContentHost is visible
                    if (MainContentScrollViewer != null)
                    {
                        MainContentScrollViewer.Visibility = System.Windows.Visibility.Visible;
                    }
                    // Close the source window instance to avoid hidden open windows
                    try { contentWindow.Close(); } catch { }
                }
            }
        }

        private void ShowOwnedDialog(Window dialog)
        {
            try
            {
                if (this.IsLoaded)
                {
                    dialog.Owner = this;
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                dialog.ShowDialog();
            }
            catch (InvalidOperationException)
            {
                // Fallback if owner not allowed yet
                dialog.Owner = null;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ShowDialog();
            }
        }

        private void HandleDashboardRefreshNeeded()
        {
            if (_currentInstance != null && !_currentInstance._isRefreshing)
            {
                _currentInstance._isRefreshing = true;

                try
                {
                    // Refresh KPIs directly
                    _currentInstance.Dispatcher.Invoke(() =>
                    {
                        _currentInstance.LoadKpis();
                    });
                }
                catch
                {
                    // Silent failure
                }
                finally
                {
                    _currentInstance._isRefreshing = false;
                }
            }
        }

        // Static method to trigger dashboard refresh from other windows
        public static void TriggerDashboardRefresh()
        {
            OnDashboardRefreshNeeded?.Invoke();
        }

    }

    // Class for Low Stock Items
    public class LowStockItem
    {
        public int Index { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}
