using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FashionStore.Models;
using FashionStore.Services;

namespace FashionStore.ViewModels
{
    public class InvoiceManagementViewModel : BaseViewModel
    {
        private List<CustomerListItem> _allCustomerItems = new();
        private List<ProductListItem> _allProducts = new();
        
        public ObservableCollection<CustomerListItem> FilteredCustomers { get; } = new();
        public ObservableCollection<ProductListItem> Products { get; } = new();
        public ObservableCollection<InvoiceItemViewModel> InvoiceItems { get; } = new();
        public ObservableCollection<Voucher> Vouchers { get; } = new();

        public List<string> DiscountModes { get; } = new() { "VND", "%" };
        public List<string> PaymentMethods { get; } = new() { "💵 Tiền mặt", "💳 Thẻ", "🏦 Chuyển khoản" };

        private bool _isInternalUpdate = false;

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
                    OnCustomerSelected();
                }
            }
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

        #endregion

        #region Commands

        public ICommand SelectCustomerCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand IncreaseQtyCommand { get; }
        public ICommand DecreaseQtyCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ClearInvoiceCommand { get; }
        public ICommand SaveInvoiceCommand { get; }
        public ICommand OpenHistoryCommand { get; }

        #endregion

        public InvoiceManagementViewModel()
        {
            SelectCustomerCommand = new RelayCommand(p =>
            {
                if (p is CustomerListItem customer) SelectCustomer(customer);
            });
            AddItemCommand = new RelayCommand(_ => AddItem());
            IncreaseQtyCommand = new RelayCommand(p => IncreaseQty(p as InvoiceItemViewModel));
            DecreaseQtyCommand = new RelayCommand(p => DecreaseQty(p as InvoiceItemViewModel));
            RemoveItemCommand = new RelayCommand(p => RemoveItem(p as InvoiceItemViewModel));
            ClearInvoiceCommand = new RelayCommand(_ => { ClearInvoice(); UpdateTotals(); });
            SaveInvoiceCommand = new RelayCommand(_ => SaveInvoice());
            OpenHistoryCommand = new RelayCommand(_ => OpenHistory());

            InitializeData();
        }

        private void InitializeData()
        {
            LoadCustomers();
            LoadProducts();
            LoadVouchers();
            ClearInvoice();
            try { PaymentSettingsManager.Load(); } catch { }
            UpdateTotals();
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
            catch { }
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
        private void LoadProducts()
        {
            try
            {
                var products = ProductService.GetAllProductsWithCategories();
                var list = products.ConvertAll(p => new ProductListItem
                {
                    Id = p.Id,
                    Name = string.IsNullOrWhiteSpace(p.Code) ? p.Name : $"{p.Name} ({p.Code})",
                    UnitPrice = p.SalePrice,
                    PromoDiscountPercent = p.PromoDiscountPercent,
                    PromoStartDate = p.PromoStartDate,
                    PromoEndDate = p.PromoEndDate,
                    StockQuantity = p.StockQuantity,
                    CategoryTaxPercent = p.CategoryTaxPercent
                });

                Products.Clear();
                foreach (var p in list) Products.Add(p);
            }
            catch { }
        }

        private void OnProductSelected()
        {
            if (SelectedProduct != null)
            {
                var promoPercent = GetActivePromoPercent(SelectedProduct);
                var discounted = ApplyPercentDiscount(SelectedProduct.UnitPrice, promoPercent);
                UnitPriceText = discounted.ToString("F2");
                if (string.IsNullOrWhiteSpace(QuantityText)) QuantityText = "1";
            }
            UpdateTotals();
        }
        
        private decimal GetActivePromoPercent(ProductListItem product)
        {
            if (product.PromoDiscountPercent <= 0) return 0m;
            var now = DateTime.Now;
            if (product.PromoStartDate.HasValue && now < product.PromoStartDate.Value) return 0m;
            if (product.PromoEndDate.HasValue && now > product.PromoEndDate.Value) return 0m;
            return product.PromoDiscountPercent;
        }

        private decimal ApplyPercentDiscount(decimal price, decimal percent)
        {
            if (percent <= 0) return price;
            if (percent >= 100) return 0m;
            return Math.Round(price * (1 - (percent / 100m)), 2);
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

            decimal unitPrice = decimal.TryParse(UnitPriceText, out var p) && p >= 0 ? p : ApplyPercentDiscount(SelectedProduct.UnitPrice, GetActivePromoPercent(SelectedProduct));

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
                    PromoDiscountPercent = GetActivePromoPercent(SelectedProduct),
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
            foreach(var item in items) InvoiceItems.Add(item);

            ItemCountText = $"{InvoiceItems.Count} mục";
        }

        private void ClearInvoice()
        {
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
            catch { }
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

            try
            {
                var subtotal = InvoiceItems.Sum(item => item.LineTotal);
                SubtotalText = subtotal.ToString("N0") + "₫";

                ApplyBestVoucher(subtotal);

                var discountVal = decimal.TryParse(DiscountValueText, out var dv) ? dv : 0m;
                var discount = SelectedDiscountMode == "%" ? Math.Round(subtotal * (discountVal / 100m), 2) : discountVal;

                var (tier, _) = SelectedCustomer != null ? CustomerService.GetCustomerLoyalty(SelectedCustomer.Id) : ("Regular", 0);
                var tierDiscountPercent = TierSettingsManager.GetTierDiscount(tier);
                var tierDiscount = Math.Round(subtotal * (tierDiscountPercent / 100m), 2);

                TierDiscountInlineText = $"(+ Ưu đãi hạng: {tierDiscount:N0}₫)";

                var totalDiscount = discount + tierDiscount;
                if (SelectedVoucher != null && (SelectedVoucher.DiscountType == Voucher.TypePercentage || SelectedVoucher.DiscountType == "%") && SelectedVoucher.MaxDiscountAmount > 0)
                {
                    totalDiscount = Math.Min(totalDiscount, SelectedVoucher.MaxDiscountAmount + tierDiscount);
                }

                decimal discountRatio = subtotal > 0 ? totalDiscount / subtotal : 0;
                var taxAmount = InvoiceItems.Sum(item => (item.LineTotal * (1 - discountRatio)) * (item.CategoryTaxPercent / 100m));
                
                var total = Math.Max(0, subtotal + taxAmount - totalDiscount);

                TaxAmountText = taxAmount.ToString("N0") + "₫";
                TotalText = total.ToString("N0") + "₫";

                var paid = decimal.TryParse(PaidText, out var p) ? p : 0m;
                var change = Math.Max(0, paid - total);
                ChangeText = change.ToString("N0") + "₫";

                UpdateQRCode(total);
            }
            catch { }
        }

        private void ApplyBestVoucher(decimal subtotal)
        {
            if (SelectedCustomer == null || Vouchers.Count == 0 || _isInternalUpdate) return;
            var bestVoucher = Vouchers
                .Where(v => v.IsValid(subtotal, VoucherService.GetVoucherUsageCountForCustomer(v.Id, SelectedCustomer.Id)))
                .OrderByDescending(v => {
                    if (v.DiscountType == Voucher.TypePercentage || v.DiscountType == "%")
                    {
                        var val = subtotal * (v.DiscountValue / 100m);
                        if (v.MaxDiscountAmount > 0) val = Math.Min(val, v.MaxDiscountAmount);
                        return val;
                    }
                    return v.DiscountValue;
                })
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

                var description = "INV" + Regex.Replace(DateTime.Now.ToString("yyMMdd"), @"[^a-zA-Z0-9]", "");
                if (description.Length > 8) description = description.Substring(0, 8);

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
        private void SaveInvoice()
        {
            if (InvoiceItems.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một sản phẩm.", "Xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedCustomer == null)
            {
                MessageBox.Show("Vui lòng chọn khách hàng.", "Xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var subtotal = InvoiceItems.Sum(item => item.LineTotal);
                
                var discountVal = decimal.TryParse(DiscountValueText, out var dv) ? dv : 0m;
                var manualDiscount = SelectedDiscountMode == "%" ? Math.Round(subtotal * (discountVal / 100m), 2) : discountVal;
                var (tier, currentPoints) = CustomerService.GetCustomerLoyalty(SelectedCustomer.Id);
                var tierDiscountPercent = TierSettingsManager.GetTierDiscount(tier);
                var tierDiscount = Math.Round(subtotal * (tierDiscountPercent / 100m), 2);
                var discount = manualDiscount + tierDiscount;

                var taxAmount = InvoiceItems.Sum(item => item.LineTotal * (item.CategoryTaxPercent / 100m));
                var total = Math.Max(0, subtotal + taxAmount - discount);
                var paid = decimal.TryParse(PaidText, out var p) ? p : 0m;

                var itemsForSave = InvoiceItems.Select(item => (item.ProductId, item.Quantity, item.UnitPrice)).ToList();

                var currentUser = Application.Current.Resources["CurrentUser"]?.ToString() ?? "admin";
                var employeeId = 1;
                try { employeeId = UserService.GetEmployeeIdByUsername(currentUser); } catch {}
                
                var result = InvoiceService.SaveInvoice(SelectedCustomer.Id, employeeId, subtotal, 0, taxAmount, discount, total, paid, itemsForSave, voucherId: SelectedVoucher?.Id);

                if (!result)
                {
                    MessageBox.Show("Không thể lưu hóa đơn.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var invoiceId = InvoiceService.LastSavedInvoiceId;
                MessageBox.Show($"Hóa đơn #{invoiceId} đã được lưu.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Update Loyalty
                var earnedPoints = (int)Math.Floor((double)total / 100000);
                var newPoints = currentPoints + earnedPoints;
                var newTier = TierSettingsManager.DetermineTierByPoints(newPoints);
                CustomerService.UpdateCustomerLoyalty(SelectedCustomer.Id, newPoints, newTier);

                ClearInvoice();
                DiscountValueText = "0";
                PaidText = "0";
                UpdateTotals();

                // Show print window
                try
                {
                    var printWindow = new InvoicePrintWindow(invoiceId, employeeId);
                    printWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi hiển thị cửa sổ in: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                DashboardViewModel.TriggerDashboardRefresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenHistory()
        {
            var historyWindow = new TransactionHistoryWindow();
            historyWindow.ShowDialog();
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
    }

    public class ProductListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal PromoDiscountPercent { get; set; }
        public DateTime? PromoStartDate { get; set; }
        public DateTime? PromoEndDate { get; set; }
        public int StockQuantity { get; set; }
        public decimal CategoryTaxPercent { get; set; }
    }

    public class CustomerListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
