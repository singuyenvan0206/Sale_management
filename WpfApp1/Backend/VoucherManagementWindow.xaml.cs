using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfApp1
{
    public partial class VoucherManagementWindow : Window
    {
        private PaginationHelper<Voucher> _paginationHelper = new();
        private Voucher? _selectedVoucher;
        
        // Filter state
        private string _selectedActiveFilter = "All";
        private string _selectedDiscountTypeFilter = "All";
        private string _selectedValidityFilter = "All";
        private string _selectedUsageFilter = "All";

        public VoucherManagementWindow()
        {
            InitializeComponent();
            _paginationHelper.PageChanged += RefreshVoucherList;
            _paginationHelper.SetPageSize(6);
            LoadVouchers();
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(30);
        }

        private void LoadVouchers()
        {
            var vouchers = DatabaseHelper.GetAllVouchers();
            _paginationHelper.SetData(vouchers);
            ApplyFilters();
        }

        private void RefreshVoucherList()
        {
            VouchersListBox.ItemsSource = _paginationHelper.GetCurrentPageItems();
            
            // Update UI controls
            if (VoucherPageInfoTextBlock != null)
                VoucherPageInfoTextBlock.Text = _paginationHelper.GetPageInfo();
                
            if (TotalVouchersTextBlock != null)
                TotalVouchersTextBlock.Text = $"{_paginationHelper.TotalItems} mã";
                
            if (VoucherCurrentPageTextBox != null)
                VoucherCurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
                
            // Update button states
            if (VoucherFirstPageButton != null) VoucherFirstPageButton.IsEnabled = _paginationHelper.CanGoFirst;
            if (VoucherPrevPageButton != null) VoucherPrevPageButton.IsEnabled = _paginationHelper.CanGoPrevious;
            if (VoucherNextPageButton != null) VoucherNextPageButton.IsEnabled = _paginationHelper.CanGoNext;
            if (VoucherLastPageButton != null) VoucherLastPageButton.IsEnabled = _paginationHelper.CanGoLast;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var voucher = new Voucher
            {
                Code = CodeTextBox.Text.Trim().ToUpper(),
                DiscountType = (DiscountTypeComboBox.SelectedIndex == 1) ? Voucher.TypePercentage : Voucher.TypeFixedAmount,
                DiscountValue = decimal.Parse(DiscountValueTextBox.Text),
                MinInvoiceAmount = decimal.Parse(MinInvoiceTextBox.Text),
                StartDate = StartDatePicker.SelectedDate ?? DateTime.Today,
                EndDate = EndDatePicker.SelectedDate ?? DateTime.Today,
                UsageLimit = int.Parse(UsageLimitTextBox.Text),
                IsActive = IsActiveCheckBox.IsChecked ?? true

            };

            if (DatabaseHelper.AddVoucher(voucher))
            {
                MessageBox.Show("Thêm Voucher thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadVouchers();
                ClearForm();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra. Mã voucher có thể đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (VouchersListBox.SelectedItem is not Voucher selected)
            {
                MessageBox.Show("Vui lòng chọn voucher cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInput()) return;

            selected.Code = CodeTextBox.Text.Trim().ToUpper();
            selected.DiscountType = (DiscountTypeComboBox.SelectedIndex == 1) ? Voucher.TypePercentage : Voucher.TypeFixedAmount;
            selected.DiscountValue = decimal.Parse(DiscountValueTextBox.Text);
            selected.MinInvoiceAmount = decimal.Parse(MinInvoiceTextBox.Text);
            selected.StartDate = StartDatePicker.SelectedDate ?? DateTime.Today;
            selected.EndDate = EndDatePicker.SelectedDate ?? DateTime.Today;
            selected.UsageLimit = int.Parse(UsageLimitTextBox.Text);
            selected.IsActive = IsActiveCheckBox.IsChecked ?? true;


            if (DatabaseHelper.UpdateVoucher(selected))
            {
                MessageBox.Show("Cập nhật Voucher thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadVouchers();
                ClearForm();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (VouchersListBox.SelectedItem is not Voucher selected)
            {
                MessageBox.Show("Vui lòng chọn voucher cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa '{selected.Code}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (DatabaseHelper.DeleteVoucher(selected.Id))
                {
                    MessageBox.Show("Xóa Voucher thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadVouchers();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Không thể xóa voucher này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            CodeTextBox.Clear();
            DiscountValueTextBox.Clear();
            MinInvoiceTextBox.Text = "0";
            UsageLimitTextBox.Text = "0";
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(30);
            IsActiveCheckBox.IsChecked = true;
            DiscountTypeComboBox.SelectedIndex = 0;
            VouchersListBox.SelectedItem = null;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(CodeTextBox.Text))
            {
                MessageBox.Show("Mã Voucher không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                CodeTextBox.Focus();
                return false;
            }
            if (!decimal.TryParse(DiscountValueTextBox.Text, out var val) || val < 0)
            {
                MessageBox.Show("Giá trị giảm giá không hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                DiscountValueTextBox.Focus();
                return false;
            }
            if (DiscountTypeComboBox.SelectedIndex == 1 && val > 100)
            {
                MessageBox.Show("Phần trăm giảm giá không được vượt quá 100%.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                DiscountValueTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(MinInvoiceTextBox.Text, out var min) || min < 0)
            {
                MessageBox.Show("Hóa đơn tối thiểu không hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                MinInvoiceTextBox.Focus();
                return false;
            }
            if (!int.TryParse(UsageLimitTextBox.Text, out var limit) || limit < 0)
            {
                MessageBox.Show("Giới hạn sử dụng không hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                UsageLimitTextBox.Focus();
                return false;
            }
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày bắt đầu và kết thúc.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                MessageBox.Show("Ngày bắt đầu không được sau ngày kết thúc.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void VouchersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VouchersListBox.SelectedItem is Voucher selected)
            {
                CodeTextBox.Text = selected.Code;
                DiscountTypeComboBox.SelectedIndex = (selected.DiscountType == Voucher.TypePercentage || selected.DiscountType == "%") ? 1 : 0;
                DiscountValueTextBox.Text = selected.DiscountValue.ToString("F0"); 

                MinInvoiceTextBox.Text = selected.MinInvoiceAmount.ToString("F0");
                StartDatePicker.SelectedDate = selected.StartDate;
                EndDatePicker.SelectedDate = selected.EndDate;
                UsageLimitTextBox.Text = selected.UsageLimit.ToString();
                IsActiveCheckBox.IsChecked = selected.IsActive;
            }
        }
        
        private void ApplyFilters()
        {
            _paginationHelper.SetFilter(v =>
            {
                // Active status filter
                bool matchesActive = _selectedActiveFilter switch
                {
                    "Active" => v.IsActive,
                    "Inactive" => !v.IsActive,
                    _ => true // "All"
                };
                
                // Discount type filter
                bool matchesDiscountType = _selectedDiscountTypeFilter switch
                {
                    "VND" => v.DiscountType == "VND",
                    "Percent" => v.DiscountType == "%",
                    _ => true // "All"
                };
                
                // Validity filter
                DateTime now = DateTime.Now;
                bool matchesValidity = _selectedValidityFilter switch
                {
                    "ValidNow" => v.StartDate <= now && v.EndDate >= now,
                    "Expired" => v.EndDate < now,
                    "Upcoming" => v.StartDate > now,
                    _ => true // "All"
                };
                
                // Usage filter
                bool matchesUsage = _selectedUsageFilter switch
                {
                    "Available" => v.UsedCount < v.UsageLimit,
                    "FullyUsed" => v.UsedCount >= v.UsageLimit,
                    _ => true // "All"
                };
                
                // Combine all filters with AND logic
                return matchesActive && matchesDiscountType && matchesValidity && matchesUsage;
            });
        }
        
        private void VoucherFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update filter state based on which ComboBox changed
            if (sender == FilterActiveComboBox && FilterActiveComboBox.SelectedItem is ComboBoxItem activeItem)
            {
                _selectedActiveFilter = activeItem.Tag?.ToString() ?? "All";
            }
            else if (sender == FilterDiscountTypeComboBox && FilterDiscountTypeComboBox.SelectedItem is ComboBoxItem typeItem)
            {
                _selectedDiscountTypeFilter = typeItem.Tag?.ToString() ?? "All";
            }
            else if (sender == FilterValidityComboBox && FilterValidityComboBox.SelectedItem is ComboBoxItem validityItem)
            {
                _selectedValidityFilter = validityItem.Tag?.ToString() ?? "All";
            }
            else if (sender == FilterUsageComboBox && FilterUsageComboBox.SelectedItem is ComboBoxItem usageItem)
            {
                _selectedUsageFilter = usageItem.Tag?.ToString() ?? "All";
            }
            
            // Apply filters
            ApplyFilters();
        }
        
        private void ResetVoucherFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset all filter ComboBoxes to default
            FilterActiveComboBox.SelectedIndex = 0;
            FilterDiscountTypeComboBox.SelectedIndex = 0;
            FilterValidityComboBox.SelectedIndex = 0;
            FilterUsageComboBox.SelectedIndex = 0;
            
            // Reset filter state
            _selectedActiveFilter = "All";
            _selectedDiscountTypeFilter = "All";
            _selectedValidityFilter = "All";
            _selectedUsageFilter = "All";
            
            // Reapply filters (which will show all vouchers)
            ApplyFilters();
        }

        // Pagination Event Handlers
        private void VoucherFirstPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.FirstPage();
        }

        private void VoucherPrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.PreviousPage();
        }

        private void VoucherNextPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.NextPage();
        }

        private void VoucherLastPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.LastPage();
        }

        private void VoucherCurrentPageTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (int.TryParse(VoucherCurrentPageTextBox.Text, out int pageNumber))
                {
                    if (!_paginationHelper.GoToPage(pageNumber))
                    {
                        VoucherCurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
                        MessageBox.Show($"Trang không hợp lệ. Vui lòng nhập từ 1 đến {_paginationHelper.TotalPages}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    VoucherCurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
                }
            }
        }

        private void Grid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (DiscountTypeComboBox?.IsDropDownOpen == true ||
                FilterActiveComboBox?.IsDropDownOpen == true ||
                FilterDiscountTypeComboBox?.IsDropDownOpen == true ||
                FilterValidityComboBox?.IsDropDownOpen == true ||
                FilterUsageComboBox?.IsDropDownOpen == true)
            {
                e.Handled = true;
            }
        }
    }

    public class BooleanToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? new SolidColorBrush(Color.FromRgb(76, 175, 80)) : new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Active" : "Inactive";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
