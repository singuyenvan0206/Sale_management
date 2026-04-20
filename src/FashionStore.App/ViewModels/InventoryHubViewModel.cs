using FashionStore.App.Core;
using FashionStore.Core.Models;
using FashionStore.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FashionStore.App.ViewModels
{
    public class InventoryHubViewModel : BaseViewModel
    {
        // Tab control
        private int _selectedTab;
        public int SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        // PO List
        public ObservableCollection<PurchaseOrder> PurchaseOrders { get; set; } = new();

        // Stock movements
        public ObservableCollection<StockMovement> StockMovements { get; set; } = new();

        // New PO form
        public ObservableCollection<Supplier> Suppliers { get; set; } = new();
        public ObservableCollection<Product> Products { get; set; } = new();
        public ObservableCollection<PurchaseOrderItem> NewPOItems { get; set; } = new();

        private Supplier? _selectedSupplier;
        public Supplier? SelectedSupplier
        {
            get => _selectedSupplier;
            set => SetProperty(ref _selectedSupplier, value);
        }

        private string _poNotes = string.Empty;
        public string PONotes
        {
            get => _poNotes;
            set => SetProperty(ref _poNotes, value);
        }

        private PurchaseOrder? _selectedPO;
        public PurchaseOrder? SelectedPO
        {
            get => _selectedPO;
            set
            {
                if (SetProperty(ref _selectedPO, value) && value != null)
                {
                    LoadPODetails(value.Id);
                }
            }
        }

        private PurchaseOrder? _selectedPODetails;
        public PurchaseOrder? SelectedPODetails
        {
            get => _selectedPODetails;
            set => SetProperty(ref _selectedPODetails, value);
        }

        // New item form - product search
        private Product? _newItemProduct;
        public Product? NewItemProduct
        {
            get => _newItemProduct;
            set => SetProperty(ref _newItemProduct, value);
        }

        private string _productSearchText = string.Empty;
        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                if (SetProperty(ref _productSearchText, value))
                {
                    FilterProducts();
                }
            }
        }

        public ObservableCollection<Product> FilteredProducts { get; set; } = new();

        private bool _isDropdownOpen;
        public bool IsDropdownOpen
        {
            get => _isDropdownOpen;
            set => SetProperty(ref _isDropdownOpen, value);
        }

        private void FilterProducts()
        {
            var term = ProductSearchText.Trim().ToLower();
            FilteredProducts.Clear();
            if (string.IsNullOrWhiteSpace(term))
            {
                IsDropdownOpen = false;
                return;
            }
            foreach (var p in Products)
            {
                if ((p.Name?.ToLower().Contains(term) ?? false) || (p.Code?.ToLower().Contains(term) ?? false))
                    FilteredProducts.Add(p);
            }
            IsDropdownOpen = FilteredProducts.Count > 0;
        }

        private int _newItemQty = 1;
        public int NewItemQty
        {
            get => _newItemQty;
            set => SetProperty(ref _newItemQty, value);
        }

        private decimal _newItemPrice;
        public decimal NewItemPrice
        {
            get => _newItemPrice;
            set => SetProperty(ref _newItemPrice, value);
        }

        private string _totalPOAmountText = "0 đ";
        public string TotalPOAmountText
        {
            get => _totalPOAmountText;
            set => SetProperty(ref _totalPOAmountText, value);
        }

        public ICommand CreatePOCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand ReceivePOCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SelectProductCommand { get; }

        public InventoryHubViewModel()
        {
            CreatePOCommand = new RelayCommand(CreatePO, _ => SelectedSupplier != null && NewPOItems.Count > 0);
            AddItemCommand = new RelayCommand(AddItem, _ => NewItemProduct != null && NewItemQty > 0);
            RemoveItemCommand = new RelayCommand(RemoveItem);
            ReceivePOCommand = new RelayCommand(ReceivePO, _ => SelectedPO != null && SelectedPO.Status != "Received");
            RefreshCommand = new RelayCommand(_ => LoadData());
            SelectProductCommand = new RelayCommand(obj =>
            {
                if (obj is Product p)
                {
                    NewItemProduct = p;
                    ProductSearchText = $"{p.Name} ({p.Code})";
                    IsDropdownOpen = false;
                }
            });
            NewPOItems.CollectionChanged += (s, e) => UpdateTotal();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var suppliers = SupplierService.GetAllSuppliers();
                Suppliers.Clear();
                foreach (var s in suppliers) Suppliers.Add(s);

                var products = ProductService.GetAllProductsWithCategories();
                Products.Clear();
                foreach (var p in products)
                {
                    Products.Add(new Product { Id = p.Id, Name = p.Name, Code = p.Code, PurchasePrice = p.PurchasePrice });
                }

                LoadPOList();
                LoadStockHistory();
            }
            catch (Exception ex)
            {
                AppLogger.Log(ex, "InventoryHubViewModel.LoadData");
            }
        }

        private void LoadPOList()
        {
            try
            {
                var pos = PurchaseOrderService.GetAllOrders();
                PurchaseOrders.Clear();
                foreach (var po in pos) PurchaseOrders.Add(po);
            }
            catch (Exception ex)
            {
                AppLogger.Log(ex, "InventoryHubViewModel.LoadPOList");
            }
        }

        private void LoadStockHistory()
        {
            try
            {
                var movements = StockMovementService.GetLatestMovements(100);
                StockMovements.Clear();
                foreach (var m in movements) StockMovements.Add(m);
            }
            catch (Exception ex)
            {
                AppLogger.Log(ex, "InventoryHubViewModel.LoadStockHistory");
            }
        }

        private void LoadPODetails(int poId)
        {
            try
            {
                SelectedPODetails = PurchaseOrderService.GetOrderById(poId);
            }
            catch (Exception ex)
            {
                AppLogger.Log(ex, "InventoryHubViewModel.LoadPODetails");
            }
        }

        private void AddItem(object? obj)
        {
            if (NewItemProduct == null) return;
            NewPOItems.Add(new PurchaseOrderItem
            {
                ProductId = NewItemProduct.Id,
                ProductName = NewItemProduct.Name,
                ProductCode = NewItemProduct.Code,
                Quantity = NewItemQty,
                UnitPrice = NewItemPrice > 0 ? NewItemPrice : NewItemProduct.PurchasePrice
            });
            NewItemProduct = null;
            ProductSearchText = string.Empty;
            FilteredProducts.Clear();
            IsDropdownOpen = false;
            NewItemQty = 1;
            NewItemPrice = 0;
            UpdateTotal();
        }

        private void RemoveItem(object? obj)
        {
            if (obj is PurchaseOrderItem item) NewPOItems.Remove(item);
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            var total = NewPOItems.Sum(i => i.Quantity * i.UnitPrice);
            TotalPOAmountText = total.ToString("C0");
        }

        private void CreatePO(object? obj)
        {
            if (SelectedSupplier == null || !NewPOItems.Any()) return;

            var username = Application.Current.Resources["CurrentUser"] as string ?? "";
            var employeeId = UserService.GetEmployeeIdByUsername(username);

            var po = new PurchaseOrder
            {
                SupplierId = SelectedSupplier.Id,
                EmployeeId = employeeId > 0 ? employeeId : 1,
                TotalAmount = NewPOItems.Sum(i => i.Quantity * i.UnitPrice),
                PaidAmount = 0,
                Status = "Pending",
                Notes = PONotes,
                Items = NewPOItems.ToList()
            };

            var id = PurchaseOrderService.CreateOrder(po);
            if (id > 0)
            {
                NewPOItems.Clear();
                PONotes = string.Empty;
                SelectedSupplier = null;
                UpdateTotal();
                LoadPOList();
                SelectedTab = 1; // Switch to PO list tab
                MessageBox.Show("✅ Đã tạo đơn nhập hàng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("❌ Không thể tạo đơn. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReceivePO(object? obj)
        {
            if (SelectedPO == null) return;
            var result = MessageBox.Show($"Xác nhận nhận hàng cho đơn #PO-{SelectedPO.Id:D5}?\nThao tác này sẽ cập nhật tồn kho.", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            var success = PurchaseOrderService.ReceiveOrder(SelectedPO.Id);
            if (success)
            {
                LoadPOList();
                LoadStockHistory();
                SelectedPO = null;
                SelectedPODetails = null;
                MessageBox.Show("✅ Đã xác nhận nhận hàng và cập nhật kho!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("❌ Thao tác thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
