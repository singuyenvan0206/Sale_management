using FashionStore.App.Core;
using FashionStore.App.Views;
using FashionStore.Core.Models;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Settings;
using FashionStore.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace FashionStore.App.ViewModels
{
    public class InvoiceManagementViewModel : BaseViewModel
    {
        private List<CustomerListItem> _allCustomerItems = new();
        private List<ProductListItem> _allProducts = new();

        public ObservableCollection<CustomerListItem> FilteredCustomers { get; } = new();
        public ObservableCollection<ProductListItem> Products { get; } = new();
        public ObservableCollection<ProductListItem> FilteredProducts { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<InvoiceItemViewModel> InvoiceItems { get; } = new();
        public ObservableCollection<Voucher> Vouchers { get; } = new();
        public List<Promotion> ActivePromotions { get; set; } = new();

        public List<string> DiscountModes { get; } = new() { "VND", "%" };
        public List<string> PaymentMethods { get; } = new() { "💵 Tiền mặt", "💳 Thẻ", "🏦 Chuyển khoản" };

        private bool _isInternalUpdate = false;
        private int _invoiceDetailsLoadVersion = 0;
        private readonly ICalculationService _calculationService;
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerService _customerService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IBankStatementService _bankStatementService;

        #region Properties

        private string _customerSearchText = "";
        public string CustomerSearchText
        {
            get => _customerSearchText;
            set
            {
                if (SetProperty(ref _customerSearchText, value) && !_isSelectingCustomer)
                {
                    FilterCustomers(value);
                }
            }
        }

        private bool _isSelectingCustomer = false;

        private bool _isCustomerPopupOpen;
        public bool IsCustomerPopupOpen
        {
            get => _isCustomerPopupOpen;
            set => SetProperty(ref _isCustomerPopupOpen, value);
        }

        private CustomerListItem? _selectedCustomer;
        public CustomerListItem? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    if (value != null && !_isSelectingCustomer)
                    {
                        _isSelectingCustomer = true;
                        CustomerSearchText = value.Name;
                        IsCustomerPopupOpen = false;
                        _isSelectingCustomer = false;
                    }
                    try { OnCustomerSelected(); }
                    catch (Exception ex) { System.Windows.MessageBox.Show($"Lỗi chọn hiển thị tệp: {ex.Message}"); }
                }
            }
        }

        private string _barcodeText = "";
        public string BarcodeText
        {
            get => _barcodeText;
            set => SetProperty(ref _barcodeText, value);
        }

        private string _loyaltyTierText = "Regular";
        public string LoyaltyTierText { get => _loyaltyTierText; set => SetProperty(ref _loyaltyTierText, value); }

        private string _loyaltyPointsText = "0 điểm";
        public string LoyaltyPointsText { get => _loyaltyPointsText; set => SetProperty(ref _loyaltyPointsText, value); }

        private ProductListItem? _selectedProduct;
        public ProductListItem? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    OnProductSelected();
                }
            }
        }

        private string _quantityText = "1";
        public string QuantityText { get => _quantityText; set => SetProperty(ref _quantityText, value); }

        private string _unitPriceText = "";
        public string UnitPriceText { get => _unitPriceText; set => SetProperty(ref _unitPriceText, value); }

        private string _itemCountText = "0 mục";
        public string ItemCountText { get => _itemCountText; set => SetProperty(ref _itemCountText, value); }

        private string _subtotalText = "0₫";
        public string SubtotalText { get => _subtotalText; set => SetProperty(ref _subtotalText, value); }

        private Voucher? _selectedVoucher;
        public Voucher? SelectedVoucher
        {
            get => _selectedVoucher;
            set
            {
                if (SetProperty(ref _selectedVoucher, value))
                {
                    OnVoucherSelected();
                }
            }
        }

        private string _selectedDiscountMode = "VND";
        public string SelectedDiscountMode
        {
            get => _selectedDiscountMode;
            set
            {
                if (SetProperty(ref _selectedDiscountMode, value)) UpdateTotals();
            }
        }

        private string _discountValueText = "0";
        public string DiscountValueText
        {
            get => _discountValueText;
            set
            {
                if (SetProperty(ref _discountValueText, value)) UpdateTotals();
            }
        }

        private string _tierDiscountInlineText = "(+ Ưu đãi hạng: 0₫)";
        public string TierDiscountInlineText { get => _tierDiscountInlineText; set => SetProperty(ref _tierDiscountInlineText, value); }

        private string _taxAmountText = "0₫";
        public string TaxAmountText { get => _taxAmountText; set => SetProperty(ref _taxAmountText, value); }

        private bool _isRedeemingPoints = false;
        public bool IsRedeemingPoints
        {
            get => _isRedeemingPoints;
            set
            {
                if (SetProperty(ref _isRedeemingPoints, value)) UpdateTotals();
            }
        }

        private string _totalText = "0₫";
        public string TotalText { get => _totalText; set => SetProperty(ref _totalText, value); }

        private string _selectedPaymentMethod = "💵 Tiền mặt";
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set
            {
                if (SetProperty(ref _selectedPaymentMethod, value)) UpdateTotals();
            }
        }

        private string _paidText = "0";
        public string PaidText
        {
            get => _paidText;
            set
            {
                if (SetProperty(ref _paidText, value)) UpdateTotals();
            }
        }

        private string _changeText = "0₫";
        public string ChangeText { get => _changeText; set => SetProperty(ref _changeText, value); }

        private ImageSource? _qrCodeImage;
        public ImageSource? QRCodeImage { get => _qrCodeImage; set => SetProperty(ref _qrCodeImage, value); }

        private bool _isQRCodeVisible = false;
        public bool IsQRCodeVisible { get => _isQRCodeVisible; set => SetProperty(ref _isQRCodeVisible, value); }

        private string _qrPlaceholderText = "Chọn 'Chuyển khoản' để hiển thị QR";
        public string QRPlaceholderText { get => _qrPlaceholderText; set => SetProperty(ref _qrPlaceholderText, value); }

        private bool _isQRPlaceholderVisible = true;
        public bool IsQRPlaceholderVisible { get => _isQRPlaceholderVisible; set => SetProperty(ref _isQRPlaceholderVisible, value); }

        private bool _isVerifyingPayment;
        public bool IsVerifyingPayment { get => _isVerifyingPayment; set => SetProperty(ref _isVerifyingPayment, value); }

        private string _verificationStatusText = "";
        public string VerificationStatusText { get => _verificationStatusText; set => SetProperty(ref _verificationStatusText, value); }

        private string _currentQRDescription = "";
        public string CurrentQRDescription { get => _currentQRDescription; set => SetProperty(ref _currentQRDescription, value); }

        #region POS Properties
        private string _productSearchText = "";
        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                if (SetProperty(ref _productSearchText, value))
                {
                    // Barcode logic: if exactly 1 product matches and text looks like a code (optional check)
                    var exactMatch = _allProducts.FirstOrDefault(p => p.Name.EndsWith($"({value})", StringComparison.OrdinalIgnoreCase) || p.Id.ToString() == value);
                    if (exactMatch != null && value.Length >= 3)
                    {
                        AddProductDirectly(exactMatch);
                        _productSearchText = ""; // Reset search after adding
                        OnPropertyChanged(nameof(ProductSearchText));
                    }
                    else
                    {
                        FilterProducts(value, SelectedCategory?.Id);
                    }
                }
            }
        }

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterProducts(ProductSearchText, value?.Id);
                }
            }
        }
        #endregion

        #region Order Management Properties
        public ObservableCollection<Invoice> FilteredInvoices { get; } = new();
        private List<Invoice> _allInvoices = new();

        private string _orderSearchText = "";
        public string OrderSearchText
        {
            get => _orderSearchText;
            set
            {
                if (SetProperty(ref _orderSearchText, value)) FilterOrders();
            }
        }

        private DateTime? _selectedOrderDate = DateTime.Today;
        public DateTime? SelectedOrderDate
        {
            get => _selectedOrderDate;
            set
            {
                if (SetProperty(ref _selectedOrderDate, value)) FilterOrders();
            }
        }

        private Invoice? _selectedInvoice;
        public Invoice? SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                if (SetProperty(ref _selectedInvoice, value)) _ = LoadSelectedInvoiceDetailsSafeAsync(value);
            }
        }

        public ObservableCollection<InvoiceItem> SelectedInvoiceItems { get; } = new();
        #endregion

        #region Operational Properties
        private string _invoiceNote = "";
        public string InvoiceNote { get => _invoiceNote; set => SetProperty(ref _invoiceNote, value); }

        public static ObservableCollection<SuspendedOrder> SuspendedOrders { get; } = new();

        private bool _isPendingOrdersPopupOpen;
        public bool IsPendingOrdersPopupOpen { get => _isPendingOrdersPopupOpen; set => SetProperty(ref _isPendingOrdersPopupOpen, value); }
        #endregion

        #region Commands
        public ICommand SelectCustomerCommand { get; }
        public ICommand AddCustomerCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand IncreaseQtyCommand { get; }
        public ICommand DecreaseQtyCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearInvoiceCommand { get; }
        public ICommand SaveInvoiceCommand { get; }
        public ICommand OpenHistoryCommand { get; }
        public ICommand AddProductDirectlyCommand { get; }
        public ICommand FilterByCategoryCommand { get; }
        public ICommand ClearVoucherCommand { get; }
        
        // NEW COMMANDS
        public ICommand SuspendOrderCommand { get; }
        public ICommand ResumeOrderCommand { get; }
        public ICommand TogglePendingOrdersCommand { get; }
        public ICommand EditItemNoteCommand { get; }
        public ICommand RefreshOrdersCommand { get; }
        public ICommand ReprintInvoiceCommand { get; }
        public ICommand RefundInvoiceCommand { get; }
        public ICommand ScanBarcodeCommand { get; }
        public ICommand VerifyPaymentCommand { get; }
        public ICommand OpenWebcamScannerCommand { get; }
        #endregion

        public InvoiceManagementViewModel() : this(null!) { }

        public InvoiceManagementViewModel(ICalculationService? calculationService = null, 
                                         IInvoiceService? invoiceService = null,
                                         ICustomerService? customerService = null,
                                         IUserService? userService = null,
                                         IProductService? productService = null,
                                         IBankStatementService? bankStatementService = null)
        {
            _calculationService = calculationService ??
                                 (App.ServiceProvider?.GetService<ICalculationService>() ?? new CalculationService());
            
            _invoiceService = invoiceService ?? 
                             (App.ServiceProvider?.GetService<IInvoiceService>() ?? ServiceLocator.ServiceProvider?.GetService<IInvoiceService>()!);
            
            _customerService = customerService ?? 
                              (App.ServiceProvider?.GetService<ICustomerService>() ?? ServiceLocator.ServiceProvider?.GetService<ICustomerService>()!);

            _userService = userService ??
                           (App.ServiceProvider?.GetService<IUserService>() ?? ServiceLocator.ServiceProvider?.GetService<IUserService>()!);

            _productService = productService ??
                            (App.ServiceProvider?.GetService<IProductService>() ?? ServiceLocator.ServiceProvider?.GetService<IProductService>()!);

            SelectCustomerCommand = new RelayCommand(p =>
            {
                if (p is CustomerListItem customer) SelectCustomer(customer);
            });
            AddItemCommand = new RelayCommand(_ => AddItem());
            IncreaseQtyCommand = new RelayCommand(p => IncreaseQty(p as InvoiceItemViewModel));
            DecreaseQtyCommand = new RelayCommand(p => DecreaseQty(p as InvoiceItemViewModel));
            RemoveItemCommand = new RelayCommand(p => RemoveItem(p as InvoiceItemViewModel));
            ClearInvoiceCommand = new RelayCommand(_ => { ClearInvoice(); UpdateTotals(); });
            SaveInvoiceCommand = new RelayCommand(_ => _ = SaveInvoiceAsync());
            OpenHistoryCommand = new RelayCommand(_ => OpenHistory());
            AddProductDirectlyCommand = new RelayCommand(p => AddProductDirectly(p as ProductListItem));
            FilterByCategoryCommand = new RelayCommand(p => SelectedCategory = p as Category);
            ClearVoucherCommand = new RelayCommand(_ => SelectedVoucher = null);

            // NEW COMMANDS
            AddCustomerCommand = new RelayCommand(_ => _ = AddNewCustomerAsync());
            SuspendOrderCommand = new RelayCommand(_ => SuspendCurrentOrder());
            TogglePendingOrdersCommand = new RelayCommand(_ => IsPendingOrdersPopupOpen = !IsPendingOrdersPopupOpen);
            ResumeOrderCommand = new RelayCommand(p => ResumeOrder(p as SuspendedOrder));
            EditItemNoteCommand = new RelayCommand(p => EditItemNote(p as InvoiceItemViewModel));
            RefreshOrdersCommand = new RelayCommand(_ => _ = LoadInvoicesAsync());
            ReprintInvoiceCommand = new RelayCommand(_ => ReprintSelectedInvoice());
            RefundInvoiceCommand = new RelayCommand(_ => _ = RefundSelectedInvoiceAsync());
            ScanBarcodeCommand = new RelayCommand(_ => SelectProductByBarcode(BarcodeText));
            VerifyPaymentCommand = new RelayCommand(_ => _ = VerifyPaymentAsync());
            OpenWebcamScannerCommand = new RelayCommand(_ => OpenWebcamScanner());

            _bankStatementService = bankStatementService ??
                                   (App.ServiceProvider?.GetService<IBankStatementService>() ?? ServiceLocator.ServiceProvider?.GetService<IBankStatementService>()!);

            _ = InitializeDataAsync();
        }
        private async System.Threading.Tasks.Task InitializeDataAsync()
        {
            // Run all 4 DB queries IN PARALLEL on thread pool - no UI blocking
            var t1 = System.Threading.Tasks.Task.Run(() => CustomerService.GetAllCustomers());
            var t2 = System.Threading.Tasks.Task.Run(() => ProductService.GetAllProductsWithCategories());
            var t3 = System.Threading.Tasks.Task.Run(() => VoucherService.GetAllVouchers());
            var t4 = System.Threading.Tasks.Task.Run(() => { try { return PromotionService.GetActivePromotions(); } catch { return new System.Collections.Generic.List<Promotion>(); } });

            await System.Threading.Tasks.Task.WhenAll(t1, t2, t3, t4);

            // Apply results on UI thread
            var customers = t1.Result;
            _allCustomerItems = customers.ConvertAll(c => new CustomerListItem { Id = c.Id, Name = c.Name, Phone = c.Phone });
            FilterCustomers("");
            if (_allCustomerItems.Count > 0) SelectCustomer(_allCustomerItems[0]);

            var products = t2.Result;
            var list = products.ConvertAll(p => new ProductListItem
            {
                Id = p.Id,
                Name = string.IsNullOrWhiteSpace(p.Code) ? p.Name : $"{p.Name} ({p.Code})",
                Code = p.Code,
                UnitPrice = p.SalePrice,
                PromoDiscountPercent = p.PromoDiscountPercent,
                PromoStartDate = p.PromoStartDate,
                PromoEndDate = p.PromoEndDate,
                StockQuantity = p.StockQuantity,
                CategoryTaxPercent = p.CategoryTaxPercent,
                CategoryId = p.CategoryId
            });
            Products.Clear();
            foreach (var p in list) Products.Add(p);
            _allProducts = list;

            var categories = await System.Threading.Tasks.Task.Run(() => CategoryService.GetAllCategories());
            Categories.Clear();
            Categories.Add(new Category { Id = 0, Name = "Tất cả" });
            foreach (var cat in categories)
            {
                Categories.Add(new Category { Id = cat.Id, Name = cat.Name, TaxPercent = cat.TaxPercent });
            }

            FilterProducts("", null);

            var vouchers = t3.Result;
            Vouchers.Clear();
            foreach (var v in vouchers.Where(v => v.IsActive)) Vouchers.Add(v);

            ActivePromotions = t4.Result;

            try { PaymentSettingsManager.Load(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Payment settings load failed: {ex.Message}"); }
            ClearInvoice();
            UpdateTotals();
        }

        private void InitializeData() => _ = InitializeDataAsync();

        private void LoadPromotions()
        {
            try { ActivePromotions = PromotionService.GetActivePromotions(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Load promotions failed: {ex.Message}"); }
        }

        #region Customer Logic
        private void LoadCustomers()
        {
            try
            {
                var customers = CustomerService.GetAllCustomers();
                _allCustomerItems = customers.ConvertAll(c => new CustomerListItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Phone = c.Phone
                });

                FilterCustomers("");

                if (_allCustomerItems.Count > 0)
                {
                    SelectCustomer(_allCustomerItems[0]);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Load customers failed: {ex.Message}"); }
        }

        private void FilterCustomers(string text)
        {
            FilteredCustomers.Clear();
            var filtered = string.IsNullOrWhiteSpace(text)
                ? _allCustomerItems
                : _allCustomerItems.Where(c =>
                    (c.Name?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Phone?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    c.Id.ToString().Contains(text, StringComparison.OrdinalIgnoreCase)).Take(50);

            foreach (var c in filtered) FilteredCustomers.Add(c);

            IsCustomerPopupOpen = FilteredCustomers.Count > 0;
        }

        private void SelectCustomer(CustomerListItem customer)
        {
            _isSelectingCustomer = true;
            SelectedCustomer = customer;
            CustomerSearchText = customer.Name;
            IsCustomerPopupOpen = false;
            _isSelectingCustomer = false;
        }

        private void OnCustomerSelected()
        {
            UpdateLoyaltyDisplay();
            UpdateTotals();
        }

        private void UpdateLoyaltyDisplay()
        {
            var (tier, points) = SelectedCustomer != null ? CustomerService.GetCustomerLoyalty(SelectedCustomer.Id) : ("Regular", 0);
            LoyaltyTierText = tier;
            LoyaltyPointsText = $"{points} điểm";
        }
        #endregion

        #region Product Logic
        private async Task ReloadProductsAsync()
        {
            try
            {
                var products = await Task.Run(() => ProductService.GetAllProductsWithCategories());
                var list = products.ConvertAll(p => new ProductListItem
                {
                    Id = p.Id,
                    Name = string.IsNullOrWhiteSpace(p.Code) ? p.Name : $"{p.Name} ({p.Code})",
                    Code = p.Code,
                    UnitPrice = p.SalePrice,
                    PromoDiscountPercent = p.PromoDiscountPercent,
                    PromoStartDate = p.PromoStartDate,
                    PromoEndDate = p.PromoEndDate,
                    StockQuantity = p.StockQuantity,
                    CategoryTaxPercent = p.CategoryTaxPercent,
                    CategoryId = p.CategoryId
                });

                Products.Clear();
                foreach (var p in list) Products.Add(p);
                _allProducts = list;
                FilterProducts(ProductSearchText, SelectedCategory?.Id);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Reload products failed: {ex.Message}"); }
        }

        private void LoadProducts() => _ = ReloadProductsAsync();

        private void OnProductSelected()
        {
            if (SelectedProduct != null)
            {
                var finalPrice = GetFinalPromoPrice(SelectedProduct);
                UnitPriceText = finalPrice.ToString("F2");
                if (string.IsNullOrWhiteSpace(QuantityText)) QuantityText = "1";
            }
            UpdateTotals();
        }

        private decimal GetFinalPromoPrice(ProductListItem product)
        {
            decimal basePrice = product.UnitPrice;
            decimal bestDiscountPrice = basePrice;

            // 1. Base Product Discount (Percentage-based)
            if (product.PromoDiscountPercent > 0)
            {
                var now = DateTime.Now;
                if ((!product.PromoStartDate.HasValue || now >= product.PromoStartDate.Value) &&
                    (!product.PromoEndDate.HasValue || now <= product.PromoEndDate.Value))
                {
                    decimal discounted = _calculationService.ApplyPercentDiscount(basePrice, product.PromoDiscountPercent);
                    if (discounted < bestDiscountPrice) bestDiscountPrice = discounted;
                }
            }

            // 2. Active Promotions (Flash Sales)
            var flashSales = ActivePromotions.Where(p => p.Type == Promotion.TypeFlashSale && (p.RequiredProductId == product.Id || p.TargetCategoryId == product.CategoryId || (p.RequiredProductId == null && p.TargetCategoryId == null)));
            foreach (var fs in flashSales)
            {
                decimal promoPrice = basePrice;
                if (fs.DiscountPercent > 0)
                {
                    promoPrice = _calculationService.ApplyPercentDiscount(basePrice, fs.DiscountPercent);
                }
                else if (fs.DiscountAmount > 0)
                {
                    promoPrice = Math.Max(0, basePrice - fs.DiscountAmount);
                }

                if (promoPrice < bestDiscountPrice) bestDiscountPrice = promoPrice;
            }

            return bestDiscountPrice;
        }
        private void FilterProducts(string searchText, int? categoryId)
        {
            FilteredProducts.Clear();
            var query = _allProducts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            }

            if (categoryId.HasValue && categoryId.Value != 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            foreach (var p in query) FilteredProducts.Add(p);
        }

        private void AddProductDirectly(ProductListItem? product)
        {
            if (product == null) return;
            SelectedProduct = product;
            QuantityText = "1";
            AddItem();
        }
        #endregion

        #region Invoice Items Logic

        private void AddItem()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm.", "Xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityText, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng hợp lệ.", "Xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int currentStock = ProductService.GetProductStockQuantity(SelectedProduct.Id);
            var existingItem = InvoiceItems.FirstOrDefault(i => i.ProductId == SelectedProduct.Id);
            int requestedQuantity = quantity + (existingItem?.Quantity ?? 0);

            if (requestedQuantity > currentStock)
            {
                MessageBox.Show($"Không đủ hàng! Sản phẩm '{SelectedProduct.Name}' chỉ còn {currentStock} sản phẩm trong kho.", "Hết hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal unitPrice = decimal.TryParse(UnitPriceText, out var p) && p >= 0 ? p : GetFinalPromoPrice(SelectedProduct);

            if (existingItem != null)
            {
                existingItem.Quantity = requestedQuantity;
                existingItem.LineTotal = existingItem.UnitPrice * existingItem.Quantity;
            }
            else
            {
                InvoiceItems.Add(new InvoiceItemViewModel
                {
                    ProductId = SelectedProduct.Id,
                    ProductName = SelectedProduct.Name,
                    PromoDiscountPercent = 0, // No longer strictly percent-based, price is pre-calculated
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    LineTotal = unitPrice * quantity,
                    CategoryTaxPercent = SelectedProduct.CategoryTaxPercent
                });
            }

            RefreshItems();
            QuantityText = "1";
            UnitPriceText = "";
            UpdateTotals();
        }

        private void IncreaseQty(InvoiceItemViewModel? item)
        {
            if (item == null) return;
            int currentStock = ProductService.GetProductStockQuantity(item.ProductId);
            if (item.Quantity + 1 > currentStock)
            {
                MessageBox.Show($"Không đủ hàng! Sản phẩm '{item.ProductName}' chỉ còn {currentStock} trong kho.", "Hết hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            item.Quantity++;
            item.LineTotal = item.UnitPrice * item.Quantity;
            RefreshItems();
            UpdateTotals();
        }

        private void DecreaseQty(InvoiceItemViewModel? item)
        {
            if (item == null) return;
            if (item.Quantity > 1)
            {
                item.Quantity--;
                item.LineTotal = item.UnitPrice * item.Quantity;
            }
            else
            {
                InvoiceItems.Remove(item);
            }
            RefreshItems();
            UpdateTotals();
        }

        private void RemoveItem(InvoiceItemViewModel? item)
        {
            if (item == null) return;
            InvoiceItems.Remove(item);
            RefreshItems();
            UpdateTotals();
        }

        private void RefreshItems()
        {
            for (int i = 0; i < InvoiceItems.Count; i++)
            {
                InvoiceItems[i].RowNumber = i + 1;
            }
            // Trigger refresh in UI
            var items = InvoiceItems.ToList();
            InvoiceItems.Clear();
            foreach (var item in items) InvoiceItems.Add(item);

            ItemCountText = $"{InvoiceItems.Count} mục";
        }

        private void ClearInvoice(bool showConfirmation = true)
        {
            if (showConfirmation && InvoiceItems.Count > 0)
            {
                var result = MessageBox.Show("Bạn có chắc muốn làm mới hóa đơn? Tất cả mặt hàng đã nhập sẽ bị xóa.", 
                                          "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
            }
            InvoiceItems.Clear();
            ItemCountText = "0 mục";
        }

        #endregion

        #region Totals Logic
        private void LoadVouchers()
        {
            try
            {
                var vouchers = VoucherService.GetAllVouchers().Where(v => v.IsActive).ToList();
                Vouchers.Clear();
                foreach (var v in vouchers) Vouchers.Add(v);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Load vouchers failed: {ex.Message}"); }
        }

        private void OnVoucherSelected()
        {
            if (_isInternalUpdate) return;
            _isInternalUpdate = true;
            try
            {
                if (SelectedVoucher != null)
                {
                    SelectedDiscountMode = SelectedVoucher.DiscountType == Voucher.TypePercentage || SelectedVoucher.DiscountType == "%" ? "%" : "VND";
                    DiscountValueText = SelectedVoucher.DiscountValue.ToString("G29");
                }
                else
                {
                    DiscountValueText = "0";
                }
            }
            finally
            {
                _isInternalUpdate = false;
                UpdateTotals();
            }
        }

        private void UpdateTotals()
        {
            if (_isInternalUpdate) return;
            _isInternalUpdate = true;
            try
            {
                ApplyBOGOPromotions();

                var subtotal = InvoiceItems.Sum(item => item.LineTotal);
                SubtotalText = subtotal.ToString("N0") + "₫";

                var discountVal = decimal.TryParse(DiscountValueText, out var dv) ? dv : 0m;
                var discount = _calculationService.CalculateDiscount(subtotal, SelectedDiscountMode, discountVal);

                var (tier, points) = SelectedCustomer != null ? CustomerService.GetCustomerLoyalty(SelectedCustomer.Id) : ("Regular", 0);
                var tierDiscountPercent = TierSettingsManager.GetTierDiscount(tier);
                var tierDiscount = _calculationService.CalculateTierDiscount(subtotal, tierDiscountPercent);

                // Redemption logic
                decimal redemptionDiscount = 0;
                if (IsRedeemingPoints && points > 0)
                {
                    // Assume 1 point = 1000 VND (or check settings)
                    decimal pointValue = points * 1000; 
                    redemptionDiscount = Math.Min(subtotal - discount - tierDiscount, pointValue);
                }

                TierDiscountInlineText = $"(+ Ưu đãi hạng: {tierDiscount:N0}₫" + (redemptionDiscount > 0 ? $" | Dùng điểm: {redemptionDiscount:N0}₫" : "") + ")";

                var totalDiscount = discount + tierDiscount + redemptionDiscount;
                if (SelectedVoucher != null && (SelectedVoucher.DiscountType == Voucher.TypePercentage || SelectedVoucher.DiscountType == "%") && SelectedVoucher.MaxDiscountAmount > 0)
                {
                    totalDiscount = Math.Min(totalDiscount, SelectedVoucher.MaxDiscountAmount + tierDiscount + redemptionDiscount);
                }

                decimal discountRatio = subtotal > 0 ? totalDiscount / subtotal : 0;
                var taxAmount = InvoiceItems.Sum(item => _calculationService.CalculateTaxAmount(item.LineTotal, discountRatio, item.CategoryTaxPercent));

                var total = Math.Max(0, subtotal + taxAmount - totalDiscount);

                TaxAmountText = taxAmount.ToString("N0") + "₫";
                TotalText = total.ToString("N0") + "₫";

                var paid = decimal.TryParse(PaidText, out var p) ? p : 0m;
                var change = Math.Max(0, paid - total);
                ChangeText = change.ToString("N0") + "₫";

                UpdateQRCode(total);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Update totals failed: {ex.Message}"); }
            finally { _isInternalUpdate = false; }
        }

        private void ApplyBOGOPromotions()
        {
            // Remove existing rewards first to recalculate cleanly
            var existingRewards = InvoiceItems.Where(i => i.IsReward).ToList();
            foreach (var r in existingRewards) InvoiceItems.Remove(r);

            var bogoPromos = ActivePromotions.Where(p => p.Type == Promotion.TypeBOGO && p.RequiredProductId.HasValue && p.RewardProductId.HasValue);

            foreach (var bogo in bogoPromos)
            {
                int requiredProductId = bogo.RequiredProductId ?? 0;
                int rewardProductId = bogo.RewardProductId ?? 0;
                if (requiredProductId <= 0 || rewardProductId <= 0) continue;

                var requiredItem = InvoiceItems.FirstOrDefault(i => i.ProductId == requiredProductId && !i.IsReward);
                if (requiredItem != null && bogo.RequiredQuantity > 0)
                {
                    int sets = requiredItem.Quantity / bogo.RequiredQuantity;
                    if (sets > 0)
                    {
                        int rewardQty = sets * bogo.RewardQuantity;
                        var rewardProduct = Products.FirstOrDefault(p => p.Id == rewardProductId);
                        if (rewardProduct != null)
                        {
                            // Check stock
                            int rewardStock = ProductService.GetProductStockQuantity(rewardProduct.Id);
                            int actualRewardQty = Math.Min(rewardQty, rewardStock);

                            if (actualRewardQty > 0)
                            {
                                InvoiceItems.Add(new InvoiceItemViewModel
                                {
                                    ProductId = rewardProduct.Id,
                                    ProductName = rewardProduct.Name + " (Quà tặng BOGO)",
                                    UnitPrice = rewardProduct.UnitPrice,
                                    PromoDiscountPercent = 100, // 100% OFF for Reward
                                    LineTotal = 0,
                                    Quantity = actualRewardQty,
                                    CategoryTaxPercent = 0,
                                    IsReward = true
                                });
                            }
                        }
                    }
                }
            }

            // Re-sequence rows
            for (int i = 0; i < InvoiceItems.Count; i++) InvoiceItems[i].RowNumber = i + 1;
        }

        private void ApplyBestVoucher(decimal subtotal)
        {
            if (SelectedCustomer == null || Vouchers.Count == 0 || _isInternalUpdate) return;
            var bestVoucher = Vouchers
                .Where(v => v.IsValid(subtotal, VoucherService.GetVoucherUsageCountForCustomer(v.Id, SelectedCustomer.Id)))
                .OrderByDescending(v => _calculationService.CalculateVoucherValue(subtotal, v))
                .FirstOrDefault();

            if (SelectedVoucher != bestVoucher)
            {
                _isInternalUpdate = true;
                SelectedVoucher = bestVoucher;
                if (SelectedVoucher != null)
                {
                    SelectedDiscountMode = SelectedVoucher.DiscountType == Voucher.TypePercentage || SelectedVoucher.DiscountType == "%" ? "%" : "VND";
                    DiscountValueText = SelectedVoucher.DiscountValue.ToString("G29");
                }
                else
                {
                    DiscountValueText = "0";
                }
                _isInternalUpdate = false;
            }
        }
        #endregion

        #region QR Code Logic
        private void UpdateQRCode(decimal total)
        {
            try
            {
                var paymentSettings = PaymentSettingsManager.Load();
                bool isBankTransfer = SelectedPaymentMethod?.Contains("Chuyển khoản") == true;

                if (!isBankTransfer || !paymentSettings.EnableQRCode)
                {
                    QRCodeImage = null;
                    IsQRCodeVisible = false;
                    IsQRPlaceholderVisible = true;
                    QRPlaceholderText = "Chọn 'Chuyển khoản' để hiển thị QR";
                    return;
                }

                var paid = decimal.TryParse(PaidText, out var p) ? p : 0m;
                var amountDue = Math.Max(0, total - paid);

                if (amountDue <= 0)
                {
                    QRCodeImage = CreateErrorQRCode("Vui lòng thêm sản phẩm để tạo QR thanh toán", 120);
                    IsQRCodeVisible = true;
                    IsQRPlaceholderVisible = true;
                    QRPlaceholderText = "Vui lòng thêm sản phẩm để tạo QR thanh toán";
                    return;
                }

                if (string.IsNullOrEmpty(paymentSettings.BankAccount) || string.IsNullOrEmpty(paymentSettings.BankCode))
                {
                    QRCodeImage = CreateErrorQRCode("Chưa cấu hình thông tin ngân hàng", 120);
                    IsQRCodeVisible = true;
                    IsQRPlaceholderVisible = true;
                    QRPlaceholderText = "Chưa cấu hình thông tin ngân hàng. Vui lòng vào Settings để thiết lập";
                    return;
                }

                var description = "FS" + DateTime.Now.ToString("HHmmss");
                CurrentQRDescription = description;

                QRCodeImage = QRCodeHelper.GenerateVietQRCode_Safe(
                    paymentSettings.BankCode.ToLower(),
                    paymentSettings.BankAccount,
                    amountDue,
                    description,
                    true,
                    370,
                    paymentSettings.AccountHolder);

                IsQRCodeVisible = true;
                IsQRPlaceholderVisible = false;
            }
            catch
            {
                QRCodeImage = CreateErrorQRCode("Lỗi tạo QR code", 120);
                IsQRCodeVisible = true;
                IsQRPlaceholderVisible = true;
                QRPlaceholderText = "Lỗi tạo QR code";
            }
        }

        private async Task VerifyPaymentAsync()
        {
            if (IsVerifyingPayment) return;

            decimal totalAmount = decimal.TryParse(TotalText.Replace("₫", "").Replace(".", "").Replace(",", ""), out var t) ? t : 0;
            if (totalAmount <= 0) return;

            IsVerifyingPayment = true;
            VerificationStatusText = "🔄 Đang kiểm tra giao dịch...";

            try
            {
                // Give the bank a moment to process (optional, but UI feedback is good)
                await Task.Delay(1000);

                var transaction = await _bankStatementService.VerifyPaymentAsync(totalAmount, CurrentQRDescription);

                if (transaction != null)
                {
                    VerificationStatusText = "✅ Đã nhận tiền!";
                    PaidText = totalAmount.ToString("N0");
                    UpdateTotals();
                    
                    MessageBox.Show($"Xác nhận: Đã nhận {transaction.Amount:N0}đ từ giao dịch {transaction.ReferenceCode}.\nHóa đơn đã sẵn sàng để lưu.", 
                                    "Thanh toán thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    VerificationStatusText = "❌ Chưa tìm thấy giao dịch.";
                    MessageBox.Show("Chưa tìm thấy giao dịch khớp với nội dung và số tiền này.\nVui lòng đợi 1-2 phút sau khi chuyển và thử lại.", 
                                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                VerificationStatusText = "⚠️ Lỗi khi kiểm tra.";
                MessageBox.Show($"Lỗi kết nối ngân hàng: {ex.Message}");
            }
            finally
            {
                IsVerifyingPayment = false;
            }
        }

        private BitmapSource CreateErrorQRCode(string message, int size)
        {
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.Red, 2), new Rect(0, 0, size, size));
                var formattedText = new FormattedText(message, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), Math.Min(10, size / 12.0), Brushes.Red, 96);
                double x = (size - formattedText.Width) / 2;
                double y = (size - formattedText.Height) / 2;
                drawingContext.DrawText(formattedText, new Point(x, y));
            }
            var b = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            b.Render(drawingVisual);
            return b;
        }
        #endregion

        #region Save Invoice
        private async System.Threading.Tasks.Task SaveInvoiceAsync()
        {
            if (InvoiceItems.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một sản phẩm.", "Xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                // -- BANK TRANSFER FLOW --
                if (SelectedPaymentMethod.Contains("Chuyển khoản"))
                {
                    var bw = new BankTransferWindow(this);
                    var dialogResult = bw.ShowDialog();
                    if (dialogResult != true) return; // User cancelled or payment not verified
                }

                var subtotal = InvoiceItems.Sum(item => item.LineTotal);
                var discountVal = decimal.TryParse(DiscountValueText, out var dv) ? dv : 0m;
                var manualDiscount = SelectedDiscountMode == "%" ? Math.Round(subtotal * (discountVal / 100m), 2) : discountVal;
                var paid = decimal.TryParse(PaidText, out var p) ? p : 0m;
                var itemsSnapshot = InvoiceItems
                    .Select(i => new InvoiceItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        PromoDiscountPercent = i.PromoDiscountPercent,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        LineTotal = i.LineTotal,
                        CategoryTaxPercent = i.CategoryTaxPercent,
                        Note = i.Note,
                        IsReward = i.IsReward
                    })
                    .ToList();
                var voucherId = SelectedVoucher?.Id;
                
                // Safety check for Customer
                if (SelectedCustomer == null)
                {
                    MessageBox.Show("Vui lòng chọn khách hàng trước khi thanh toán.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var customerId = SelectedCustomer.Id;

                var currentUser = Application.Current.Resources["CurrentUser"]?.ToString() ?? "admin";

                var loyalty = CustomerService.GetCustomerLoyalty(customerId);
                var tierDiscountPercent = TierSettingsManager.GetTierDiscount(loyalty.Tier);
                var tierDiscount = Math.Round(subtotal * (tierDiscountPercent / 100m), 2);

                // Redemption logic
                decimal redemptionDiscount = 0;
                if (IsRedeemingPoints && loyalty.Points > 0)
                {
                    decimal pointValue = loyalty.Points * 1000;
                    redemptionDiscount = Math.Min(subtotal - manualDiscount - tierDiscount, pointValue);
                }

                var totalDiscount = manualDiscount + tierDiscount + redemptionDiscount;
                decimal discountRatio = subtotal > 0 ? totalDiscount / subtotal : 0;
                var taxAmount = itemsSnapshot.Sum(item => _calculationService.CalculateTaxAmount(item.LineTotal, discountRatio, item.CategoryTaxPercent));
                var total = Math.Max(0, subtotal + taxAmount - totalDiscount);

                var empId = 1;
                try
                {
                    var employeeIdResult = await _userService.GetEmployeeIdByUsernameAsync(currentUser);
                    if (employeeIdResult.IsSuccess && employeeIdResult.Value > 0)
                    {
                        empId = employeeIdResult.Value;
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Resolve employee failed: {ex.Message}"); }

                var invoiceModel = new Invoice
                {
                    CustomerId = customerId,
                    EmployeeId = empId,
                    Subtotal = subtotal,
                    TaxAmount = taxAmount,
                    Discount = totalDiscount,
                    Total = total,
                    Paid = paid,
                    PaymentMethod = NormalizePaymentMethod(SelectedPaymentMethod),
                    VoucherId = voucherId,
                    Note = InvoiceNote,
                    Status = "Completed",
                    Items = itemsSnapshot.Select(i => new InvoiceItem
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        LineTotal = i.LineTotal,
                        Note = i.Note,
                        EmployeeId = empId
                    }).ToList()
                };

                var saved = await _invoiceService.SaveInvoiceAsync(invoiceModel, voucherId);
                var resultData = new { EmployeeId = empId, Result = saved, InvoiceId = invoiceModel.Id };

                if (!resultData.Result)
                {
                    MessageBox.Show("Không thể lưu hóa đơn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show($"Hóa đơn #{resultData.InvoiceId} đã được lưu.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh stock shown in POS immediately after successful checkout.
                await ReloadProductsAsync();

                // Loyalty is already updated in invoice repository transaction.

                ClearInvoice(false);
                DiscountValueText = "0";
                PaidText = "0";
                UpdateTotals();

                try { new InvoicePrintWindow(resultData.InvoiceId, resultData.EmployeeId).ShowDialog(); }
                catch (Exception ex) { MessageBox.Show($"Lỗi hiển thị cửa sổ in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }

                // Keep invoice management tab in sync after checkout/print.
                await LoadInvoicesAsync();
                var newInvoice = _allInvoices.FirstOrDefault(i => i.Id == resultData.InvoiceId);
                if (newInvoice != null)
                {
                    SelectedInvoice = newInvoice;
                }

                DashboardViewModel.TriggerDashboardRefresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveInvoice() => _ = SaveInvoiceAsync();

        private void OpenHistory()
        {
            try
            {
                // Navigate by opening the dedicated TransactionHistoryWindow as a popup
                // instead of trying to find a missing TabControl index.
                var historyWin = new FashionStore.App.Views.TransactionHistoryWindow(SelectedCustomer?.Id);
                
                // Try to set owner to the current dashboard if possible
                if (Application.Current.MainWindow is FashionStore.App.Views.DashboardWindow dashboard)
                {
                    historyWin.Owner = dashboard;
                    historyWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    historyWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                
                historyWin.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở lịch sử giao dịch: {ex.Message}");
            }
        }

        #endregion

        #region Operational Logic (New)
        private async Task AddNewCustomerAsync()
        {
            // Simple approach: add a generic walking customer with details
            string name = "Khách lẻ " + DateTime.Now.ToString("HH:mm");
            try
            {
                await _customerService.AddCustomerAsync(new Customer { Name = name, Phone = "0000", Address = "Tại quầy", CustomerType = "Regular" });
                LoadCustomers();
                var added = _allCustomerItems.OrderByDescending(c => c.Id).FirstOrDefault();
                if (added != null) SelectCustomer(added);
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi thêm khách nhanh: {ex.Message}"); }
        }

        private void SuspendCurrentOrder()
        {
            if (InvoiceItems.Count == 0) return;

            // Use underlying raw values instead of parsing formatted UI strings
            var subtotal = InvoiceItems.Sum(item => item.LineTotal);
            var discountVal = decimal.TryParse(DiscountValueText, out var dv) ? dv : 0m;
            var discount = _calculationService.CalculateDiscount(subtotal, SelectedDiscountMode, discountVal);
            var (tier, _) = SelectedCustomer != null ? CustomerService.GetCustomerLoyalty(SelectedCustomer.Id) : ("Regular", 0);
            var tierDiscount = _calculationService.CalculateTierDiscount(subtotal, TierSettingsManager.GetTierDiscount(tier));
            var taxAmount = InvoiceItems.Sum(i => _calculationService.CalculateTaxAmount(i.LineTotal, subtotal > 0 ? (discount + tierDiscount) / subtotal : 0, i.CategoryTaxPercent));
            
            var suspended = new SuspendedOrder
            {
                Customer = SelectedCustomer,
                Items = InvoiceItems.ToList(),
                Total = Math.Max(0, subtotal + taxAmount - (discount + tierDiscount)),
                Note = InvoiceNote
            };
            
            SuspendedOrders.Add(suspended);
            ClearInvoice(false); // Successfully bypasses confirmation and updates ItemCountText
            UpdateTotals();
            MessageBox.Show("Đã tạm dừng đơn hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ResumeOrder(SuspendedOrder? order)
        {
            if (order == null) return;
            
            if (InvoiceItems.Count > 0)
            {
                var result = MessageBox.Show("Hóa đơn hiện tại có đồ. Bạn muốn ghép hay thay thế?", "Xác nhận", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.No) InvoiceItems.Clear();
            }

            foreach (var item in order.Items) InvoiceItems.Add(item);
            SelectedCustomer = order.Customer;
            InvoiceNote = order.Note ?? "";
            
            SuspendedOrders.Remove(order);
            IsPendingOrdersPopupOpen = false;
            RefreshItems();
            UpdateTotals();
        }

        private void EditItemNote(InvoiceItemViewModel? item)
        {
            if (item == null) return;
            // Simplified input
            string note = Microsoft.VisualBasic.Interaction.InputBox("Nhập ghi chú cho sản phẩm:", "Ghi chú món", item.Note);
            item.Note = note;
        }
        #endregion

        #region Order Management Logic
        private async System.Threading.Tasks.Task LoadInvoicesAsync(int? customerId = null)
        {
            try
            {
                // Optionally clear dates when searching for a specific customer history from the POS
                if (customerId.HasValue) SelectedOrderDate = null;

                var invoicesTask = _invoiceService.SearchInvoicesAsync(null, null, customerId, "");
                _allInvoices = (await invoicesTask).ToList();
                FilterOrders();
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi tải lịch sử đơn: {ex.Message}"); }
        }

        private void FilterOrders()
        {
            FilteredInvoices.Clear();
            var query = _allInvoices.AsEnumerable();
            
            if (SelectedOrderDate.HasValue)
            {
                query = query.Where(i => i.CreatedDate.Date == SelectedOrderDate.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(OrderSearchText))
            {
                query = query.Where(i => 
                    i.Id.ToString().Contains(OrderSearchText) || 
                    (i.CustomerName?.Contains(OrderSearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            foreach (var inv in query.OrderByDescending(i => i.CreatedDate)) FilteredInvoices.Add(inv);
        }

        private async Task LoadSelectedInvoiceDetailsSafeAsync(Invoice? invoice)
        {
            try
            {
                await LoadSelectedInvoiceDetailsAsync(invoice);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải chi tiết đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadSelectedInvoiceDetailsAsync(Invoice? invoice)
        {
            int requestVersion = ++_invoiceDetailsLoadVersion;
            SelectedInvoiceItems.Clear();
            if (invoice == null) return;
            
            var details = await _invoiceService.GetInvoiceDetailsAsync(invoice.Id);
            if (requestVersion != _invoiceDetailsLoadVersion) return; // Ignore stale responses.
            foreach (var item in details.Items ?? new List<InvoiceItem>()) SelectedInvoiceItems.Add(item);
        }

        private void ReprintSelectedInvoice()
        {
            if (SelectedInvoice == null) return;
            try 
            { 
               new InvoicePrintWindow(SelectedInvoice.Id, SelectedInvoice.EmployeeId).ShowDialog(); 
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi in lại hóa đơn: {ex.Message}"); }
        }

        private async System.Threading.Tasks.Task RefundSelectedInvoiceAsync()
        {
            if (SelectedInvoice == null) return;
            
            var res = MessageBox.Show($"Xác nhận HỦY và TRẢ HÀNG cho đơn #{SelectedInvoice.Id}?\nHành động này sẽ cộng lại tồn kho cho sản phẩm.", 
                                      "Xác nhận hoàn trả", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (res != MessageBoxResult.Yes) return;

            try
            {
                bool ok = await _invoiceService.RefundInvoiceAsync(SelectedInvoice.Id);
                if (!ok)
                {
                    MessageBox.Show("Không thể hoàn trả đơn. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Đã hủy đơn và hoàn trả kho thành công.", "Hoàn tất");
                await LoadInvoicesAsync();
                await ReloadProductsAsync();
                DashboardViewModel.TriggerDashboardRefresh();
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi xử lý hoàn trả: {ex.Message}"); }
        }

        private static string NormalizePaymentMethod(string? uiText)
        {
            if (string.IsNullOrWhiteSpace(uiText)) return "Cash";
            if (uiText.Contains("Chuyển khoản", StringComparison.OrdinalIgnoreCase)) return "BankTransfer";
            if (uiText.Contains("Thẻ", StringComparison.OrdinalIgnoreCase)) return "Card";
            return "Cash";
        }
        private void SelectProductByBarcode(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return;
            barcode = barcode.Trim();

            // 1. Search in local products list (Code)
            var product = _allProducts.FirstOrDefault(p => p.Code?.Trim() == barcode);
            if (product != null)
            {
                AddProductDirectly(product);
                BarcodeText = "";
                return;
            }

            // 2. Search variants in service
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                var variant = await _productService.GetVariantByBarcodeAsync(barcode);
                if (variant != null)
                {
                    // Find the base product for this variant in our list
                    var baseProduct = _allProducts.FirstOrDefault(p => p.Id == variant.ProductId);
                    if (baseProduct != null)
                    {
                        // Update UI on main thread
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            var listProduct = new ProductListItem
                            {
                                Id = baseProduct.Id,
                                Name = $"{baseProduct.Name} - {variant.Color} {variant.Size}",
                                UnitPrice = baseProduct.UnitPrice + variant.PriceAdjustment,
                                Code = variant.Barcode,
                                CategoryId = baseProduct.CategoryId,
                                CategoryTaxPercent = baseProduct.CategoryTaxPercent
                            };
                            AddProductDirectly(listProduct);
                            BarcodeText = "";
                        });
                        return;
                    }
                }
                
                // NOT FOUND
                System.Windows.Application.Current.Dispatcher.Invoke(() => 
                {
                    System.Windows.MessageBox.Show($"Không tìm thấy sản phẩm có mã vạch: {barcode}", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    BarcodeText = "";
                });
            });
        }
        private void OpenWebcamScanner()
        {
            var scannerWindow = new BarcodeScannerWindow();
            if (scannerWindow.ShowDialog() == true)
            {
                BarcodeText = scannerWindow.ScannedBarcode;
                SelectProductByBarcode(BarcodeText);
                
                // Play a simple beep if success
                try { System.Media.SystemSounds.Beep.Play(); } catch { }
            }
        }
        #endregion
    }

    public class InvoiceItemViewModel : BaseViewModel
    {
        private int _rowNumber;
        public int RowNumber { get => _rowNumber; set => SetProperty(ref _rowNumber, value); }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal PromoDiscountPercent { get; set; }
        public decimal UnitPrice { get; set; }

        private int _quantity;
        public int Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

        private decimal _lineTotal;
        public decimal LineTotal { get => _lineTotal; set => SetProperty(ref _lineTotal, value); }

        public decimal CategoryTaxPercent { get; set; }

        private string _note = "";
        public string Note { get => _note; set => SetProperty(ref _note, value); }

        public bool IsReward { get; set; } = false;
    }

    public class SuspendedOrder
    {
        public int Id { get; set; } = DateTime.Now.GetHashCode();
        public string Title { get; set; } = DateTime.Now.ToString("HH:mm:ss");
        public CustomerListItem? Customer { get; set; }
        public List<InvoiceItemViewModel> Items { get; set; } = new();
        public string? Note { get; set; }
        public decimal Total { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public class ProductListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal PromoDiscountPercent { get; set; }
        public DateTime? PromoStartDate { get; set; }
        public DateTime? PromoEndDate { get; set; }
        public int StockQuantity { get; set; }
        public decimal CategoryTaxPercent { get; set; }
        public int CategoryId { get; set; }
    }

    public class CustomerListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
#endregion
