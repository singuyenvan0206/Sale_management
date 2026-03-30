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

            AddCommand = new RelayCommand(_ => AddCustomer());
            UpdateCommand = new RelayCommand(_ => UpdateCustomer());
            DeleteCommand = new RelayCommand(_ => DeleteCustomer());
            ClearCommand = new RelayCommand(_ => ClearForm());
            UpdateLoyaltyCommand = new RelayCommand(_ => UpdateLoyalty());
            DeleteAllCommand = new RelayCommand(_ => DeleteAllCustomers());
            ImportCsvCommand = new RelayCommand(_ => ImportCsv());
            ExportCsvCommand = new RelayCommand(_ => ExportCsv());
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());

            FirstPageCommand = new RelayCommand(_ => _paginationHelper.FirstPage());
            PrevPageCommand = new RelayCommand(_ => _paginationHelper.PreviousPage());
            NextPageCommand = new RelayCommand(_ => _paginationHelper.NextPage());
            LastPageCommand = new RelayCommand(_ => _paginationHelper.LastPage());
            GoToPageCommand = new RelayCommand(_ => GoToPage());

            LoadCustomers();
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

        private void LoadCustomers()
        {
            var customers = CustomerService.GetAllCustomers();
            _allCustomers = customers.ConvertAll(c => new CustomerItemViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                CustomerType = c.CustomerType,
                Tier = CustomerService.GetCustomerLoyalty(c.Id).Tier,
                Points = CustomerService.GetCustomerLoyalty(c.Id).Points
            });
            _paginationHelper.SetData(_allCustomers);
            ApplyFilters();
        }

        private void LoadPurchaseHistory(int customerId)
        {
            PurchaseHistory.Clear();
            try
            {
                var history = CustomerService.GetCustomerPurchaseHistory(customerId)
                    .Select(h => new PurchaseHistoryItemViewModel
                    {
                        InvoiceId = h.InvoiceId,
                        CreatedAt = h.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                        ItemCount = h.ItemCount,
                        Total = h.Total.ToString("N0")
                    });
                foreach (var h in history)
                {
                    PurchaseHistory.Add(h);
                }
            }
            catch {}
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

        private void AddCustomer()
        {
            if (!ValidateInput()) return;

            if (CustomerService.AddCustomer(EditingCustomer.Name.Trim(), EditingCustomer.Phone.Trim(), EditingCustomer.Email.Trim(), "Regular", EditingCustomer.Address.Trim()))
            {
                try
                {
                    int id = CustomerService.GetAllCustomers().Last().Id;
                    if (IsAdminOrManager)
                    {
                        CustomerService.UpdateCustomerLoyalty(id, EditingCustomer.Points, EditingCustomer.Tier);
                    }
                    else
                    {
                        CustomerService.UpdateCustomerLoyalty(id, 0, "Regular");
                    }
                } catch { }

                LoadCustomers();
                ClearForm();
                MessageBox.Show($"Khách hàng '{EditingCustomer.Name}' đã được thêm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show("Không thể thêm khách hàng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCustomer()
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng để cập nhật.", "Yêu cầu chọn", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!ValidateInput()) return;

            if (CustomerService.UpdateCustomer(SelectedCustomer.Id, EditingCustomer.Name.Trim(), EditingCustomer.Phone.Trim(), EditingCustomer.Email.Trim(), SelectedCustomer.CustomerType ?? "Regular", EditingCustomer.Address.Trim()))
            {
                try
                {
                    if (IsAdminOrManager)
                    {
                        CustomerService.UpdateCustomerLoyalty(SelectedCustomer.Id, EditingCustomer.Points, EditingCustomer.Tier);
                    }
                } catch { }

                LoadCustomers();
                ClearForm();
                MessageBox.Show($"Khách hàng '{EditingCustomer.Name}' đã được cập nhật thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show("Không thể cập nhật khách hàng. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCustomer()
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng để xóa.", "Yêu cầu chọn", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string customerName = SelectedCustomer.Name;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{customerName}'?\n\nHành động này không thể hoàn tác.", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (CustomerService.DeleteCustomer(SelectedCustomer.Id))
                {
                    LoadCustomers();
                    ClearForm();
                    MessageBox.Show($"Khách hàng '{customerName}' đã được xóa thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    DashboardViewModel.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Không thể xóa khách hàng. Khách hàng có thể đang được sử dụng trong hóa đơn.", "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void UpdateLoyalty()
        {
            if (SelectedCustomer == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng.");
                return;
            }
            if (CustomerService.UpdateCustomerLoyalty(SelectedCustomer.Id, EditingCustomer.Points, EditingCustomer.Tier))
            {
                LoadCustomers();
                MessageBox.Show("Đã cập nhật hạng/điểm.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show("Cập nhật thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                
                bool matchesActivity = SelectedFilterActivity switch
                {
                    "HasPurchases" => CustomerService.GetCustomerPurchaseHistory(c.Id).Any(),
                    "NoPurchases" => !CustomerService.GetCustomerPurchaseHistory(c.Id).Any(),
                    _ => true
                };
                
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
