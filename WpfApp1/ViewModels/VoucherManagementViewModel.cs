using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FashionStore.Services;
using FashionStore.Core;

namespace FashionStore.ViewModels
{
    public class VoucherManagementViewModel : BaseViewModel
    {
        private List<Voucher> _allVouchers = new();
        private readonly PaginationHelper<Voucher> _pagination = new();

        public ObservableCollection<Voucher> PagedVouchers { get; } = new();

        // -------- Filter Options --------
        public List<string> ActiveFilterOptions { get; } = new() { "All", "Active", "Inactive" };
        public List<string> DiscountTypeFilterOptions { get; } = new() { "All", "VND", "Percent" };
        public List<string> ValidityFilterOptions { get; } = new() { "All", "ValidNow", "Expired", "Upcoming" };
        public List<string> UsageFilterOptions { get; } = new() { "All", "Available", "FullyUsed" };

        private string _activeFilter = "All";
        public string ActiveFilter { get => _activeFilter; set { if (SetProperty(ref _activeFilter, value)) ApplyFilters(); } }

        private string _discountTypeFilter = "All";
        public string DiscountTypeFilter { get => _discountTypeFilter; set { if (SetProperty(ref _discountTypeFilter, value)) ApplyFilters(); } }

        private string _validityFilter = "All";
        public string ValidityFilter { get => _validityFilter; set { if (SetProperty(ref _validityFilter, value)) ApplyFilters(); } }

        private string _usageFilter = "All";
        public string UsageFilter { get => _usageFilter; set { if (SetProperty(ref _usageFilter, value)) ApplyFilters(); } }

        // -------- Pagination --------
        private string _pageInfo = "1 / 1";
        public string PageInfo { get => _pageInfo; set => SetProperty(ref _pageInfo, value); }

        private string _totalVouchersText = "0 mã";
        public string TotalVouchersText { get => _totalVouchersText; set => SetProperty(ref _totalVouchersText, value); }

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

        // -------- Form Fields --------
        private Voucher? _selectedVoucher;
        public Voucher? SelectedVoucher
        {
            get => _selectedVoucher;
            set { if (SetProperty(ref _selectedVoucher, value)) OnVoucherSelected(); }
        }

        private string _code = "";
        public string Code { get => _code; set => SetProperty(ref _code, value); }

        private int _discountTypeIndex;
        public int DiscountTypeIndex { get => _discountTypeIndex; set => SetProperty(ref _discountTypeIndex, value); }

        private string _discountValue = "";
        public string DiscountValue { get => _discountValue; set => SetProperty(ref _discountValue, value); }

        private string _minInvoice = "0";
        public string MinInvoice { get => _minInvoice; set => SetProperty(ref _minInvoice, value); }

        private DateTime? _startDate = DateTime.Today;
        public DateTime? StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }

        private DateTime? _endDate = DateTime.Today.AddDays(30);
        public DateTime? EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        private string _usageLimit = "0";
        public string UsageLimit { get => _usageLimit; set => SetProperty(ref _usageLimit, value); }

        private bool _isActive = true;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        // -------- Commands --------
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand FirstPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public VoucherManagementViewModel()
        {
            AddCommand = new RelayCommand(_ => AddVoucher());
            UpdateCommand = new RelayCommand(_ => UpdateVoucher());
            DeleteCommand = new RelayCommand(_ => DeleteVoucher());
            ClearFormCommand = new RelayCommand(_ => ClearForm());
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());
            FirstPageCommand = new RelayCommand(_ => { _pagination.FirstPage(); RefreshPage(); });
            PrevPageCommand = new RelayCommand(_ => { _pagination.PreviousPage(); RefreshPage(); });
            NextPageCommand = new RelayCommand(_ => { _pagination.NextPage(); RefreshPage(); });
            LastPageCommand = new RelayCommand(_ => { _pagination.LastPage(); RefreshPage(); });
            GoToPageCommand = new RelayCommand(p => GoToPage(p?.ToString()));

