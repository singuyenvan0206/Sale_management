using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using FashionStore.Services;
using FashionStore.Core;
using FashionStore.Models;

namespace FashionStore.ViewModels
{
    public class CustomerItemViewModel : BaseViewModel
    {
        private int _id;
        public int Id { get => _id; set => SetProperty(ref _id, value); }

        private string _name = "";
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _phone = "";
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

        private string _email = "";
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _address = "";
        public string Address { get => _address; set => SetProperty(ref _address, value); }

        private string _customerType = "Regular";
        public string CustomerType { get => _customerType; set => SetProperty(ref _customerType, value); }

        private string _tier = "Regular";
        public string Tier { get => _tier; set => SetProperty(ref _tier, value); }

        private int _points;
        public int Points { get => _points; set => SetProperty(ref _points, value); }

        public CustomerItemViewModel Clone()
        {
            return (CustomerItemViewModel)this.MemberwiseClone();
        }
    }

    public class PurchaseHistoryItemViewModel
    {
        public int InvoiceId { get; set; }
        public string CreatedAt { get; set; } = "";
        public int ItemCount { get; set; }
        public string Total { get; set; } = "";
    }

    public class CustomerManagementViewModel : BaseViewModel
    {
        private PaginationHelper<CustomerItemViewModel> _paginationHelper = new();
        private List<CustomerItemViewModel> _allCustomers = new();

        public ObservableCollection<CustomerItemViewModel> PagedCustomers { get; } = new();
        public ObservableCollection<PurchaseHistoryItemViewModel> PurchaseHistory { get; } = new();

        private CustomerItemViewModel _editingCustomer = new();
        public CustomerItemViewModel EditingCustomer
        {
            get => _editingCustomer;
            set => SetProperty(ref _editingCustomer, value);
        }

