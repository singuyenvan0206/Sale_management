using FashionStore.App.Core;
using FashionStore.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FashionStore.App.ViewModels
{
    public class CategoryManagementViewModel : BaseViewModel
    {
        public ObservableCollection<CategoryViewModel> Categories { get; } = new();

        private CategoryViewModel? _selectedCategory;
        public CategoryViewModel? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    if (value != null)
                    {
                        EditingCategoryName = value.Name;
                        EditingCategoryTax = value.TaxPercent;
                    }
                    else
                    {
                        EditingCategoryName = "";
                        EditingCategoryTax = 0;
                    }
                }
            }
        }

        private string _editingCategoryName = "";
        public string EditingCategoryName
        {
            get => _editingCategoryName;
            set => SetProperty(ref _editingCategoryName, value);
        }

        private decimal _editingCategoryTax;
        public decimal EditingCategoryTax
        {
            get => _editingCategoryTax;
            set => SetProperty(ref _editingCategoryTax, value);
        }

        private string _statusText = "Sẵn sàng";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand DeleteAllCommand { get; }

        public CategoryManagementViewModel()
        {
            AddCommand = new RelayCommand(_ => AddCategory());
            UpdateCommand = new RelayCommand(_ => UpdateCategory());
            DeleteCommand = new RelayCommand(_ => DeleteCategory());
            DeleteAllCommand = new RelayCommand(_ => DeleteAllCategories());

            LoadCategories();
        }

        private void LoadCategories()
        {
            Categories.Clear();
            var cats = CategoryService.GetAllCategories();
            foreach (var c in cats)
            {
                Categories.Add(new CategoryViewModel { Id = c.Id, Name = c.Name, TaxPercent = c.TaxPercent });
            }

            StatusText = Categories.Count == 1 ? "Tìm thấy 1 danh mục" : $"Tìm thấy {Categories.Count} danh mục";
        }

        private void AddCategory()
        {
            string name = EditingCategoryName.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên danh mục.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (name.Length > 255)
            {
                MessageBox.Show("Tên danh mục quá dài. Tối đa 255 ký tự được phép.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoryService.AddCategory(name, EditingCategoryTax))
            {
                LoadCategories();
                EditingCategoryName = "";
                EditingCategoryTax = 0;
                MessageBox.Show($"Danh mục '{name}' đã được thêm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show($"Danh mục '{name}' đã tồn tại hoặc có lỗi xảy ra.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCategory()
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục để cập nhật.", "Yêu cầu chọn", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string name = EditingCategoryName.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên danh mục.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (name.Length > 255)
            {
                MessageBox.Show("Tên danh mục quá dài. Tối đa 255 ký tự được phép.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoryService.UpdateCategory(SelectedCategory.Id, name, EditingCategoryTax))
            {
                LoadCategories();
                EditingCategoryName = "";
                EditingCategoryTax = 0;
                MessageBox.Show($"Danh mục đã được cập nhật thành công thành '{name}'!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show($"Không thể cập nhật danh mục. Tên '{name}' có thể đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCategory()
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Vui lòng chọn danh mục để xóa.", "Yêu cầu chọn", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string categoryName = SelectedCategory.Name;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa danh mục '{categoryName}'?\n\nHành động này không thể hoàn tác.", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (CategoryService.DeleteCategory(SelectedCategory.Id))
                {
                    LoadCategories();
                    EditingCategoryName = "";
                    EditingCategoryTax = 0;
                    MessageBox.Show($"Danh mục '{categoryName}' đã được xóa thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    DashboardViewModel.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show($"Không thể xóa danh mục '{categoryName}'. Nó có thể đang được sử dụng bởi các sản phẩm hiện có.", "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void DeleteAllCategories()
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa tất cả danh mục không?\n\nHành động này không thể hoàn tác.", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (CategoryService.DeleteAllCategories())
                {
                    LoadCategories();
                    EditingCategoryName = "";
                    EditingCategoryTax = 0;
                    MessageBox.Show("Tất cả danh mục đã được xóa thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    DashboardViewModel.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Không thể xóa tất cả danh mục. Có thể có các sản phẩm đang sử dụng các danh mục này.", "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}
