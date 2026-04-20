using FashionStore.App.ViewModels;
using FashionStore.Core.Models;
using FashionStore.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FashionStore.App.Views
{
    public partial class DashboardWindow : Window
    {
        private DashboardViewModel _viewModel;
        private static DashboardWindow? _currentInstance;

        public DashboardWindow(string username, string role)
        {
            InitializeComponent();
            _currentInstance = this;

            _viewModel = new DashboardViewModel();
            this.DataContext = _viewModel;
            _viewModel.Initialize(username, role);

            // Set current user in application resources for other windows to access
            Application.Current.Resources["CurrentUser"] = username;

            // Subscribe to refresh event
            DashboardViewModel.OnDashboardRefreshNeeded += HandleDashboardRefreshNeeded;

            // Add cleanup when window is closing
            this.Closing += DashboardWindow_Closing;

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

        private void HandleDashboardRefreshNeeded()
        {
            this.Dispatcher.Invoke(() =>
            {
                _viewModel.LoadKpis();
            });
        }

        private void LowStockAlertButton_Click(object sender, RoutedEventArgs e)
        {
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

            try
            {
                var lowStock = ProductService.GetLowStockProducts(10);
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

        private void DashboardWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                DashboardViewModel.OnDashboardRefreshNeeded -= HandleDashboardRefreshNeeded;
                Application.Current.Resources.Remove("CurrentUser");
            }
            catch { }
        }

        private void NavList_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Forward mouse wheel events up to the parent ScrollViewer
            // because ListBox swallows them by default
            if (!e.Handled)
            {
                e.Handled = true;
                var args = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = sender
                };
                var parent = ((System.Windows.Controls.Control)sender).Parent as UIElement;
                parent?.RaiseEvent(args);
            }
        }

        private void NavList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MainContentHost == null || DefaultContentScrollViewer == null)
            {
                return;
            }
            if (NavList.SelectedItem is System.Windows.Controls.ListBoxItem item)
            {
                string? tab = item.Content?.ToString();
                if (string.IsNullOrWhiteSpace(tab)) return;

                if (tab == "🏠 Trang Chủ" || tab == "Home")
                {
                    MainContentHost.Visibility = System.Windows.Visibility.Collapsed;
                    DefaultContentScrollViewer.Visibility = System.Windows.Visibility.Visible;
                    return;
                }

                System.Windows.Window? contentWindow = tab switch
                {
                    "📦 Sản Phẩm" or "📦 Products" => new ProductManagementWindow(),
                    "🔎 Tìm Kiếm Sản Phẩm" or "🔎 Search Products" => new ProductManagementWindow { DataContext = new ProductManagementViewModel { IsSearchMode = true } },
                    "📂 Danh Mục" or "📂 Categories" => new CategoryManagementWindow(),
                    "🏭 Nhà Cung Cấp" or "🏭 Suppliers" => new SupplierManagementWindow(),
                    "📋 Quản Lý Kho" => new InventoryHubWindow(),
                    "🎟️ Mã Giảm Giá" or "🎟️ Vouchers" => new VoucherManagementWindow(),
                    "🎁 Chương Trình KM" or "🎁 Promotions" => new PromotionManagementWindow(),
                    "👥 Khách Hàng" or "👥 Customers" => new CustomerManagementWindow(),
                    "🧾 Hóa Đơn" or "🧾 Invoices" => new InvoiceManagementWindow(),
                    "⏰ Ca Làm Việc" => new ShiftManagementWindow(),
                    "📊 Báo Cáo" or "📊 Reports" => new ReportsWindow(),
                    "💰 Chi Phí & Tài Chính" => new FinanceManagementWindow(),
                    "👤 Quản Lý Người Dùng" or "👤 Users" => new UserManagementWindow(),
                    "⚙️ Cài Đặt" or "⚙️ Settings" => new SettingsWindow(),
                    "🚪 Đăng Xuất" or "Logout" => null,
                    _ => null
                };

                if (tab == "🚪 Đăng Xuất" || tab == "Logout")
                {
                    if (_viewModel.LogoutCommand.CanExecute(this))
                    {
                        _viewModel.LogoutCommand.Execute(this);
                    }
                    return;
                }

                if (contentWindow != null)
                {
                    var content = contentWindow.Content as System.Windows.FrameworkElement;
                    if (content != null)
                    {
                        // Explicitly transfer DataContext to the content element 
                        // before it's moved to avoid inheriting DashboardViewModel
                        content.DataContext = contentWindow.DataContext;
                    }

                    contentWindow.Content = null;
                    MainContentHost.Content = content;
                    DefaultContentScrollViewer.Visibility = System.Windows.Visibility.Collapsed;
                    MainContentHost.Visibility = System.Windows.Visibility.Visible;

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

        // Static method to trigger dashboard refresh from other windows
        public static void TriggerDashboardRefresh()
        {
            DashboardViewModel.TriggerDashboardRefresh();
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