            _pagination.PageChanged += RefreshPage;
            _pagination.SetPageSize(6);
            Load();
        }

        private void Load()
        {
            _allVouchers = VoucherService.GetAllVouchers();
            _pagination.SetData(_allVouchers);
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            _pagination.SetFilter(v =>
            {
                bool matchesActive = ActiveFilter switch
                {
                    "Active" => v.IsActive,
                    "Inactive" => !v.IsActive,
                    _ => true
                };
                bool matchesType = DiscountTypeFilter switch
                {
                    "VND" => v.DiscountType == "VND",
                    "Percent" => v.DiscountType == "%",
                    _ => true
                };
                var now = DateTime.Now;
                bool matchesValidity = ValidityFilter switch
                {
                    "ValidNow" => v.StartDate <= now && v.EndDate >= now,
                    "Expired" => v.EndDate < now,
                    "Upcoming" => v.StartDate > now,
                    _ => true
                };
                bool matchesUsage = UsageFilter switch
                {
                    "Available" => v.UsedCount < v.UsageLimit,
                    "FullyUsed" => v.UsedCount >= v.UsageLimit,
                    _ => true
                };
                return matchesActive && matchesType && matchesValidity && matchesUsage;
            });
        }

        private void RefreshPage()
        {
            PagedVouchers.Clear();
            foreach (var v in _pagination.GetCurrentPageItems()) PagedVouchers.Add(v);
            PageInfo = _pagination.GetPageInfo();
            TotalVouchersText = $"{_pagination.TotalItems} mã";
            CurrentPageText = _pagination.CurrentPage.ToString();
            CanFirst = _pagination.CanGoFirst;
            CanPrev = _pagination.CanGoPrevious;
            CanNext = _pagination.CanGoNext;
            CanLast = _pagination.CanGoLast;
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
            else
            {
                CurrentPageText = _pagination.CurrentPage.ToString();
            }
        }

        private void OnVoucherSelected()
        {
            if (SelectedVoucher is Voucher v)
            {
                Code = v.Code;
                DiscountTypeIndex = (v.DiscountType == Voucher.TypePercentage || v.DiscountType == "%") ? 1 : 0;
                DiscountValue = v.DiscountValue.ToString("F0");
                MinInvoice = v.MinInvoiceAmount.ToString("F0");
                StartDate = v.StartDate;
                EndDate = v.EndDate;
                UsageLimit = v.UsageLimit.ToString();
                IsActive = v.IsActive;
            }
        }

        private bool ValidateAndBuild(out Voucher? voucher)
        {
            voucher = null;
            if (string.IsNullOrWhiteSpace(Code))
            {
                MessageBox.Show("Mã Voucher không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!decimal.TryParse(DiscountValue, out var val) || val < 0)
            {
                MessageBox.Show("Giá trị giảm giá không hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (DiscountTypeIndex == 1 && val > 100)
            {
                MessageBox.Show("Phần trăm giảm giá không được vượt quá 100%.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!decimal.TryParse(MinInvoice, out var min) || min < 0)
            {
                MessageBox.Show("Hóa đơn tối thiểu không hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!int.TryParse(UsageLimit, out var limit) || limit < 0)
            {
                MessageBox.Show("Giới hạn sử dụng không hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (StartDate == null || EndDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày bắt đầu và kết thúc.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (StartDate > EndDate)
            {
                MessageBox.Show("Ngày bắt đầu không được sau ngày kết thúc.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            voucher = new Voucher
            {
                Code = Code.Trim().ToUpper(),
                DiscountType = DiscountTypeIndex == 1 ? Voucher.TypePercentage : Voucher.TypeFixedAmount,
                DiscountValue = val,
                MinInvoiceAmount = min,
                StartDate = StartDate.Value,
                EndDate = EndDate.Value,
                UsageLimit = limit,
                IsActive = IsActive
            };
            return true;
        }

        private void AddVoucher()
        {
            if (!ValidateAndBuild(out var voucher) || voucher == null) return;
            if (VoucherService.AddVoucher(voucher))
            {
                MessageBox.Show("Thêm Voucher thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                Load(); ClearForm();
            }
            else MessageBox.Show("Có lỗi xảy ra. Mã voucher có thể đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void UpdateVoucher()
        {
            if (SelectedVoucher == null)
            {
                MessageBox.Show("Vui lòng chọn voucher cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!ValidateAndBuild(out var voucher) || voucher == null) return;
            voucher.Id = SelectedVoucher.Id;
            if (VoucherService.UpdateVoucher(voucher))
            {
                MessageBox.Show("Cập nhật Voucher thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                Load(); ClearForm();
            }
            else MessageBox.Show("Có lỗi xảy ra khi cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DeleteVoucher()
        {
            if (SelectedVoucher == null)
            {
                MessageBox.Show("Vui lòng chọn voucher cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var r = MessageBox.Show($"Bạn có chắc chắn muốn xóa '{SelectedVoucher.Code}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes)
            {
                if (VoucherService.DeleteVoucher(SelectedVoucher.Id))
                {
                    MessageBox.Show("Xóa Voucher thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    Load(); ClearForm();
                }
                else MessageBox.Show("Không thể xóa voucher này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            SelectedVoucher = null;
            Code = "";
            DiscountValue = "";
            MinInvoice = "0";
            UsageLimit = "0";
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(30);
            IsActive = true;
            DiscountTypeIndex = 0;
        }

        private void ResetFilters()
        {
            ActiveFilter = "All";
            DiscountTypeFilter = "All";
            ValidityFilter = "All";
            UsageFilter = "All";
        }
    }
}
