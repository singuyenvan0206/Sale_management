using FashionStore.App.Core;
using FashionStore.Core.Models;
using FashionStore.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
namespace FashionStore.App.ViewModels
{
    public class PromotionViewModel : BaseViewModel
    {
        public ObservableCollection<Promotion> Promotions { get; set; } = new();
        public ObservableCollection<Product> Products { get; set; } = new();
        public ObservableCollection<CategoryViewModel> Categories { get; set; } = new();

        private Promotion? _selectedPromotion;
        public Promotion? SelectedPromotion
        {
            get => _selectedPromotion;
            set
            {
                if (SetProperty(ref _selectedPromotion, value) && value != null)
                {
                    LoadPromotionDetails();
                }
            }
        }

        // Form fields
        private string _name = string.Empty;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private string _type = Promotion.TypeFlashSale;
        public string Type { get => _type; set { SetProperty(ref _type, value); OnPropertyChanged(nameof(IsFlashSale)); OnPropertyChanged(nameof(IsBOGO)); } }

        public bool IsFlashSale => Type == Promotion.TypeFlashSale;
        public bool IsBOGO => Type == Promotion.TypeBOGO;

        private DateTime _startDate = DateTime.Today;
        public DateTime StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }

        private DateTime _endDate = DateTime.Today.AddDays(7);
        public DateTime EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        private decimal _discountPercent;
        public decimal DiscountPercent { get => _discountPercent; set => SetProperty(ref _discountPercent, value); }

        private decimal _discountAmount;
        public decimal DiscountAmount { get => _discountAmount; set => SetProperty(ref _discountAmount, value); }

        private Product? _requiredProduct;
        public Product? RequiredProduct { get => _requiredProduct; set => SetProperty(ref _requiredProduct, value); }

        private int _requiredQuantity;
        public int RequiredQuantity { get => _requiredQuantity; set => SetProperty(ref _requiredQuantity, value); }

        private Product? _rewardProduct;
        public Product? RewardProduct { get => _rewardProduct; set => SetProperty(ref _rewardProduct, value); }

        private int _rewardQuantity;
        public int RewardQuantity { get => _rewardQuantity; set => SetProperty(ref _rewardQuantity, value); }

        private CategoryViewModel? _targetCategory;
        public CategoryViewModel? TargetCategory { get => _targetCategory; set => SetProperty(ref _targetCategory, value); }

        private bool _isActive = true;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

        public List<string> PromotionTypes { get; } = new() { Promotion.TypeFlashSale, Promotion.TypeBOGO, Promotion.TypeCombo };

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public PromotionViewModel()
        {
            SaveCommand = new RelayCommand(SavePromotion);
            DeleteCommand = new RelayCommand(DeletePromotion, CanDelete);
            ClearCommand = new RelayCommand(ClearForm);

            LoadData();
        }

        private void LoadData()
        {
            var productsList = ProductService.GetAllProductsWithCategories();
            Products.Clear();
            foreach (var p in productsList)
            {
                Products.Add(new Product
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code
                });
            }

            var cats = CategoryService.GetAllCategories();
            Categories.Clear();
            foreach (var c in cats)
            {
                Categories.Add(new CategoryViewModel { Id = c.Id, Name = c.Name });
            }

            LoadPromotions();
        }

        private void LoadPromotions()
        {
            var pList = PromotionService.GetAllPromotions();
            Promotions.Clear();
            foreach (var p in pList) Promotions.Add(p);
        }

        private void LoadPromotionDetails()
        {
            if (SelectedPromotion == null) return;
            Name = SelectedPromotion.Name;
            Type = SelectedPromotion.Type;
            StartDate = SelectedPromotion.StartDate;
            EndDate = SelectedPromotion.EndDate;
            DiscountPercent = SelectedPromotion.DiscountPercent;
            DiscountAmount = SelectedPromotion.DiscountAmount;
            RequiredQuantity = SelectedPromotion.RequiredQuantity;
            RewardQuantity = SelectedPromotion.RewardQuantity;
            IsActive = SelectedPromotion.IsActive;

            TargetCategory = Categories.FirstOrDefault(c => c.Id == SelectedPromotion.TargetCategoryId);

            RequiredProduct = Products.FirstOrDefault(p => p.Id == SelectedPromotion.RequiredProductId);
            RewardProduct = Products.FirstOrDefault(p => p.Id == SelectedPromotion.RewardProductId);
        }

        private void ClearForm(object? obj = null)
        {
            SelectedPromotion = null;
            Name = string.Empty;
            Type = Promotion.TypeFlashSale;
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(7);
            DiscountPercent = 0;
            DiscountAmount = 0;
            RequiredProduct = null;
            RequiredQuantity = 0;
            RewardProduct = null;
            RewardQuantity = 0;
            TargetCategory = null;
            IsActive = true;
        }

        private void SavePromotion(object? obj)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Vui lòng nhập tên chương trình.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var p = SelectedPromotion ?? new Promotion();
            p.Name = Name;
            p.Type = Type;
            p.StartDate = StartDate;
            p.EndDate = EndDate;
            p.DiscountPercent = IsFlashSale ? DiscountPercent : 0;
            p.DiscountAmount = IsFlashSale ? DiscountAmount : 0;
            p.RequiredProductId = (IsBOGO || IsFlashSale) ? RequiredProduct?.Id : null;
            p.RequiredQuantity = IsBOGO ? RequiredQuantity : 0;
            p.RewardProductId = IsBOGO ? RewardProduct?.Id : null;
            p.RewardQuantity = IsBOGO ? RewardQuantity : 0;
            p.TargetCategoryId = IsFlashSale ? TargetCategory?.Id : null;
            p.IsActive = IsActive;

            bool success;
            if (p.Id == 0)
                success = PromotionService.AddPromotion(p);
            else
                success = PromotionService.UpdatePromotion(p);

            if (success)
            {
                MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                LoadPromotions();
            }
            else
            {
                MessageBox.Show("Lỗi khi lưu chương trình khuyến mãi.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDelete(object? obj) => SelectedPromotion != null;

        private void DeletePromotion(object? obj)
        {
            if (SelectedPromotion == null) return;
            if (MessageBox.Show($"Bạn có chắc muốn xoá chương trình '{SelectedPromotion.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (PromotionService.DeletePromotion(SelectedPromotion.Id))
                {
                    MessageBox.Show("Xoá thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    LoadPromotions();
                }
                else
                {
                    MessageBox.Show("Xoá thất bại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
