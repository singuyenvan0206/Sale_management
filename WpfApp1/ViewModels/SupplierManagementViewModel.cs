using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FashionStore.Models;
using FashionStore.Services;
using FashionStore.Core;

namespace FashionStore.ViewModels
{
    public class SupplierManagementViewModel : BaseViewModel
    {
        private PaginationHelper<Supplier> _paginationHelper = new();

        public ObservableCollection<Supplier> PagedSuppliers { get; } = new();

        private Supplier _editingSupplier = new();
        public Supplier EditingSupplier
        {
            get => _editingSupplier;
            set => SetProperty(ref _editingSupplier, value);
        }

        private Supplier? _selectedSupplier;
        public Supplier? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (SetProperty(ref _selectedSupplier, value))
                {
                    if (value != null)
                    {
                        EditingSupplier = new Supplier
                        {
                            Id = value.Id,
                            Name = value.Name,
                            ContactName = value.ContactName,
                            Phone = value.Phone,
                            Email = value.Email,
                            Address = value.Address
                        };
                    }
                    else
                    {
                        ClearForm();
                    }
                }
            }
        }

        private string _pageInfoText = "Trang: 1 / 1";
        public string PageInfoText { get => _pageInfoText; set => SetProperty(ref _pageInfoText, value); }
        
        private string _totalSuppliersText = "0 nhà cung cấp";
        public string TotalSuppliersText { get => _totalSuppliersText; set => SetProperty(ref _totalSuppliersText, value); }

        private int _currentPageBox = 1;
        public int CurrentPageBox { get => _currentPageBox; set => SetProperty(ref _currentPageBox, value); }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public ICommand FirstPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public SupplierManagementViewModel()
        {
            _paginationHelper.PageChanged += OnPageChanged;
            _paginationHelper.SetPageSize(12);

            AddCommand = new RelayCommand(_ => AddSupplier());
            UpdateCommand = new RelayCommand(_ => UpdateSupplier());
            DeleteCommand = new RelayCommand(_ => DeleteSupplier());
            ClearCommand = new RelayCommand(_ => ClearForm());

            FirstPageCommand = new RelayCommand(_ => _paginationHelper.FirstPage());
            PrevPageCommand = new RelayCommand(_ => _paginationHelper.PreviousPage());
            NextPageCommand = new RelayCommand(_ => _paginationHelper.NextPage());
            LastPageCommand = new RelayCommand(_ => _paginationHelper.LastPage());
            GoToPageCommand = new RelayCommand(_ => GoToPage());

            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            var suppliers = SupplierService.GetAllSuppliers();
            _paginationHelper.SetData(suppliers);
            UpdateDisplay();
        }

        private void OnPageChanged()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            PagedSuppliers.Clear();
            foreach (var item in _paginationHelper.GetCurrentPageItems())
            {
                PagedSuppliers.Add(item);
            }

            PageInfoText = _paginationHelper.GetPageInfo();
            TotalSuppliersText = $"{_paginationHelper.TotalItems} nhà cung cấp";
            CurrentPageBox = _paginationHelper.CurrentPage;
        }

        private void ClearForm()
        {
            EditingSupplier = new Supplier();
            SelectedSupplier = null;
        }
        
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(EditingSupplier.Name))
            {
                MessageBox.Show("Tên nhà cung cấp không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void AddSupplier()
        {
            if (!ValidateInput()) return;

            var supplier = new Supplier
            {
                Name = EditingSupplier.Name.Trim(),
                ContactName = EditingSupplier.ContactName?.Trim() ?? "",
                Phone = EditingSupplier.Phone?.Trim() ?? "",
                Email = EditingSupplier.Email?.Trim() ?? "",
                Address = EditingSupplier.Address?.Trim() ?? ""
            };

            if (SupplierService.AddSupplier(supplier))
            {
                MessageBox.Show("Thêm nhà cung cấp thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSuppliers();
                ClearForm();
                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi thêm nhà cung cấp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSupplier()
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInput()) return;

            var supplier = new Supplier
            {
                Id = EditingSupplier.Id,
                Name = EditingSupplier.Name.Trim(),
                ContactName = EditingSupplier.ContactName?.Trim() ?? "",
                Phone = EditingSupplier.Phone?.Trim() ?? "",
                Email = EditingSupplier.Email?.Trim() ?? "",
                Address = EditingSupplier.Address?.Trim() ?? ""
            };

            if (SupplierService.UpdateSupplier(supplier))
            {
                MessageBox.Show("Cập nhật nhà cung cấp thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSuppliers();
                ClearForm();
                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSupplier()
        {
            if (SelectedSupplier == null)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa '{SelectedSupplier.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (SupplierService.DeleteSupplier(SelectedSupplier.Id))
                {
                    MessageBox.Show("Xóa nhà cung cấp thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSuppliers();
                    ClearForm();
                    DashboardViewModel.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Không thể xóa nhà cung cấp này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void GoToPage()
        {
            if (!_paginationHelper.GoToPage(CurrentPageBox))
            {
                CurrentPageBox = _paginationHelper.CurrentPage;
            }
        }
    }
}
