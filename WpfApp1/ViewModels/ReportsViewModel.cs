using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using FashionStore.Services;
using FashionStore.Core;

namespace FashionStore.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private List<InvoiceListItem> _allInvoices = new();
        private readonly PaginationHelper<InvoiceListItem> _pagination = new();

        // -------- Collections --------
        public ObservableCollection<InvoiceListItem> PagedInvoices { get; } = new();
        public ObservableCollection<CustomerList> Customers { get; } = new();

        // -------- Filters --------
        private DateTime? _fromDate = DateTime.Today.AddYears(-1);
        public DateTime? FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        private DateTime? _toDate = DateTime.Today;
        public DateTime? ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        private CustomerList? _selectedCustomer;
        public CustomerList? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        // -------- KPIs --------
        private string _totalInvoicesText = "";
        public string TotalInvoicesText { get => _totalInvoicesText; set => SetProperty(ref _totalInvoicesText, value); }

        private string _revenueText = "";
        public string RevenueText { get => _revenueText; set => SetProperty(ref _revenueText, value); }

        private string _lastUpdateText = "";
        public string LastUpdateText { get => _lastUpdateText; set => SetProperty(ref _lastUpdateText, value); }

        private string _statusText = "";
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        private string _countText = "";
        public string CountText { get => _countText; set => SetProperty(ref _countText, value); }

        // -------- Pagination --------
        private string _pageInfo = "";
        public string PageInfo { get => _pageInfo; set => SetProperty(ref _pageInfo, value); }

        private string _currentPageText = "1";
        public string CurrentPageText { get => _currentPageText; set => SetProperty(ref _currentPageText, value); }

        private bool _canFirst;
        public bool CanFirst { get => _canFirst; set => SetProperty(ref _canFirst, value); }

        private bool _canPrev;
        public bool CanPrev { get => _canPrev; set => SetProperty(ref _canPrev, value); }

        private bool _canNext;
        public bool CanNext { get => _canNext; set => SetProperty(ref _canNext, value); }

        private bool _canLast;
        public bool CanLast { get => _canLast; set => SetProperty(ref _canLast, value); }

        // -------- Commands --------
        public ICommand ApplyFilterCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand TodayCommand { get; }
        public ICommand Last7DaysCommand { get; }
        public ICommand Last30DaysCommand { get; }
        public ICommand ThisMonthCommand { get; }
        public ICommand DetailsCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand FirstPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }
        public ICommand GoToPageCommand { get; }
        public ICommand SortCommand { get; }

        public ReportsViewModel()
        {
            ApplyFilterCommand = new RelayCommand(_ => LoadInvoices());
            RefreshCommand = new RelayCommand(_ => LoadInvoices());
            TodayCommand = new RelayCommand(_ => { FromDate = ToDate = DateTime.Today; LoadInvoices(); });
            Last7DaysCommand = new RelayCommand(_ => { FromDate = DateTime.Today.AddDays(-7); ToDate = DateTime.Today; LoadInvoices(); });
            Last30DaysCommand = new RelayCommand(_ => { FromDate = DateTime.Today.AddDays(-30); ToDate = DateTime.Today; LoadInvoices(); });
            ThisMonthCommand = new RelayCommand(_ => { var t = DateTime.Today; FromDate = new DateTime(t.Year, t.Month, 1); ToDate = t; LoadInvoices(); });
            DetailsCommand = new RelayCommand(p => ShowDetails(p as InvoiceListItem));
            DeleteCommand = new RelayCommand(p => DeleteInvoice(p as InvoiceListItem));
            PrintCommand = new RelayCommand(p => PrintInvoice(p as InvoiceListItem));
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
            FirstPageCommand = new RelayCommand(_ => { _pagination.FirstPage(); RefreshPage(); });
            PrevPageCommand = new RelayCommand(_ => { _pagination.PreviousPage(); RefreshPage(); });
            NextPageCommand = new RelayCommand(_ => { _pagination.NextPage(); RefreshPage(); });
            LastPageCommand = new RelayCommand(_ => { _pagination.LastPage(); RefreshPage(); });
            GoToPageCommand = new RelayCommand(p => GoToPage(p?.ToString()));
            SortCommand = new RelayCommand(p => ApplySort(p?.ToString()));

            _pagination.PageChanged += RefreshPage;
            _pagination.SetPageSize(20);

            LoadFilters();
            LoadInvoices();
        }

        private void LoadFilters()
        {
            var (oldestDate, _) = InvoiceService.GetInvoiceDateRange();
            FromDate = oldestDate ?? DateTime.Today.AddYears(-1);
            ToDate = DateTime.Today;

            var customers = CustomerService.GetAllCustomers();
            Customers.Clear();
            Customers.Add(new CustomerList { Id = 0, Name = "Tất cả khách hàng" });
            foreach (var c in customers) Customers.Add(new CustomerList { Id = c.Id, Name = c.Name });
            SelectedCustomer = Customers.FirstOrDefault();
        }

        public void LoadInvoices()
        {
            var from = FromDate;
            var to = ToDate?.AddDays(1).AddTicks(-1);
            int? customerId = SelectedCustomer?.Id == 0 ? null : SelectedCustomer?.Id;
            var search = (SearchText ?? "").Trim();

            var data = InvoiceService.QueryInvoices(from, to, customerId, search);
            _allInvoices = data.ConvertAll(i => new InvoiceListItem
            {
                Id = i.Id,
                CreatedDate = i.CreatedDate,
                CustomerName = i.CustomerName,
                Subtotal = i.Subtotal,
                TaxAmount = i.TaxAmount,
                DiscountAmount = i.Discount,
                Total = i.Total,
                Paid = i.Paid
            });

            _pagination.SetData(_allInvoices);
            RefreshPage();
            RefreshKPIs();

            CountText = _pagination.TotalItems.ToString();
            StatusText = _pagination.TotalItems == 0 ? "Không tìm thấy hóa đơn nào với bộ lọc đã chọn." : string.Empty;
        }

        private void RefreshPage()
        {
            PagedInvoices.Clear();
            foreach (var i in _pagination.GetCurrentPageItems()) PagedInvoices.Add(i);
            PageInfo = $"📄 Trang: {_pagination.GetPageInfo()} • 📝 Tổng: {_pagination.TotalItems} hóa đơn";
            CurrentPageText = _pagination.CurrentPage.ToString();
            CanFirst = _pagination.CanGoFirst;
            CanPrev = _pagination.CanGoPrevious;
            CanNext = _pagination.CanGoNext;
            CanLast = _pagination.CanGoLast;
        }

        private void RefreshKPIs()
        {
            try
            {
                TotalInvoicesText = $"{_pagination.TotalItems} hóa đơn";
                RevenueText = $"{_allInvoices.Sum(x => x.Total):N2}₫";
                LastUpdateText = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            }
            catch { }
        }

        private void GoToPage(string? text)
        {
            if (int.TryParse(text, out int page))
            {
                if (!_pagination.GoToPage(page))
                {
                    CurrentPageText = _pagination.CurrentPage.ToString();
                    MessageBox.Show($"Trang không hợp lệ. Vui lòng nhập từ 1 đến {_pagination.TotalPages}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else CurrentPageText = _pagination.CurrentPage.ToString();
        }

        private string? _currentSortProperty;
        private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;

        public void ApplySort(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return;

            if (_currentSortProperty == propertyName)
                _currentSortDirection = _currentSortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending : ListSortDirection.Ascending;
            else
            {
                _currentSortProperty = propertyName;
                _currentSortDirection = ListSortDirection.Ascending;
            }

            Func<IEnumerable<InvoiceListItem>, IOrderedEnumerable<InvoiceListItem>>? sortFunc = propertyName.ToLower() switch
            {
                "id" => _currentSortDirection == ListSortDirection.Ascending
                    ? items => items.OrderBy(i => i.Id) : items => items.OrderByDescending(i => i.Id),
                "createddate" => _currentSortDirection == ListSortDirection.Ascending
                    ? items => items.OrderBy(i => i.CreatedDate) : items => items.OrderByDescending(i => i.CreatedDate),
                "customername" => _currentSortDirection == ListSortDirection.Ascending
                    ? items => items.OrderBy(i => i.CustomerName) : items => items.OrderByDescending(i => i.CustomerName),
                "subtotal" => _currentSortDirection == ListSortDirection.Ascending
                    ? items => items.OrderBy(i => i.Subtotal) : items => items.OrderByDescending(i => i.Subtotal),
                "total" => _currentSortDirection == ListSortDirection.Ascending
                    ? items => items.OrderBy(i => i.Total) : items => items.OrderByDescending(i => i.Total),
                _ => null
            };

            if (sortFunc != null) { _pagination.SetSort(sortFunc); }
        }

        private void ShowDetails(InvoiceListItem? row)
        {
            if (row == null) return;
            var detail = InvoiceService.GetInvoiceDetails(row.Id);
            var sb = new StringBuilder();
            sb.AppendLine($"Hóa đơn #{detail.Header.Id} - {detail.Header.CreatedDate:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Khách hàng: {detail.Header.CustomerName}");
            sb.AppendLine("Sản phẩm:");
            foreach (var it in detail.Items)
                sb.AppendLine($" - {it.ProductName} x{it.Quantity} @ {it.UnitPrice:F2} = {it.LineTotal:F2}");
            sb.AppendLine($"Tạm tính: {detail.Header.Subtotal:F2}");
            sb.AppendLine($"Thuế: {detail.Header.TaxAmount:F2}");
            sb.AppendLine($"Giảm giá: {detail.Header.DiscountAmount:F2}");
            sb.AppendLine($"Tổng cộng: {detail.Header.Total:F2}");
            sb.AppendLine($"Đã trả: {detail.Header.Paid:F2}");
            MessageBox.Show(sb.ToString(), $"Hóa đơn #{detail.Header.Id}", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteInvoice(InvoiceListItem? row)
        {
            if (row == null) return;
            var confirm = MessageBox.Show($"Xóa hóa đơn #{row.Id}?\nHành động này không thể hoàn tác.", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm == MessageBoxResult.Yes)
            {
                if (InvoiceService.DeleteInvoice(row.Id)) { LoadInvoices(); MessageBox.Show($"Hóa đơn #{row.Id} đã được xóa.", "Đã xóa", MessageBoxButton.OK, MessageBoxImage.Information); }
                else MessageBox.Show("Xóa thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintInvoice(InvoiceListItem? row)
        {
            if (row == null) return;
            var printWindow = new InvoicePrintWindow(row.Id, 1);
            printWindow.ShowDialog();
        }

        private void OpenSettings()
        {
            var settings = new ReportsSettingsWindow();
            settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            settings.ShowDialog();
        }
    }

    public class InvoiceListItem
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public decimal Paid { get; set; }
    }

    public class CustomerList
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
