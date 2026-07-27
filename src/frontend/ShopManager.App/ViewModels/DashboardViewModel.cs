using ShopManager.App.Core;
using ShopManager.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Windows;
using System.Windows.Input;

namespace ShopManager.App.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public static event Action? OnDashboardRefreshNeeded;
        public static void TriggerDashboardRefresh() => OnDashboardRefreshNeeded?.Invoke();

        private string _userInfoText = string.Empty;
        public string UserInfoText
        {
            get => _userInfoText;
            set => SetProperty(ref _userInfoText, value);
        }

        private string _revenueTodayText = "0₫";
        public string RevenueTodayText
        {
            get => _revenueTodayText;
            set => SetProperty(ref _revenueTodayText, value);
        }

        private string _revenue30Text = "0₫";
        public string Revenue30Text
        {
            get => _revenue30Text;
            set => SetProperty(ref _revenue30Text, value);
        }

        private string _invoicesTodayText = "0";
        public string InvoicesTodayText
        {
            get => _invoicesTodayText;
            set => SetProperty(ref _invoicesTodayText, value);
        }

        private string _countsText = "0 / 0";
        public string CountsText
        {
            get => _countsText;
            set => SetProperty(ref _countsText, value);
        }

        private string _lowStockAlertCount = "0";
        public string LowStockAlertCount
        {
            get => _lowStockAlertCount;
            set => SetProperty(ref _lowStockAlertCount, value);
        }

        private bool _isLowStockAlertEnabled;
        public bool IsLowStockAlertEnabled
        {
            get => _isLowStockAlertEnabled;
            set => SetProperty(ref _isLowStockAlertEnabled, value);
        }

        private PlotModel? _homeRevenuePlotModel;
        public PlotModel? HomeRevenuePlotModel
        {
            get => _homeRevenuePlotModel;
            set => SetProperty(ref _homeRevenuePlotModel, value);
        }

        private PlotModel? _homeCategoryPieModel;
        public PlotModel? HomeCategoryPieModel
        {
            get => _homeCategoryPieModel;
            set => SetProperty(ref _homeCategoryPieModel, value);
        }

        private PlotModel? _topCustomersPlotModel;
        public PlotModel? TopCustomersPlotModel
        {
            get => _topCustomersPlotModel;
            set => SetProperty(ref _topCustomersPlotModel, value);
        }

        private PlotModel? _topProductsPlotModel;
        public PlotModel? TopProductsPlotModel
        {
            get => _topProductsPlotModel;
            set => SetProperty(ref _topProductsPlotModel, value);
        }

        public ICommand LogoutCommand { get; }

        public DashboardViewModel()
        {
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        public void Initialize(string username, string role)
        {
            _ = InitializeAsync(username, role);
        }

        private async System.Threading.Tasks.Task InitializeAsync(string username, string role)
        {
            string employeeName = await System.Threading.Tasks.Task.Run(() => UserService.GetEmployeeName(username) ?? username);
            UserInfoText = $"Chào mừng, {employeeName} ({role})";
            await LoadKpisAsync();
        }

        public void LoadKpis() => _ = LoadKpisAsync();

        public async System.Threading.Tasks.Task LoadKpisAsync()
        {
            try
            {
                var todayStart = DateTime.Today;
                var todayEnd = todayStart.AddDays(1).AddTicks(-1);
                var monthStart = DateTime.Today.AddDays(-30);
                var monthEnd = todayEnd;

                var revenueTodayTask = System.Threading.Tasks.Task.Run(() => InvoiceService.GetRevenueBetween(todayStart, todayEnd));
                var revenue30Task = System.Threading.Tasks.Task.Run(() => InvoiceService.GetRevenueBetween(monthStart, monthEnd));
                var invoicesTodayTask = System.Threading.Tasks.Task.Run(() => InvoiceService.GetInvoiceCountBetween(todayStart, todayEnd));
                var totalCustomersTask = System.Threading.Tasks.Task.Run(() => CustomerService.GetTotalCustomers());
                var totalProductsTask = System.Threading.Tasks.Task.Run(() => ProductService.GetTotalProducts());

                await System.Threading.Tasks.Task.WhenAll(
                    revenueTodayTask, revenue30Task, invoicesTodayTask, totalCustomersTask, totalProductsTask
                );

                RevenueTodayText = $"{revenueTodayTask.Result:N0}₫";
                Revenue30Text = $"{revenue30Task.Result:N0}₫";
                InvoicesTodayText = invoicesTodayTask.Result.ToString();
                CountsText = $"{totalCustomersTask.Result} / {totalProductsTask.Result}";

                await LoadHomeChartsAsync(monthStart, monthEnd);
            }
            catch (Exception ex)
            {
                AppLogger.Log(ex, "DashboardViewModel.LoadKpisAsync");
            }
        }

        private async System.Threading.Tasks.Task LoadHomeChartsAsync(DateTime monthStart, DateTime monthEnd)
        {
            try
            {
                var revenueTask = System.Threading.Tasks.Task.Run(() => InvoiceService.GetRevenueByDay(monthStart, monthEnd));
                var categoryTask = System.Threading.Tasks.Task.Run(() => InvoiceService.GetRevenueByCategory(DateTime.MinValue, DateTime.MaxValue, 10000));
                var customersTask = System.Threading.Tasks.Task.Run(() => CustomerService.GetTopCustomers(10));
                var productsTask = System.Threading.Tasks.Task.Run(() => ProductService.GetTopProducts(10));
                var lowStockTask = System.Threading.Tasks.Task.Run(() => ProductService.GetLowStockProducts(10));

                await System.Threading.Tasks.Task.WhenAll(revenueTask, categoryTask, customersTask, productsTask, lowStockTask);

                // Revenue line chart
                var revenueModel = new PlotModel();
                revenueModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "MM-dd", IsZoomEnabled = false, IsPanEnabled = false });
                revenueModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Doanh thu (VND)", StringFormat = "#,##0", Minimum = 0, IsZoomEnabled = false, IsPanEnabled = false });
                var line = new LineSeries { MarkerType = MarkerType.Circle };
                foreach (var (day, amount) in revenueTask.Result)
                {
                    line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(day), (double)amount));
                }
                revenueModel.Series.Add(line);
                HomeRevenuePlotModel = revenueModel;

                // Category pie chart
                var catModel = new PlotModel();
                var pie = new PieSeries { InsideLabelPosition = 0.7 };
                foreach (var (name, revenue) in categoryTask.Result)
                {
                    pie.Slices.Add(new PieSlice(name, (double)revenue));
                }
                catModel.Series.Add(pie);
                HomeCategoryPieModel = catModel;

                // Customers chart
                var customerModel = new PlotModel();
                customerModel.Axes.Add(new CategoryAxis { Position = AxisPosition.Left, IsZoomEnabled = false, IsPanEnabled = false });
                customerModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Tổng chi tiêu (VND)", StringFormat = "#,##0", IsZoomEnabled = false, IsPanEnabled = false });
                var barSeries = new BarSeries { FillColor = OxyColors.Blue };
                var labels = new List<string>();
                foreach (var (name, spent) in customersTask.Result)
                {
                    labels.Add(name.Length > 15 ? name.Substring(0, 15) + "..." : name);
                    barSeries.Items.Add(new BarItem((double)spent));
                }
                if (customersTask.Result.Count > 0)
                {
                    ((CategoryAxis)customerModel.Axes[0]).Labels.AddRange(labels);
                    customerModel.Series.Add(barSeries);
                }
                TopCustomersPlotModel = customerModel;

                // Products chart
                var productsModel = new PlotModel();
                productsModel.Axes.Add(new CategoryAxis { Position = AxisPosition.Left, IsZoomEnabled = false, IsPanEnabled = false });
                productsModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Số lượng", IsZoomEnabled = false, IsPanEnabled = false });
                var productsBarSeries = new BarSeries { FillColor = OxyColors.Orange };
                var productsLabels = new List<string>();
                foreach (var (name, qty, revenue) in productsTask.Result)
                {
                    productsLabels.Add(name.Length > 15 ? name.Substring(0, 15) + "..." : name);
                    productsBarSeries.Items.Add(new BarItem(qty));
                }
                if (productsTask.Result.Count > 0)
                {
                    ((CategoryAxis)productsModel.Axes[0]).Labels.AddRange(productsLabels);
                    productsModel.Series.Add(productsBarSeries);
                }
                TopProductsPlotModel = productsModel;

                // Low stock alert
                var count = lowStockTask.Result.Count;
                LowStockAlertCount = count.ToString();
                IsLowStockAlertEnabled = count > 0;
            }
            catch (Exception ex)
            {
                AppLogger.Log(ex, "DashboardViewModel.LoadHomeChartsAsync");
            }
        }

        private void ExecuteLogout(object? parameter)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new MainWindow();
                Application.Current.MainWindow = loginWindow;
                loginWindow.Show();

                if (parameter is Window window)
                {
                    window.Close();
                }
            }
        }
    }
}
