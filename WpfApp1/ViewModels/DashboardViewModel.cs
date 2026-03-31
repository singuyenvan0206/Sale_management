using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using FashionStore.Services;
using FashionStore.Core;
using FashionStore.Models;

namespace FashionStore.ViewModels
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
            string employeeName = UserService.GetEmployeeName(username);
            UserInfoText = $"Chào mừng, {employeeName} ({role})";
            LoadKpis();
        }

        public void LoadKpis()
        {
            try
            {
                var todayStart = DateTime.Today;
                var todayEnd = todayStart.AddDays(1).AddTicks(-1);
                var monthStart = DateTime.Today.AddDays(-30);
                var monthEnd = todayEnd;

                decimal revenueToday = InvoiceService.GetRevenueBetween(todayStart, todayEnd);
                decimal revenue30 = InvoiceService.GetRevenueBetween(monthStart, monthEnd);
                int invoicesToday = InvoiceService.GetInvoiceCountBetween(todayStart, todayEnd);
                int totalCustomers = CustomerService.GetTotalCustomers();
                int totalProducts = ProductService.GetTotalProducts();

                RevenueTodayText = $"{revenueToday:N0}₫";
                Revenue30Text = $"{revenue30:N0}₫";
                InvoicesTodayText = invoicesToday.ToString();
                CountsText = $"{totalCustomers} / {totalProducts}";

                LoadHomeCharts(monthStart, monthEnd);
            }
            catch { }
        }

        private void LoadHomeCharts(DateTime monthStart, DateTime monthEnd)
        {
            // Revenue line chart
            var revenuePoints = InvoiceService.GetRevenueByDay(monthStart, monthEnd);
            var revenueModel = new PlotModel();
            revenueModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, StringFormat = "MM-dd", IsZoomEnabled = false, IsPanEnabled = false });
            revenueModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Doanh thu (VND)", StringFormat = "#,##0", Minimum = 0, IsZoomEnabled = false, IsPanEnabled = false });
            var line = new LineSeries { MarkerType = MarkerType.Circle };
            foreach (var (day, amount) in revenuePoints)
            {
                line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(day), (double)amount));
            }
            revenueModel.Series.Add(line);
            HomeRevenuePlotModel = revenueModel;

            // Category pie chart
            var byCat = InvoiceService.GetRevenueByCategory(DateTime.MinValue, DateTime.MaxValue, 10000);
            var catModel = new PlotModel();
            var pie = new PieSeries { InsideLabelPosition = 0.7 };
            foreach (var (name, revenue) in byCat)
            {
                pie.Slices.Add(new PieSlice(name, (double)revenue));
            }
            catModel.Series.Add(pie);
            HomeCategoryPieModel = catModel;

            // Customers chart
            var topCustomers = CustomerService.GetTopCustomers(10);
            var customerModel = new PlotModel();
            customerModel.Axes.Add(new CategoryAxis { Position = AxisPosition.Left, IsZoomEnabled = false, IsPanEnabled = false });
            customerModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Tổng chi tiêu (VND)", StringFormat = "#,##0", IsZoomEnabled = false, IsPanEnabled = false });
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
            TopCustomersPlotModel = customerModel;

            // Products chart
            var topProducts = ProductService.GetTopProducts(10);
            var productsModel = new PlotModel();
            productsModel.Axes.Add(new CategoryAxis { Position = AxisPosition.Left, IsZoomEnabled = false, IsPanEnabled = false });
            productsModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Số lượng", IsZoomEnabled = false, IsPanEnabled = false });
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
            TopProductsPlotModel = productsModel;

            LoadLowStockAlert();
        }

        private void LoadLowStockAlert()
        {
            try
            {
                var lowStock = ProductService.GetLowStockProducts(10);
                var count = lowStock.Count;
                LowStockAlertCount = count.ToString();
                IsLowStockAlertEnabled = count > 0;
            }
            catch { }
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
