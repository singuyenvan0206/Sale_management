using System.Windows;
using System.Windows.Controls;

namespace FashionStore
{
    using FashionStore.Repositories;
    public partial class ProductManagementWindow : Window
    {
        private List<ProductViewModel> _products = new();
        private List<CategoryViewModel> _categories = new();
        private List<Supplier> _suppliers = new();
        private ProductViewModel? _selectedProduct;
        private PaginationHelper<ProductViewModel> _paginationHelper = new();
        
        // Filter state
        private int? _selectedCategoryId = null;
        private int? _selectedSupplierId = null;
        private string _selectedStockFilter = "All";
        private string _selectedPromoFilter = "All";
        private string _selectedPriceFilter = "All";

        public ProductManagementWindow()
        {
            InitializeComponent();
            _selectedProduct = new ProductViewModel();
            _paginationHelper.PageChanged += OnPageChanged;
            LoadData();
            ProductDataGrid.Sorting += ProductDataGrid_Sorting;
        }

        private void LoadData()
        {
            LoadCategories();
            LoadSuppliers();
            LoadProducts();
            PopulateFilterComboBoxes();
        }

        private void LoadSuppliers()
        {
            _suppliers = SupplierRepository.GetAllSuppliers();
            SupplierComboBox.ItemsSource = _suppliers;
        }

        private void LoadCategories()
        {
            var categories = CategoryRepository.GetAllCategories();
            _categories = categories.ConvertAll(c => new CategoryViewModel { Id = c.Id, Name = c.Name, TaxPercent = c.TaxPercent });
            CategoryComboBox.ItemsSource = _categories;
        }

        private void LoadProducts()
        {
            var products = ProductRepository.GetAllProductsWithCategories();
            _products = products.ConvertAll(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                CategoryId = p.CategoryId,
                CategoryName = p.CategoryName,
                SalePrice = p.SalePrice,
                PromoDiscountPercent = p.PromoDiscountPercent,
                PromoStartDate = p.PromoStartDate,
                PromoEndDate = p.PromoEndDate,
                PurchasePrice = p.PurchasePrice,
                PurchaseUnit = p.PurchaseUnit,
                ImportQuantity = p.ImportQuantity,
                StockQuantity = p.StockQuantity,

                Description = p.Description,
                SupplierId = p.SupplierId,
                SupplierName = p.SupplierName
            });
            _paginationHelper.SetData(_products);
            UpdateDisplayAndPagination();
        }

        private void UpdateStatusText()
        {
            int count = _paginationHelper.TotalItems;
            StatusTextBlock.Text = count == 1 ? "Tìm thấy 1 sản phẩm" : $"Tìm thấy {count} sản phẩm";
        }

        private void OnPageChanged()
        {
            UpdateDisplayAndPagination();
        }