        private CustomerItemViewModel? _selectedCustomer;
        public CustomerItemViewModel? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    if (value != null)
                    {
                        EditingCustomer = value.Clone();
                        LoadPurchaseHistory(value.Id);
                    }
                    else
                    {
                        ClearForm();
                        PurchaseHistory.Clear();
                    }
                }
            }
        }

        private bool _isAdminOrManager;
        public bool IsAdminOrManager
        {
            get => _isAdminOrManager;
            set => SetProperty(ref _isAdminOrManager, value);
        }

        private string _statusText = "Sẵn sàng";
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        private string _pageInfoText = "Trang: 1 / 1";
        public string PageInfoText { get => _pageInfoText; set => SetProperty(ref _pageInfoText, value); }

        private string _totalCustomersText = "0 khách hàng";
        public string TotalCustomersText { get => _totalCustomersText; set => SetProperty(ref _totalCustomersText, value); }

        private int _currentPageBox = 1;
        public int CurrentPageBox { get => _currentPageBox; set => SetProperty(ref _currentPageBox, value); }

        // Filters
        private string _searchTerm = "";
        public string SearchTerm
        {
            get => _searchTerm;
            set { if (SetProperty(ref _searchTerm, value)) ApplyFilters(); }
        }

        private string _selectedFilterTier = "All";
        public string SelectedFilterTier
        {
            get => _selectedFilterTier;
            set { if (SetProperty(ref _selectedFilterTier, value)) ApplyFilters(); }
        }

        private string _selectedFilterPoints = "All";
        public string SelectedFilterPoints
        {
            get => _selectedFilterPoints;
            set { if (SetProperty(ref _selectedFilterPoints, value)) ApplyFilters(); }
        }

        private string _selectedFilterActivity = "All";
        public string SelectedFilterActivity
        {
            get => _selectedFilterActivity;
            set { if (SetProperty(ref _selectedFilterActivity, value)) ApplyFilters(); }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand UpdateLoyaltyCommand { get; }
        public ICommand DeleteAllCommand { get; }
        public ICommand ImportCsvCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand ResetFiltersCommand { get; }

        public ICommand FirstPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public CustomerManagementViewModel()
        {
            CheckUserPermissions();

            _paginationHelper.PageChanged += OnPageChanged;
            DashboardViewModel.OnDashboardRefreshNeeded += HandleDashboardRefresh;

            AddCommand = new RelayCommand(_ => _ = AddCustomerAsync());
            UpdateCommand = new RelayCommand(_ => _ = UpdateCustomerAsync());
            DeleteCommand = new RelayCommand(_ => _ = DeleteCustomerAsync());
            ClearCommand = new RelayCommand(_ => ClearForm());
            UpdateLoyaltyCommand = new RelayCommand(_ => _ = UpdateLoyaltyAsync());
            DeleteAllCommand = new RelayCommand(_ => DeleteAllCustomers());
            ImportCsvCommand = new RelayCommand(_ => ImportCsv());
            ExportCsvCommand = new RelayCommand(_ => ExportCsv());
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());

            FirstPageCommand = new RelayCommand(_ => _paginationHelper.FirstPage());
            PrevPageCommand = new RelayCommand(_ => _paginationHelper.PreviousPage());
            NextPageCommand = new RelayCommand(_ => _paginationHelper.NextPage());
            LastPageCommand = new RelayCommand(_ => _paginationHelper.LastPage());
            GoToPageCommand = new RelayCommand(_ => GoToPage());

            _ = LoadCustomersAsync();
        }

        private void HandleDashboardRefresh()
        {
            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await LoadCustomersAsync();
            });
        }

        private void CheckUserPermissions()
        {
            var currentUser = Application.Current.Resources["CurrentUser"] as string;
            if (string.IsNullOrEmpty(currentUser))
            {
                IsAdminOrManager = false;
                return;
            }

            var userRole = UserService.GetUserRole(currentUser);
            IsAdminOrManager = ParseRole(userRole).CanManageTierSettings();
        }

        private static UserRole ParseRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role)) return UserRole.Cashier;
            switch (role.Trim().ToLower())
            {
                case "admin": return UserRole.Admin;
                case "manager": return UserRole.Manager;
                default: return UserRole.Cashier;
            }
        }

        private async System.Threading.Tasks.Task LoadCustomersAsync()
        {
            StatusText = "Đang tải...";
            // Run DB query on thread pool to not block UI
            var customers = await System.Threading.Tasks.Task.Run(() => CustomerService.GetAllCustomers());
            _allCustomers = customers.ConvertAll(c => new CustomerItemViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone ?? "",
                Email = c.Email ?? "",
                Address = c.Address ?? "",
                CustomerType = c.CustomerType ?? "Regular",
                // FIX: CustomerType IS the tier — avoid extra per-row DB call
                Tier = c.CustomerType ?? "Regular",
                Points = c.Points
            });
            _paginationHelper.SetData(_allCustomers);
            ApplyFilters();
            StatusText = $"Tìm thấy {_paginationHelper.TotalItems} khách hàng";
        }

        // Keep sync version for internal callers that need immediate reload
        private void LoadCustomers() => _ = LoadCustomersAsync();

        private async void LoadPurchaseHistory(int customerId)
        {
            PurchaseHistory.Clear();
            try
            {
                // Purchase history query runs on thread pool
                var history = await System.Threading.Tasks.Task.Run(() =>
                    CustomerService.GetCustomerPurchaseHistory(customerId));
                foreach (var h in history.Select(h => new PurchaseHistoryItemViewModel
                {
                    InvoiceId = h.InvoiceId,
                    CreatedAt = h.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    ItemCount = h.ItemCount,
                    Total = h.Total.ToString("N0")
                }))
                { PurchaseHistory.Add(h); }
            }
            catch { }
        }

        private void OnPageChanged()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            PagedCustomers.Clear();
            foreach (var item in _paginationHelper.GetCurrentPageItems())
            {
                PagedCustomers.Add(item);
            }

            PageInfoText = $"Trang: {_paginationHelper.GetPageInfo()}";
            TotalCustomersText = $"{_paginationHelper.TotalItems} khách hàng";
            CurrentPageBox = _paginationHelper.CurrentPage;
            StatusText = _paginationHelper.TotalItems == 1 ? "Tìm thấy 1 khách hàng" : $"Tìm thấy {_paginationHelper.TotalItems} khách hàng";
        }

        private void ClearForm()
        {
            EditingCustomer = new CustomerItemViewModel();
            SelectedCustomer = null;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(EditingCustomer.Name))
            {
                MessageBox.Show("Vui lòng nhập tên khách hàng.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditingCustomer.Phone))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(EditingCustomer.Email) && !IsValidEmail(EditingCustomer.Email))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ email hợp lệ.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch { return false; }
        }

        private async System.Threading.Tasks.Task AddCustomerAsync()
        {
            if (!ValidateInput()) return;
            var name = EditingCustomer.Name.Trim();
            bool ok = await System.Threading.Tasks.Task.Run(() =>
                CustomerService.AddCustomer(name, EditingCustomer.Phone.Trim(), EditingCustomer.Email.Trim(), "Regular", EditingCustomer.Address.Trim()));
            if (ok)
            {
                await LoadCustomersAsync();
                ClearForm();
                MessageBox.Show($"Khách hàng '{name}' đã được thêm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
                MessageBox.Show("Không thể thêm khách hàng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async System.Threading.Tasks.Task UpdateCustomerAsync()
        {
            if (SelectedCustomer == null) { MessageBox.Show("Vui lòng chọn khách hàng để cập nhật.", "Yêu cầu chọn", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            if (!ValidateInput()) return;
            var id = SelectedCustomer.Id;
            var name = EditingCustomer.Name.Trim();
            bool ok = await System.Threading.Tasks.Task.Run(() =>
                CustomerService.UpdateCustomer(id, name, EditingCustomer.Phone.Trim(), EditingCustomer.Email.Trim(), SelectedCustomer.CustomerType ?? "Regular", EditingCustomer.Address.Trim()));
            if (ok)
            {
                await LoadCustomersAsync();
                ClearForm();
                MessageBox.Show($"Khách hàng '{name}' đã được cập nhật thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
                MessageBox.Show("Không thể cập nhật khách hàng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async System.Threading.Tasks.Task DeleteCustomerAsync()
        {
            if (SelectedCustomer == null) { MessageBox.Show("Vui lòng chọn khách hàng để xóa."); return; }
            string customerName = SelectedCustomer.Name;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{customerName}'?\n\nHành động này không thể hoàn tác.", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            var id = SelectedCustomer.Id;
            bool ok = await System.Threading.Tasks.Task.Run(() => CustomerService.DeleteCustomer(id));
            if (ok) { await LoadCustomersAsync(); ClearForm(); MessageBox.Show($"Đã xóa '{customerName}' thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information); DashboardViewModel.TriggerDashboardRefresh(); }
            else MessageBox.Show("Không thể xóa. Khách hàng có thể đang được sử dụng trong hóa đơn.", "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private async System.Threading.Tasks.Task UpdateLoyaltyAsync()
        {
            if (SelectedCustomer == null) { MessageBox.Show("Vui lòng chọn khách hàng."); return; }
            var id = SelectedCustomer.Id;
            var points = EditingCustomer.Points;
            var tier = EditingCustomer.Tier;
            bool ok = await System.Threading.Tasks.Task.Run(() => CustomerService.UpdateCustomerLoyalty(id, points, tier));
            if (ok) { await LoadCustomersAsync(); MessageBox.Show("Đã cập nhật hạng/điểm.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information); DashboardViewModel.TriggerDashboardRefresh(); }
            else MessageBox.Show("Cập nhật thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DeleteAllCustomers()
        {
            if (_allCustomers.Count == 0) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa TẤT CẢ {_allCustomers.Count} khách hàng?\n\nHành động này không thể hoàn tác.", "Xác nhận xóa tất cả", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (CustomerService.DeleteAllCustomers())
                {
                    LoadCustomers();
                    ClearForm();
                    MessageBox.Show("Đã xóa tất cả khách hàng thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    DashboardViewModel.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Không thể xóa tất cả khách hàng. Vui lòng kiểm tra ràng buộc dữ liệu (hóa đơn liên quan).", "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ImportCsv()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv" };
            if (openFileDialog.ShowDialog() == true)
            {
                int importedCount = CustomerService.ImportCustomersFromCsv(openFileDialog.FileName);
                if (importedCount >= 0)
                {
                    LoadCustomers();
                    MessageBox.Show($"Đã nhập thành công {importedCount} khách hàng từ tệp CSV.", "Nhập thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    DashboardViewModel.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Không thể nhập khách hàng từ tệp CSV. Vui lòng kiểm tra định dạng tệp.", "Lỗi nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportCsv()
        {
            if (_allCustomers.Count == 0) return;
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = "customers.csv" };
            if (saveFileDialog.ShowDialog() == true)
            {
                if (CustomerService.ExportCustomersToCsv(saveFileDialog.FileName))
                    MessageBox.Show("Đã xuất khách hàng thành công sang tệp CSV.", "Xuất thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Không thể xuất khách hàng sang tệp CSV. Vui lòng thử lại.", "Lỗi xuất", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            string term = SearchTerm.ToLower();

            _paginationHelper.SetFilter(c =>
            {
                bool matchesSearch = string.IsNullOrWhiteSpace(term) ||
                    c.Name.ToLower().Contains(term) ||
                    c.Phone.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term) ||
                    c.CustomerType.ToLower().Contains(term) ||
                    c.Address.ToLower().Contains(term) ||
                    c.Tier.ToLower().Contains(term) ||
                    c.Points.ToString().Contains(term);

                bool matchesTier = SelectedFilterTier == "All" || string.Equals(c.Tier, SelectedFilterTier, StringComparison.OrdinalIgnoreCase);

                bool matchesPoints = SelectedFilterPoints switch
                {
                    "Under100" => c.Points < 100,
                    "100To500" => c.Points >= 100 && c.Points < 500,
                    "500To1000" => c.Points >= 500 && c.Points < 1000,
                    "Over1000" => c.Points >= 1000,
                    _ => true
                };

                // NOTE: HasPurchases filter removed from per-row evaluation to avoid N+1 DB queries.
                // Use search by name/phone instead for performance.
                bool matchesActivity = true; // Activity filter placeholder - implement with batch query if needed

                return matchesSearch && matchesTier && matchesPoints && matchesActivity;
            });
            UpdateDisplay();
        }

        private void ResetFilters()
        {
            SelectedFilterTier = "All";
            SelectedFilterPoints = "All";
            SelectedFilterActivity = "All";
            SearchTerm = "";
            ApplyFilters();
        }

        private void GoToPage()
        {
            if (!_paginationHelper.GoToPage(CurrentPageBox))
            {
                CurrentPageBox = _paginationHelper.CurrentPage;
            }
        }

        public void CallSort(DataGridColumn column)
        {
            var propertyName = column.SortMemberPath;
            if (string.IsNullOrEmpty(propertyName)) return;

            var direction = column.SortDirection != System.ComponentModel.ListSortDirection.Ascending
                ? System.ComponentModel.ListSortDirection.Ascending
                : System.ComponentModel.ListSortDirection.Descending;

            Func<IEnumerable<CustomerItemViewModel>, IOrderedEnumerable<CustomerItemViewModel>>? sortFunc = null;

            switch (propertyName.ToLower())
            {
                case "id": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(c => c.Id) : items => items.OrderByDescending(c => c.Id); break;
                case "name": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(c => c.Name) : items => items.OrderByDescending(c => c.Name); break;
                case "phone": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(c => c.Phone) : items => items.OrderByDescending(c => c.Phone); break;
                case "email": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(c => c.Email) : items => items.OrderByDescending(c => c.Email); break;
                case "address": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(c => c.Address) : items => items.OrderByDescending(c => c.Address); break;
                case "tier": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(c => c.Tier) : items => items.OrderByDescending(c => c.Tier); break;
                case "points": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(c => c.Points) : items => items.OrderByDescending(c => c.Points); break;
            }

            if (sortFunc != null)
            {
                _paginationHelper.SetSort(sortFunc);
                column.SortDirection = direction;
            }
        }
    }
}
