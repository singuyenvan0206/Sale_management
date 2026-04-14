using FashionStore.App.Core;
using FashionStore.Core.Models;
using FashionStore.Core.Settings;
using FashionStore.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FashionStore.App.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        private int _id;
        public int Id { get => _id; set => SetProperty(ref _id, value); }

        private string _name = "";
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _code = "";
        public string Code { get => _code; set => SetProperty(ref _code, value); }

        private int _categoryId;
        public int CategoryId { get => _categoryId; set => SetProperty(ref _categoryId, value); }

        private string _categoryName = "";
        public string CategoryName { get => _categoryName; set => SetProperty(ref _categoryName, value); }

        private decimal _salePrice;
        public decimal SalePrice { get => _salePrice; set => SetProperty(ref _salePrice, value); }

        private decimal _promoDiscountPercent;
        public decimal PromoDiscountPercent { get => _promoDiscountPercent; set => SetProperty(ref _promoDiscountPercent, value); }

        private DateTime? _promoStartDate;
        public DateTime? PromoStartDate { get => _promoStartDate; set => SetProperty(ref _promoStartDate, value); }

        private DateTime? _promoEndDate;
        public DateTime? PromoEndDate { get => _promoEndDate; set => SetProperty(ref _promoEndDate, value); }

        private decimal _purchasePrice;
        public decimal PurchasePrice { get => _purchasePrice; set => SetProperty(ref _purchasePrice, value); }

        private string _purchaseUnit = "VND";
        public string PurchaseUnit { get => _purchaseUnit; set => SetProperty(ref _purchaseUnit, value); }

        private int _importQuantity;
        public int ImportQuantity { get => _importQuantity; set => SetProperty(ref _importQuantity, value); }

        private int _stockQuantity;
        public int StockQuantity { get => _stockQuantity; set => SetProperty(ref _stockQuantity, value); }

        private string _description = "";
        public string Description { get => _description; set => SetProperty(ref _description, value); }

        private int _supplierId;
        public int SupplierId { get => _supplierId; set => SetProperty(ref _supplierId, value); }

        private string _supplierName = "";
        public string SupplierName { get => _supplierName; set => SetProperty(ref _supplierName, value); }

        private decimal _finalPrice;
        public decimal FinalPrice { get => _finalPrice; set => SetProperty(ref _finalPrice, value); }

        public ProductViewModel Clone()
        {
            return (ProductViewModel)this.MemberwiseClone();
        }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal TaxPercent { get; set; }
    }

    public class ProductManagementViewModel : BaseViewModel
    {
        private PaginationHelper<ProductViewModel> _paginationHelper = new();
        private List<ProductViewModel> _allProducts = new();

        public ObservableCollection<ProductViewModel> PagedProducts { get; } = new();

        public ObservableCollection<CategoryViewModel> Categories { get; } = new();
        public ObservableCollection<CategoryViewModel> FilterCategories { get; } = new();

        public ObservableCollection<Supplier> Suppliers { get; } = new();
        public ObservableCollection<Supplier> FilterSuppliers { get; } = new();

        private ProductViewModel _editingProduct = new();
        public ProductViewModel EditingProduct
        {
            get => _editingProduct;
            set => SetProperty(ref _editingProduct, value);
        }

        private ProductViewModel? _selectedProduct;
        public ProductViewModel? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    if (value != null)
                    {
                        EditingProduct = value.Clone();
                    }
                    else
                    {
                        ClearForm();
                    }
                }
            }
        }

        private string _statusText = "Sẵn sàng";
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        private string _pageInfoText = "Trang: 1 / 1 • Tổng: 0 sản phẩm";
        public string PageInfoText { get => _pageInfoText; set => SetProperty(ref _pageInfoText, value); }

        private int _currentPageBox = 1;
        public int CurrentPageBox { get => _currentPageBox; set => SetProperty(ref _currentPageBox, value); }

        // Filter Props
        private string _searchTerm = "";
        public string SearchTerm
        {
            get => _searchTerm;
            set { if (SetProperty(ref _searchTerm, value)) ApplyFilters(); }
        }

        private int _selectedFilterCategoryId = 0;
        public int SelectedFilterCategoryId
        {
            get => _selectedFilterCategoryId;
            set { if (SetProperty(ref _selectedFilterCategoryId, value)) ApplyFilters(); }
        }

        private int _selectedFilterSupplierId = 0;
        public int SelectedFilterSupplierId
        {
            get => _selectedFilterSupplierId;
            set { if (SetProperty(ref _selectedFilterSupplierId, value)) ApplyFilters(); }
        }

        private string _selectedFilterStock = "All";
        public string SelectedFilterStock
        {
            get => _selectedFilterStock;
            set { if (SetProperty(ref _selectedFilterStock, value)) ApplyFilters(); }
        }

        private string _selectedFilterPromo = "All";
        public string SelectedFilterPromo
        {
            get => _selectedFilterPromo;
            set { if (SetProperty(ref _selectedFilterPromo, value)) ApplyFilters(); }
        }

        private string _selectedFilterPrice = "All";
        public string SelectedFilterPrice
        {
            get => _selectedFilterPrice;
            set { if (SetProperty(ref _selectedFilterPrice, value)) ApplyFilters(); }
        }

        private bool _isSearchMode = false;
        public bool IsSearchMode
        {
            get => _isSearchMode;
            set
            {
                if (SetProperty(ref _isSearchMode, value))
                {
                    OnPropertyChanged(nameof(LeftPanelWidth));
                }
            }
        }

        public GridLength LeftPanelWidth => IsSearchMode ? new GridLength(0) : new GridLength(350);

        public ICommand SearchCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand FirstPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }
        public ICommand GoToPageCommand { get; }
        public ICommand DeleteAllCommand { get; }
        public ICommand ImportCsvCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand LookupProductByCodeCommand { get; }

        public ProductManagementViewModel()
        {
            _paginationHelper.PageChanged += OnPageChanged;

            SearchCommand = new RelayCommand(_ => ApplyFilters());
            AddCommand = new RelayCommand(_ => AddProduct());
            UpdateCommand = new RelayCommand(_ => UpdateProduct());
            DeleteCommand = new RelayCommand(_ => DeleteProduct());
            ClearCommand = new RelayCommand(_ => ClearForm());
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());

            FirstPageCommand = new RelayCommand(_ => _paginationHelper.FirstPage());
            PrevPageCommand = new RelayCommand(_ => _paginationHelper.PreviousPage());
            NextPageCommand = new RelayCommand(_ => _paginationHelper.NextPage());
            LastPageCommand = new RelayCommand(_ => _paginationHelper.LastPage());
            GoToPageCommand = new RelayCommand(_ => GoToPage());

            DeleteAllCommand = new RelayCommand(_ => DeleteAllProducts());
            ImportCsvCommand = new RelayCommand(_ => ImportCsv());
            ExportCsvCommand = new RelayCommand(_ => ExportCsv());
            LookupProductByCodeCommand = new RelayCommand(_ => LookupProductByCode());

            LoadData();
        }

        public void LoadData()
        {
            LoadCategories();
            LoadSuppliers();
            LoadProducts();
        }

        private void LoadCategories()
        {
            Categories.Clear();
            FilterCategories.Clear();
            FilterCategories.Add(new CategoryViewModel { Id = 0, Name = "Tất cả danh mục" });

            var cats = CategoryService.GetAllCategories();
            foreach (var c in cats)
            {
                var vm = new CategoryViewModel { Id = c.Id, Name = c.Name, TaxPercent = c.TaxPercent };
                Categories.Add(vm);
                FilterCategories.Add(vm);
            }
        }

        private void LoadSuppliers()
        {
            Suppliers.Clear();
            FilterSuppliers.Clear();
            FilterSuppliers.Add(new Supplier { Id = 0, Name = "Tất cả nhà cung cấp" });

            var sups = SupplierService.GetAllSuppliers();
            foreach (var s in sups)
            {
                Suppliers.Add(s);
                FilterSuppliers.Add(s);
            }
        }

        private void LoadProducts()
        {
            var activePromos = PromotionService.GetActivePromotions();
            var products = ProductService.GetAllProductsWithCategories();
            _allProducts = products.ConvertAll(p =>
            {
                var vm = new ProductViewModel
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
                };

                // Calculate Final Price (Best Discount)
                decimal basePrice = p.SalePrice;
                decimal bestPrice = basePrice;

                // 1. Base Product Discount (legacy fields)
                if (p.PromoDiscountPercent > 0)
                {
                    var now = DateTime.Now;
                    var startDate = p.PromoStartDate ?? DateTime.MinValue;
                    var endDate = p.PromoEndDate ?? DateTime.MaxValue;

                    if (now >= startDate && now <= endDate)
                    {
                        var discounted = basePrice * (1 - (p.PromoDiscountPercent / 100m));
                        if (discounted < bestPrice) bestPrice = discounted;
                    }
                }

                // 2. Flash Sales (Product or Category based)
                var relevantPromos = activePromos.Where(promo =>
                    promo.Type == Promotion.TypeFlashSale &&
                    (promo.RequiredProductId == p.Id || promo.TargetCategoryId == p.CategoryId || promo.TargetCategoryId == null && promo.RequiredProductId == null)
                );

                foreach (var promo in relevantPromos)
                {
                    decimal promoPrice = basePrice;
                    if (promo.DiscountPercent > 0)
                        promoPrice = basePrice * (1 - (promo.DiscountPercent / 100m));
                    else if (promo.DiscountAmount > 0)
                        promoPrice = Math.Max(0, basePrice - promo.DiscountAmount);

                    if (promoPrice < bestPrice) bestPrice = promoPrice;
                }

                vm.FinalPrice = Math.Round(bestPrice, 2);
                return vm;
            });
            _paginationHelper.SetData(_allProducts);
            UpdateDisplay();
        }

        private void OnPageChanged()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            PagedProducts.Clear();
            foreach (var item in _paginationHelper.GetCurrentPageItems())
            {
                PagedProducts.Add(item);
            }

            PageInfoText = $"Trang: {_paginationHelper.GetPageInfo()} • Tổng: {_paginationHelper.TotalItems} sản phẩm";
            CurrentPageBox = _paginationHelper.CurrentPage;
            StatusText = _paginationHelper.TotalItems == 1 ? "Tìm thấy 1 sản phẩm" : $"Tìm thấy {_paginationHelper.TotalItems} sản phẩm";
        }

        private void ClearForm()
        {
            EditingProduct = new ProductViewModel();
            SelectedProduct = null;
        }

        private void LookupProductByCode()
        {
            if (string.IsNullOrWhiteSpace(EditingProduct.Code)) return;

            var product = ProductService.GetProductByCode(EditingProduct.Code);
            if (product != null)
            {
                var p = product;

                // Switch to edit mode for this product if found
                EditingProduct.Id = p.Id;
                EditingProduct.Name = p.Name;
                EditingProduct.CategoryId = p.CategoryId;
                EditingProduct.SalePrice = p.SalePrice;
                EditingProduct.PurchasePrice = p.PurchasePrice;
                EditingProduct.PurchaseUnit = p.PurchaseUnit;
                EditingProduct.StockQuantity = p.StockQuantity;
                EditingProduct.Description = p.Description;
                EditingProduct.PromoDiscountPercent = p.PromoDiscountPercent;
                EditingProduct.PromoStartDate = p.PromoStartDate;
                EditingProduct.PromoEndDate = p.PromoEndDate;
                EditingProduct.SupplierId = p.SupplierId;

                StatusText = $"🔍 Tìm thấy sản phẩm: {p.Name}. Đã tự động điền thông tin.";
            }
        }

        private void AddProduct()
        {
            if (!ValidateInput()) return;

            if (ProductService.AddProduct(EditingProduct.Name, EditingProduct.Code, EditingProduct.CategoryId,
                EditingProduct.SalePrice, EditingProduct.PurchasePrice, EditingProduct.PurchaseUnit,
                EditingProduct.ImportQuantity, EditingProduct.StockQuantity, EditingProduct.Description,
                EditingProduct.PromoDiscountPercent, EditingProduct.PromoStartDate, EditingProduct.PromoEndDate,
                EditingProduct.SupplierId))
            {
                LoadProducts();
                ClearForm();
                MessageBox.Show($"Sản phẩm '{EditingProduct.Name}' đã được thêm thành công!", "Thành công", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show("Không thể thêm sản phẩm. Mã sản phẩm có thể đã tồn tại.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void UpdateProduct()
        {
            if (SelectedProduct == null || EditingProduct.Id == 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để cập nhật.", "Yêu cầu", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            if (!ValidateInput()) return;

            if (ProductService.UpdateProduct(EditingProduct.Id, EditingProduct.Name, EditingProduct.Code,
                EditingProduct.CategoryId, EditingProduct.SalePrice, EditingProduct.PurchasePrice,
                EditingProduct.PurchaseUnit, EditingProduct.ImportQuantity, EditingProduct.StockQuantity,
                EditingProduct.Description, EditingProduct.PromoDiscountPercent, EditingProduct.PromoStartDate,
                EditingProduct.PromoEndDate, EditingProduct.SupplierId))
            {
                LoadProducts();
                ClearForm();
                MessageBox.Show($"Sản phẩm '{EditingProduct.Name}' đã được cập nhật thành công!", "Thành công", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                DashboardViewModel.TriggerDashboardRefresh();
            }
            else
            {
                MessageBox.Show("Không thể cập nhật sản phẩm. Mã sản phẩm có thể đã tồn tại.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm để xóa.", "Yêu cầu chọn", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa sản phẩm '{SelectedProduct.Name}'?\n\nHành động này không thể hoàn tác.", "Xác nhận xóa", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                if (ProductService.DeleteProduct(SelectedProduct.Id))
                {
                    LoadProducts();
                    ClearForm();
                    MessageBox.Show("Đã xóa sản phẩm thành công!", "Thành công", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    DashboardViewModel.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Không thể xóa sản phẩm. Sản phẩm có thể đang được sử dụng trong hóa đơn.", "Xóa thất bại", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
        }

        private void DeleteAllProducts()
        {
            if (_allProducts.Count == 0) return;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa TẤT CẢ {_allProducts.Count} sản phẩm?", "Cảnh báo", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                if (ProductService.DeleteAllProducts())
                {
                    LoadProducts();
                    ClearForm();
                    MessageBox.Show("Đã xóa thành công tất cả sản phẩm!", "Thành công", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    DashboardViewModel.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Lỗi xóa sản phẩm.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }
        }

        private void ImportCsv()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv" };
            if (openFileDialog.ShowDialog() == true)
            {
                int importedCount = ProductService.ImportProductsFromCsv(openFileDialog.FileName);
                if (importedCount >= 0)
                {
                    LoadProducts();
                    MessageBox.Show($"Đã nhập thành công {importedCount} sản phẩm.", "Nhập thành công", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    DashboardViewModel.TriggerDashboardRefresh();
                }
                else
                {
                    MessageBox.Show("Lỗi định dạng tập tin.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ExportCsv()
        {
            if (_allProducts.Count == 0) return;
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = "products.csv" };
            if (saveFileDialog.ShowDialog() == true)
            {
                if (ProductService.ExportProductsToCsv(saveFileDialog.FileName))
                    MessageBox.Show("Đã xuất sản phẩm thành công.", "Xuất thành công", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                else
                    MessageBox.Show("Lỗi xuất CSV.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(EditingProduct.Name))
            {
                MessageBox.Show("Vui lòng nhập tên sản phẩm.", "Lỗi xác thực", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (EditingProduct.SalePrice < 0)
            {
                MessageBox.Show("Giá bán không hợp lệ.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (EditingProduct.StockQuantity < 0)
            {
                MessageBox.Show("Số lượng tồn không hợp lệ.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (EditingProduct.PromoDiscountPercent < 0 || EditingProduct.PromoDiscountPercent > 100)
            {
                MessageBox.Show("Giảm giá phải từ 0 đến 100%.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void ApplyFilters()
        {
            string term = SearchTerm.ToLower();
            _paginationHelper.SetFilter(p =>
            {
                bool matchesSearch = string.IsNullOrWhiteSpace(term) ||
                    p.Name.ToLower().Contains(term) ||
                    p.Code.ToLower().Contains(term) ||
                    p.CategoryName.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term);

                bool matchesCat = SelectedFilterCategoryId == 0 || p.CategoryId == SelectedFilterCategoryId;
                bool matchesSup = SelectedFilterSupplierId == 0 || p.SupplierId == SelectedFilterSupplierId;

                bool matchesStock = SelectedFilterStock switch
                {
                    "OutOfStock" => p.StockQuantity == 0,
                    "LowStock" => p.StockQuantity > 0 && p.StockQuantity < 10,
                    "InStock" => p.StockQuantity >= 10,
                    _ => true
                };

                bool matchesPromo = SelectedFilterPromo switch
                {
                    "HasPromo" => p.PromoDiscountPercent > 0 &&
                                  (!p.PromoStartDate.HasValue || p.PromoStartDate.Value <= DateTime.Now) &&
                                  (!p.PromoEndDate.HasValue || p.PromoEndDate.Value >= DateTime.Now),
                    "NoPromo" => p.PromoDiscountPercent == 0 ||
                                 (p.PromoStartDate.HasValue && p.PromoStartDate.Value > DateTime.Now) ||
                                 (p.PromoEndDate.HasValue && p.PromoEndDate.Value < DateTime.Now),
                    _ => true
                };

                bool matchesPrice = SelectedFilterPrice switch
                {
                    "Under100k" => p.SalePrice < 100000,
                    "100kTo500k" => p.SalePrice >= 100000 && p.SalePrice < 500000,
                    "500kTo1M" => p.SalePrice >= 500000 && p.SalePrice < 1000000,
                    "Over1M" => p.SalePrice >= 1000000,
                    _ => true
                };

                return matchesSearch && matchesCat && matchesSup && matchesStock && matchesPromo && matchesPrice;
            });
        }

        private void ResetFilters()
        {
            SelectedFilterCategoryId = 0;
            SelectedFilterSupplierId = 0;
            SelectedFilterStock = "All";
            SelectedFilterPromo = "All";
            SelectedFilterPrice = "All";
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

        public void CallSort(System.Windows.Controls.DataGridColumn column)
        {
            var propertyName = column.SortMemberPath;
            if (string.IsNullOrEmpty(propertyName)) return;

            var direction = column.SortDirection != System.ComponentModel.ListSortDirection.Ascending
                ? System.ComponentModel.ListSortDirection.Ascending
                : System.ComponentModel.ListSortDirection.Descending;

            Func<IEnumerable<ProductViewModel>, IOrderedEnumerable<ProductViewModel>>? sortFunc = null;

            switch (propertyName.ToLower())
            {
                case "id": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(p => p.Id) : items => items.OrderByDescending(p => p.Id); break;
                case "name": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(p => p.Name) : items => items.OrderByDescending(p => p.Name); break;
                case "code": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(p => p.Code) : items => items.OrderByDescending(p => p.Code); break;
                case "categoryname": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(p => p.CategoryName) : items => items.OrderByDescending(p => p.CategoryName); break;
                case "saleprice": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(p => p.SalePrice) : items => items.OrderByDescending(p => p.SalePrice); break;
                case "stockquantity": sortFunc = direction == System.ComponentModel.ListSortDirection.Ascending ? items => items.OrderBy(p => p.StockQuantity) : items => items.OrderByDescending(p => p.StockQuantity); break;
            }

            if (sortFunc != null)
            {
                _paginationHelper.SetSort(sortFunc);
                column.SortDirection = direction;
            }
        }
    }
}
