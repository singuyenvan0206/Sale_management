
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace WpfApp1
{
    public partial class ReportsWindow : Window
    {
        private List<InvoiceListItem> _invoices = new();
        private PaginationHelper<InvoiceListItem> _paginationHelper = new();

        public ReportsWindow()
        {
            InitializeComponent();
            _paginationHelper.PageChanged += OnPageChanged;
            LoadFilters();
            LoadInvoices();
            
            // Enable sorting for DataGrid
            InvoicesDataGrid.Sorting += InvoicesDataGrid_Sorting;
        }
        
        private void OnPageChanged()
        {
            UpdateDisplayAndPagination();
        }

        

        private void LoadFilters()
        {
            // Get oldest invoice date from database
            var (oldestDate, newestDate) = DatabaseHelper.GetInvoiceDateRange();
            
            // Set default date range: from oldest invoice to today
            ToDatePicker.SelectedDate = DateTime.Today;
            FromDatePicker.SelectedDate = oldestDate ?? DateTime.Today.AddYears(-1); // Fallback to 1 year ago if no invoices

            var customers = DatabaseHelper.GetAllCustomers();
            var list = new List<CustomerList> { new CustomerList { Id = 0, Name = "Tất cả khách hàng" } };
            list.AddRange(customers.ConvertAll(c => new CustomerList { Id = c.Id, Name = c.Name }));
            CustomerComboBox.ItemsSource = list;
            CustomerComboBox.SelectedIndex = 0;
        }

        private void LoadInvoices()
        {
            DateTime? from = FromDatePicker.SelectedDate;
            DateTime? to = ToDatePicker.SelectedDate?.AddDays(1).AddTicks(-1); // include end day
            int? customerId = (CustomerComboBox.SelectedValue as int?) ?? 0;
            string search = (SearchTextBox.Text ?? string.Empty).Trim();

            var data = DatabaseHelper.QueryInvoices(from, to, customerId == 0 ? null : customerId, search);
            _invoices = data.ConvertAll(i => new InvoiceListItem
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
            _paginationHelper.SetData(_invoices);
            UpdateDisplayAndPagination();
            RefreshKPIStats();
            CountTextBlock.Text = _paginationHelper.TotalItems.ToString();
            StatusTextBlock.Text = _paginationHelper.TotalItems == 0 ? "Không tìm thấy hóa đơn nào với bộ lọc đã chọn." : string.Empty;
        }

        private void UpdateDisplayAndPagination()
        {
            // Update DataGrid with current page items
            InvoicesDataGrid.ItemsSource = _paginationHelper.GetCurrentPageItems();
            
            // Update pagination info
            if (ReportsPageInfoTextBlock != null)
            {
                ReportsPageInfoTextBlock.Text = $"📄 Trang: {_paginationHelper.GetPageInfo()} • 📊 Tổng: {_paginationHelper.TotalItems} hóa đơn";
            }
            
            // Update current page textbox
            if (ReportsCurrentPageTextBox != null)
            {
                ReportsCurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
            }
            
            // Update button states
            if (ReportsFirstPageButton != null) ReportsFirstPageButton.IsEnabled = _paginationHelper.CanGoFirst;
            if (ReportsPrevPageButton != null) ReportsPrevPageButton.IsEnabled = _paginationHelper.CanGoPrevious;
            if (ReportsNextPageButton != null) ReportsNextPageButton.IsEnabled = _paginationHelper.CanGoNext;
            if (ReportsLastPageButton != null) ReportsLastPageButton.IsEnabled = _paginationHelper.CanGoLast;
        }

        private void RefreshKPIStats()
        {
            try
            {
                // Get today's data
                var today = DateTime.Today;
                var todayInvoices = DatabaseHelper.QueryInvoices(today, today, null, string.Empty);
                var todayRevenue = todayInvoices.Sum(i => i.Total);
                
                // Get 30 days data  
                var thirtyDaysAgo = today.AddDays(-30);
                var monthInvoices = DatabaseHelper.QueryInvoices(thirtyDaysAgo, today, null, string.Empty);
                var monthRevenue = monthInvoices.Sum(i => i.Total);
                
                // Get total customers and products
                var totalCustomers = DatabaseHelper.GetTotalCustomers();
                var totalProducts = DatabaseHelper.GetTotalProducts();

   
                    
                if (TotalInvoicesText != null)
                    TotalInvoicesText.Text = $"{_paginationHelper.TotalItems} hóa đơn";
                    
                if (RevenueTextBlock != null)
                    RevenueTextBlock.Text = $"{_invoices.Sum(x => x.Total):N2}₫";
                    
                // Update last refresh time
                if (LastUpdateText != null)
                    LastUpdateText.Text = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            }
            catch
            {
                // Silent failure
            }
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            LoadInvoices();
        }

       
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadInvoices();
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            FromDatePicker.SelectedDate = DateTime.Today;
            ToDatePicker.SelectedDate = DateTime.Today;
            LoadInvoices();
        }

        private void Last7DaysButton_Click(object sender, RoutedEventArgs e)
        {
            FromDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
            ToDatePicker.SelectedDate = DateTime.Today;
            LoadInvoices();
        }

        private void Last30DaysButton_Click(object sender, RoutedEventArgs e)
        {
            FromDatePicker.SelectedDate = DateTime.Today.AddDays(-30);
            ToDatePicker.SelectedDate = DateTime.Today;
            LoadInvoices();
        }

        private void ThisMonthButton_Click(object sender, RoutedEventArgs e)
        {
            var today = DateTime.Today;
            FromDatePicker.SelectedDate = new DateTime(today.Year, today.Month, 1);
            ToDatePicker.SelectedDate = DateTime.Today;
            LoadInvoices();
        }
        private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new ReportsSettingsWindow();
            try
            {
                settings.Owner = this;
                settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            catch (InvalidOperationException)
            {
                settings.Owner = null;
                settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            settings.ShowDialog();
        }

 

        
        // Pagination event handlers
        private void ReportsFirstPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.FirstPage();
        }

        private void ReportsPrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.PreviousPage();
        }

        private void ReportsNextPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.NextPage();
        }

        private void ReportsLastPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.LastPage();
        }

        private void ReportsCurrentPageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(ReportsCurrentPageTextBox.Text, out int pageNumber))
                {
                    if (!_paginationHelper.GoToPage(pageNumber))
                    {
                        // Reset to current page if invalid
                        ReportsCurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
                        MessageBox.Show($"Trang không hợp lệ. Vui lòng nhập từ 1 đến {_paginationHelper.TotalPages}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    ReportsCurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
                }
            }
        }
        
        private void InvoicesDataGrid_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            e.Handled = true; // Prevent default sorting
            
            var column = e.Column;
            var propertyName = column.SortMemberPath;
            
            if (string.IsNullOrEmpty(propertyName)) 
            {
                return;
            }
            
            // Determine sort direction
            var direction = column.SortDirection != System.ComponentModel.ListSortDirection.Ascending 
                ? System.ComponentModel.ListSortDirection.Ascending 
                : System.ComponentModel.ListSortDirection.Descending;
            
            // Apply sort to all data through PaginationHelper
            Func<IEnumerable<InvoiceListItem>, IOrderedEnumerable<InvoiceListItem>>? sortFunc = null;
            
            switch (propertyName.ToLower())
            {
                case "id":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(i => i.Id)
                        : items => items.OrderByDescending(i => i.Id);
                    break;
                case "createddate":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(i => i.CreatedDate)
                        : items => items.OrderByDescending(i => i.CreatedDate);
                    break;
                case "customername":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(i => i.CustomerName)
                        : items => items.OrderByDescending(i => i.CustomerName);
                    break;
                case "subtotal":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(i => i.Subtotal)
                        : items => items.OrderByDescending(i => i.Subtotal);
                    break;
                case "taxamount":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(i => i.TaxAmount)
                        : items => items.OrderByDescending(i => i.TaxAmount);
                    break;
                case "discount":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(i => i.DiscountAmount)
                        : items => items.OrderByDescending(i => i.DiscountAmount);
                    break;
                case "total":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(i => i.Total)
                        : items => items.OrderByDescending(i => i.Total);
                    break;
                case "paid":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(i => i.Paid)
                        : items => items.OrderByDescending(i => i.Paid);
                    break;
            }
            
            if (sortFunc != null)
            {
                _paginationHelper.SetSort(sortFunc);
                
                // Update column sort direction
                column.SortDirection = direction;
                
                // Clear other columns' sort direction
                foreach (var col in InvoicesDataGrid.Columns)
                {
                    if (col != column)
                        col.SortDirection = null;
                }
            }
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is InvoiceListItem row)
            {
                var detail = DatabaseHelper.GetInvoiceDetails(row.Id);
                var sb = new StringBuilder();
                sb.AppendLine($"Hóa đơn #{detail.Header.Id} - {detail.Header.CreatedDate:yyyy-MM-dd HH:mm}");
                sb.AppendLine($"Khách hàng: {detail.Header.CustomerName}");
                sb.AppendLine("Sản phẩm:");
                foreach (var it in detail.Items)
                {
                    sb.AppendLine($" - {it.ProductName} x{it.Quantity} @ {it.UnitPrice:F2} = {it.LineTotal:F2}");
                }
                sb.AppendLine($"Tạm tính: {detail.Header.Subtotal:F2}");
                sb.AppendLine($"Thuế: {detail.Header.TaxAmount:F2}");
                sb.AppendLine($"Giảm giá: {detail.Header.DiscountAmount:F2}");
                sb.AppendLine($"Tổng cộng: {detail.Header.Total:F2}");
                sb.AppendLine($"Đã trả: {detail.Header.Paid:F2}");
                MessageBox.Show(sb.ToString(), $"Hóa đơn #{detail.Header.Id}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is InvoiceListItem row)
            {
                var confirm = MessageBox.Show($"Xóa hóa đơn #{row.Id}?\nHành động này không thể hoàn tác.", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.Yes)
                {
                    if (DatabaseHelper.DeleteInvoice(row.Id))
                    {
                        LoadInvoices();
                        MessageBox.Show($"Hóa đơn #{row.Id} đã được xóa.", "Đã xóa", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }



        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is InvoiceListItem row)
            {
                var printWindow = new InvoicePrintWindow(row.Id, 1);
                printWindow.ShowDialog();
            }
        }

        private void Grid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (CustomerComboBox?.IsDropDownOpen == true)
            {
                e.Handled = true;
            }
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