        private void UpdateDisplayAndPagination()
        {
            ProductDataGrid.ItemsSource = null;
            ProductDataGrid.ItemsSource = _paginationHelper.GetCurrentPageItems();
            if (PageInfoTextBlock != null)
            {
                PageInfoTextBlock.Text = $"📄 Trang: {_paginationHelper.GetPageInfo()} • 📊 Tổng: {_paginationHelper.TotalItems} sản phẩm";
            }
            
            // Update current page textbox
            if (CurrentPageTextBox != null)
            {
                CurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
            }
            
            // Update button states
            if (FirstPageButton != null) FirstPageButton.IsEnabled = _paginationHelper.CanGoFirst;
            if (PrevPageButton != null) PrevPageButton.IsEnabled = _paginationHelper.CanGoPrevious;
            if (NextPageButton != null) NextPageButton.IsEnabled = _paginationHelper.CanGoNext;
            if (LastPageButton != null) LastPageButton.IsEnabled = _paginationHelper.CanGoLast;
            
            UpdateStatusText();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var product = new ProductViewModel
            {
                Name = ProductNameTextBox.Text.Trim(),
                Code = ProductCodeTextBox.Text.Trim(),
                CategoryId = CategoryComboBox.SelectedValue as int? ?? 0,
                SalePrice = decimal.Parse(PriceTextBox.Text),
                PromoDiscountPercent = decimal.TryParse(PromoDiscountPercentTextBox?.Text, out var dp) ? dp : 0m,
                PromoStartDate = PromoStartDatePicker?.SelectedDate,
                PromoEndDate = PromoEndDatePicker?.SelectedDate,
                PurchasePrice = decimal.TryParse(ImportPriceTextBox.Text, out var pp) ? pp : 0,
                PurchaseUnit = "VND",
                ImportQuantity = int.TryParse(ImportQuantityTextBox.Text, out var iq) ? iq : 0,
                StockQuantity = int.Parse(StockQuantityTextBox.Text),
                Description = DescriptionTextBox.Text.Trim(),
                SupplierId = SupplierComboBox.SelectedValue as int? ?? 0
            };

            if (ProductRepository.AddProduct(product.Name, product.Code, product.CategoryId, product.SalePrice, product.PurchasePrice, product.PurchaseUnit, product.ImportQuantity, product.StockQuantity, product.Description, product.PromoDiscountPercent, product.PromoStartDate, product.PromoEndDate, product.SupplierId))
            {
                LoadProducts();
                ClearForm();
                MessageBox.Show($"Sản phẩm '{product.Name}' đã được thêm thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Trigger dashboard refresh for real-time updates
                DashboardWindow.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show("Không thể thêm sản phẩm. Mã sản phẩm có thể đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProduct == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để cập nhật.", "Yêu cầu chọn", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!ValidateInput()) return;

            var product = new ProductViewModel
            {
                Id = _selectedProduct.Id,
                Name = ProductNameTextBox.Text.Trim(),
                Code = ProductCodeTextBox.Text.Trim(),
                CategoryId = CategoryComboBox.SelectedValue as int? ?? 0,
                SalePrice = decimal.Parse(PriceTextBox.Text),
                PromoDiscountPercent = decimal.TryParse(PromoDiscountPercentTextBox?.Text, out var dp) ? dp : 0m,
                PromoStartDate = PromoStartDatePicker?.SelectedDate,
                PromoEndDate = PromoEndDatePicker?.SelectedDate,
                PurchasePrice = decimal.TryParse(ImportPriceTextBox.Text, out var pp) ? pp : 0,
                PurchaseUnit = "VND",
                ImportQuantity = int.TryParse(ImportQuantityTextBox.Text, out var iq) ? iq : 0,
                StockQuantity = int.Parse(StockQuantityTextBox.Text),
                Description = DescriptionTextBox.Text.Trim(),
                SupplierId = SupplierComboBox.SelectedValue as int? ?? 0
            };

            if (ProductRepository.UpdateProduct(product.Id, product.Name, product.Code, product.CategoryId, product.SalePrice, product.PurchasePrice, product.PurchaseUnit, product.ImportQuantity, product.StockQuantity, product.Description, product.PromoDiscountPercent, product.PromoStartDate, product.PromoEndDate, product.SupplierId))
            {
                LoadProducts();
                ClearForm();
                MessageBox.Show($"Sản phẩm '{product.Name}' đã được cập nhật thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Trigger dashboard refresh for real-time updates
                DashboardWindow.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show("Không thể cập nhật sản phẩm. Mã sản phẩm có thể đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProduct == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để xóa.", "Yêu cầu chọn", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string productName = _selectedProduct.Name;
            int productId = _selectedProduct.Id;

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa sản phẩm '{productName}'?\n\nHành động này không thể hoàn tác.",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (ProductRepository.DeleteProduct(productId))
                {
                    LoadProducts();
                    ClearForm();
                    MessageBox.Show($"Sản phẩm '{productName}' đã được xóa thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Trigger dashboard refresh for real-time updates
                    DashboardWindow.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Không thể xóa sản phẩm. Sản phẩm có thể đang được sử dụng trong hóa đơn.", "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ImportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Chọn tệp CSV để nhập sản phẩm"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                int importedCount = ProductRepository.ImportProductsFromCsv(filePath);
                if (importedCount >= 0)
                {
                    LoadProducts();
                    MessageBox.Show($"Đã nhập thành công {importedCount} sản phẩm từ tệp CSV.", "Nhập thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Trigger dashboard refresh for real-time updates
                    DashboardWindow.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Không thể nhập sản phẩm từ tệp CSV. Vui lòng kiểm tra định dạng tệp.", "Lỗi nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "Lưu sản phẩm vào tệp CSV",
                FileName = "products.csv"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                bool success = ProductRepository.ExportProductsToCsv(filePath);
                if (success)
                {
                    MessageBox.Show("Đã xuất sản phẩm thành công sang tệp CSV.", "Xuất thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Không thể xuất sản phẩm sang tệp CSV.", "Lỗi xuất", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                 }
        }

        private void DeleteAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_products.Count == 0)
            {
                MessageBox.Show("Không có sản phẩm nào để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa TẤT CẢ {_products.Count} sản phẩm?\n\nHành động này không thể hoàn tác và sẽ xóa toàn bộ dữ liệu sản phẩm.",
                "Xác nhận xóa tất cả",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                if (ProductRepository.DeleteAllProducts())
                {
                    LoadProducts();
                    ClearForm();
                    MessageBox.Show($"Đã xóa thành công tất cả {_products.Count} sản phẩm!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Trigger dashboard refresh for real-time updates
                    DashboardWindow.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Không thể xóa tất cả sản phẩm. Một số sản phẩm có thể đang được sử dụng trong hóa đơn.", "Xóa thất bại", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ClearForm()
        {
            ProductNameTextBox.Clear();
            ProductCodeTextBox.Clear();
            CategoryComboBox.SelectedIndex = -1;
            PriceTextBox.Clear();
            if (PromoDiscountPercentTextBox != null) PromoDiscountPercentTextBox.Text = "0";
            if (PromoStartDatePicker != null) PromoStartDatePicker.SelectedDate = null;
            if (PromoEndDatePicker != null) PromoEndDatePicker.SelectedDate = null;
            StockQuantityTextBox.Clear();
            ImportPriceTextBox.Clear();
            ImportQuantityTextBox.Clear();
            // Unit removed - using VND as default
            DescriptionTextBox.Clear();
            SupplierComboBox.SelectedIndex = -1;
            _selectedProduct = null;
            ProductDataGrid.SelectedItem = null;
            ProductNameTextBox.Focus();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sản phẩm.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                ProductNameTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(PriceTextBox.Text) || !decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Vui lòng nhập giá hợp lệ.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(StockQuantityTextBox.Text) || !int.TryParse(StockQuantityTextBox.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng tồn hợp lệ.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                StockQuantityTextBox.Focus();
                return false;
            }

            // Promotion percent validation (0..100)
            if (PromoDiscountPercentTextBox != null &&
                !string.IsNullOrWhiteSpace(PromoDiscountPercentTextBox.Text) &&
                (!decimal.TryParse(PromoDiscountPercentTextBox.Text, out var promo) || promo < 0 || promo > 100))
            {
                MessageBox.Show("Giảm giá (%) phải nằm trong khoảng 0 đến 100.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                PromoDiscountPercentTextBox.Focus();
                return false;
            }

            return true;
        }

        private void ProductDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProduct = ProductDataGrid.SelectedItem as ProductViewModel;
            if (_selectedProduct != null)
            {
                ProductNameTextBox.Text = _selectedProduct.Name ?? "";
                ProductCodeTextBox.Text = _selectedProduct.Code ?? "";
                CategoryComboBox.SelectedValue = _selectedProduct.CategoryId;
                PriceTextBox.Text = _selectedProduct.SalePrice.ToString("F2");
                if (PromoDiscountPercentTextBox != null) PromoDiscountPercentTextBox.Text = _selectedProduct.PromoDiscountPercent.ToString("F2");
                if (PromoStartDatePicker != null) PromoStartDatePicker.SelectedDate = _selectedProduct.PromoStartDate;
                if (PromoEndDatePicker != null) PromoEndDatePicker.SelectedDate = _selectedProduct.PromoEndDate;
                StockQuantityTextBox.Text = _selectedProduct.StockQuantity.ToString();
                ImportPriceTextBox.Text = _selectedProduct.PurchasePrice.ToString("F2");
                ImportQuantityTextBox.Text = _selectedProduct.ImportQuantity.ToString();
                // Unit removed - using VND as default
                DescriptionTextBox.Text = _selectedProduct.Description ?? "";
                SupplierComboBox.SelectedValue = _selectedProduct.SupplierId;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            FilterProducts();
        }

        private void FilterProducts()
        {
            string searchTerm = SearchTextBox?.Text?.ToLower() ?? string.Empty;
            
            _paginationHelper.SetFilter(p =>
            {
                // Search filter
                bool matchesSearch = string.IsNullOrWhiteSpace(searchTerm) ||
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Code.ToLower().Contains(searchTerm) ||
                    p.CategoryName.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm);
                
                // Category filter
                bool matchesCategory = !_selectedCategoryId.HasValue || p.CategoryId == _selectedCategoryId.Value;
                
                // Supplier filter
                bool matchesSupplier = !_selectedSupplierId.HasValue || p.SupplierId == _selectedSupplierId.Value;
                
                // Stock filter
                bool matchesStock = _selectedStockFilter switch
                {
                    "OutOfStock" => p.StockQuantity == 0,
                    "LowStock" => p.StockQuantity > 0 && p.StockQuantity < 10,
                    "InStock" => p.StockQuantity >= 10,
                    _ => true // "All"
                };
                
                // Promotion filter
                bool matchesPromo = _selectedPromoFilter switch
                {
                    "HasPromo" => p.PromoDiscountPercent > 0 && 
                                  (!p.PromoStartDate.HasValue || p.PromoStartDate.Value <= DateTime.Now) &&
                                  (!p.PromoEndDate.HasValue || p.PromoEndDate.Value >= DateTime.Now),
                    "NoPromo" => p.PromoDiscountPercent == 0 || 
                                 (p.PromoStartDate.HasValue && p.PromoStartDate.Value > DateTime.Now) ||
                                 (p.PromoEndDate.HasValue && p.PromoEndDate.Value < DateTime.Now),
                    _ => true // "All"
                };
                
                // Price filter
                bool matchesPrice = _selectedPriceFilter switch
                {
                    "Under100k" => p.SalePrice < 100000,
                    "100kTo500k" => p.SalePrice >= 100000 && p.SalePrice < 500000,
                    "500kTo1M" => p.SalePrice >= 500000 && p.SalePrice < 1000000,
                    "Over1M" => p.SalePrice >= 1000000,
                    _ => true // "All"
                };
                
                // Combine all filters with AND logic
                return matchesSearch && matchesCategory && matchesSupplier && matchesStock && matchesPromo && matchesPrice;
            });
        }

        // Pagination event handlers
        private void FirstPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.FirstPage();
        }

        private void PrevPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.PreviousPage();
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.NextPage();
        }

        private void LastPageButton_Click(object sender, RoutedEventArgs e)
        {
            _paginationHelper.LastPage();
        }

        private void CurrentPageTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (int.TryParse(CurrentPageTextBox.Text, out int pageNumber))
                {
                    if (!_paginationHelper.GoToPage(pageNumber))
                    {
                        // Reset to current page if invalid
                        CurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
                        MessageBox.Show($"Trang không hợp lệ. Vui lòng nhập từ 1 đến {_paginationHelper.TotalPages}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    CurrentPageTextBox.Text = _paginationHelper.CurrentPage.ToString();
                }
            }
        }
        
        private void ProductDataGrid_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            e.Handled = true; // Prevent default sorting
            
            var column = e.Column;
            var propertyName = column.SortMemberPath;
            
            if (string.IsNullOrEmpty(propertyName)) return;
            
            // Determine sort direction
            var direction = column.SortDirection != System.ComponentModel.ListSortDirection.Ascending 
                ? System.ComponentModel.ListSortDirection.Ascending 
                : System.ComponentModel.ListSortDirection.Descending;
            
            // Apply sort to all data through PaginationHelper
            Func<IEnumerable<ProductViewModel>, IOrderedEnumerable<ProductViewModel>>? sortFunc = null;
            
            switch (propertyName.ToLower())
            {
                case "id":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(p => p.Id)
                        : items => items.OrderByDescending(p => p.Id);
                    break;
                case "name":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(p => p.Name)
                        : items => items.OrderByDescending(p => p.Name);
                    break;
                case "code":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(p => p.Code)
                        : items => items.OrderByDescending(p => p.Code);
                    break;
                case "categoryname":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(p => p.CategoryName)
                        : items => items.OrderByDescending(p => p.CategoryName);
                    break;
                case "price":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(p => p.SalePrice)
                        : items => items.OrderByDescending(p => p.SalePrice);
                    break;
                case "stockquantity":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(p => p.StockQuantity)
                        : items => items.OrderByDescending(p => p.StockQuantity);
                    break;
                case "description":
                    sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending
                        ? items => items.OrderBy(p => p.Description)
                        : items => items.OrderByDescending(p => p.Description);
                    break;
            }
            
            if (sortFunc != null)
            {
                _paginationHelper.SetSort(sortFunc);
                
                // Update column sort direction
                column.SortDirection = direction;
                
                // Clear other columns' sort direction
                foreach (var col in ProductDataGrid.Columns)
                {
                    if (col != column)
                        col.SortDirection = null;
                }
            }
        }
        
        private void PopulateFilterComboBoxes()
        {
            // Populate Category Filter
            var allCategoriesItem = new CategoryViewModel { Id = 0, Name = "Tất cả danh mục" };
            var categoryList = new List<CategoryViewModel> { allCategoriesItem };
            categoryList.AddRange(_categories);
            FilterCategoryComboBox.ItemsSource = categoryList;
            FilterCategoryComboBox.SelectedIndex = 0;
            
            // Populate Supplier Filter
            var allSuppliersItem = new Supplier { Id = 0, Name = "Tất cả nhà cung cấp" };
            var supplierList = new List<Supplier> { allSuppliersItem };
            supplierList.AddRange(_suppliers);
            FilterSupplierComboBox.ItemsSource = supplierList;
            FilterSupplierComboBox.SelectedIndex = 0;
        }
        
        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update filter state based on which ComboBox changed
            if (sender == FilterCategoryComboBox && FilterCategoryComboBox.SelectedValue != null)
            {
                int categoryId = (int)FilterCategoryComboBox.SelectedValue;
                _selectedCategoryId = categoryId == 0 ? null : categoryId;
            }
            else if (sender == FilterSupplierComboBox && FilterSupplierComboBox.SelectedValue != null)
            {
                int supplierId = (int)FilterSupplierComboBox.SelectedValue;
                _selectedSupplierId = supplierId == 0 ? null : supplierId;
            }
            else if (sender == FilterStockComboBox && FilterStockComboBox.SelectedItem is ComboBoxItem stockItem)
            {
                _selectedStockFilter = stockItem.Tag?.ToString() ?? "All";
            }
            else if (sender == FilterPromoComboBox && FilterPromoComboBox.SelectedItem is ComboBoxItem promoItem)
            {
                _selectedPromoFilter = promoItem.Tag?.ToString() ?? "All";
            }
            else if (sender == FilterPriceComboBox && FilterPriceComboBox.SelectedItem is ComboBoxItem priceItem)
            {
                _selectedPriceFilter = priceItem.Tag?.ToString() ?? "All";
            }
            
            // Apply filters
            FilterProducts();
        }
        
        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset all filter ComboBoxes to default
            FilterCategoryComboBox.SelectedIndex = 0;
            FilterSupplierComboBox.SelectedIndex = 0;
            FilterStockComboBox.SelectedIndex = 0;
            FilterPromoComboBox.SelectedIndex = 0;
            FilterPriceComboBox.SelectedIndex = 0;
            
            // Reset filter state
            _selectedCategoryId = null;
            _selectedSupplierId = null;
            _selectedStockFilter = "All";
            _selectedPromoFilter = "All";
            _selectedPriceFilter = "All";
            
            // Clear search box
            SearchTextBox.Clear();
            
            // Reapply filters (which will show all products)
            FilterProducts();
        }

        private void Grid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (CategoryComboBox?.IsDropDownOpen == true ||
                SupplierComboBox?.IsDropDownOpen == true ||
                FilterCategoryComboBox?.IsDropDownOpen == true ||
                FilterSupplierComboBox?.IsDropDownOpen == true ||
                FilterStockComboBox?.IsDropDownOpen == true ||
                FilterPromoComboBox?.IsDropDownOpen == true ||
                FilterPriceComboBox?.IsDropDownOpen == true)
            {
                e.Handled = true;
            }
        }
    }

    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal SalePrice { get; set; }
        public decimal PromoDiscountPercent { get; set; }
        public DateTime? PromoStartDate { get; set; }
        public DateTime? PromoEndDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public string PurchaseUnit { get; set; } = "";
        public int ImportQuantity { get; set; }
        public int StockQuantity { get; set; }
        public string Description { get; set; } = "";
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = "";
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal TaxPercent { get; set; }

    }
}
