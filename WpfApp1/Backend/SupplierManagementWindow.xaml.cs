using System.Windows;
using System.Windows.Controls;

namespace FashionStore
{
    using FashionStore.Repositories;
    public partial class SupplierManagementWindow : Window
    {
        private PaginationHelper<Supplier> _paginationHelper = new();

        public SupplierManagementWindow()
        {
            InitializeComponent();
            _paginationHelper.PageChanged += RefreshSupplierList;
            _paginationHelper.SetPageSize(12); // Increased to 12 for compact view
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            var suppliers = SupplierRepository.GetAllSuppliers();
            _paginationHelper.SetData(suppliers);
        }

        private void RefreshSupplierList()
        {
            SuppliersListBox.ItemsSource = _paginationHelper.GetCurrentPageItems();
            
            // Update UI controls
            if (SupplierPageInfoTextBlock != null)
                SupplierPageInfoTextBlock.Text = _paginationHelper.GetPageInfo();
                
            if (TotalSuppliersTextBlock != null)
                TotalSuppliersTextBlock.Text = $"{_paginationHelper.TotalItems} nhà cung cấp";
                
            if (SupplierCurrentPageTextBox != null)
                SupplierCurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
                
            // Update button states
            if (SupplierFirstPageButton != null) SupplierFirstPageButton.IsEnabled = _paginationHelper.CanGoFirst;
            if (SupplierPrevPageButton != null) SupplierPrevPageButton.IsEnabled = _paginationHelper.CanGoPrevious;
            if (SupplierNextPageButton != null) SupplierNextPageButton.IsEnabled = _paginationHelper.CanGoNext;
            if (SupplierLastPageButton != null) SupplierLastPageButton.IsEnabled = _paginationHelper.CanGoLast;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var supplier = new Supplier
            {
                Name = NameTextBox.Text.Trim(),
                ContactName = ContactTextBox.Text.Trim(),
                Phone = PhoneTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim(),
                Address = AddressTextBox.Text.Trim(),

            };

            if (SupplierRepository.AddSupplier(supplier))
            {
                MessageBox.Show("Thêm nhà cung cấp thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSuppliers();
                ClearForm();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi thêm nhà cung cấp.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersListBox.SelectedItem is not Supplier selectedSupplier)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInput()) return;

            selectedSupplier.Name = NameTextBox.Text.Trim();
            selectedSupplier.ContactName = ContactTextBox.Text.Trim();
            selectedSupplier.Phone = PhoneTextBox.Text.Trim();
            selectedSupplier.Email = EmailTextBox.Text.Trim();
            selectedSupplier.Address = AddressTextBox.Text.Trim();


            if (SupplierRepository.UpdateSupplier(selectedSupplier))
            {
                MessageBox.Show("Cập nhật nhà cung cấp thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSuppliers();
                ClearForm();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersListBox.SelectedItem is not Supplier selectedSupplier)
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp cần xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa '{selectedSupplier.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (SupplierRepository.DeleteSupplier(selectedSupplier.Id))
                {
                    MessageBox.Show("Xóa nhà cung cấp thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSuppliers();
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("Không thể xóa nhà cung cấp này.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            NameTextBox.Clear();
            ContactTextBox.Clear();
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
            AddressTextBox.Clear();
            NoteTextBox.Clear();
            SuppliersListBox.SelectedItem = null;
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Tên nhà cung cấp không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return false;
            }
            return true;
        }

        private void SuppliersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuppliersListBox.SelectedItem is Supplier selectedSupplier)
            {
                NameTextBox.Text = selectedSupplier.Name;
                ContactTextBox.Text = selectedSupplier.ContactName;
                PhoneTextBox.Text = selectedSupplier.Phone;
                EmailTextBox.Text = selectedSupplier.Email;
                AddressTextBox.Text = selectedSupplier.Address;

            }
        }
        // Pagination Event Handlers
        private void SupplierFirstPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.FirstPage();
        }

        private void SupplierPrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.PreviousPage();
        }

        private void SupplierNextPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.NextPage();
        }

        private void SupplierLastPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.LastPage();
        }

        private void SupplierCurrentPageTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (int.TryParse(SupplierCurrentPageTextBox.Text, out int pageNumber))
                {
                    if (!_paginationHelper.GoToPage(pageNumber))
                    {
                        SupplierCurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
                        MessageBox.Show($"Trang không hợp lệ. Vui lòng nhập từ 1 đến {_paginationHelper.TotalPages}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    SupplierCurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
                }
            }
        }
    }
}
