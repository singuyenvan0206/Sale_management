using System.Windows;
using System.Windows.Input;
using FashionStore.Services;

namespace FashionStore.ViewModels
{
    public class TierSettingsViewModel : BaseViewModel
    {
        #region Regular Tier
        private string _regularMinPoints = "0"; public string RegularMinPoints { get => _regularMinPoints; set => SetProperty(ref _regularMinPoints, value); }
        private string _regularDiscount = "0"; public string RegularDiscount { get => _regularDiscount; set => SetProperty(ref _regularDiscount, value); }
        private string _regularBenefits = ""; public string RegularBenefits { get => _regularBenefits; set => SetProperty(ref _regularBenefits, value); }
        private string _regularDescription = ""; public string RegularDescription { get => _regularDescription; set => SetProperty(ref _regularDescription, value); }
        #endregion

        #region Silver Tier
        private string _silverMinPoints = "500"; public string SilverMinPoints { get => _silverMinPoints; set => SetProperty(ref _silverMinPoints, value); }
        private string _silverDiscount = "3"; public string SilverDiscount { get => _silverDiscount; set => SetProperty(ref _silverDiscount, value); }
        private string _silverBenefits = ""; public string SilverBenefits { get => _silverBenefits; set => SetProperty(ref _silverBenefits, value); }
        private string _silverDescription = ""; public string SilverDescription { get => _silverDescription; set => SetProperty(ref _silverDescription, value); }
        #endregion

        #region Gold Tier
        private string _goldMinPoints = "1000"; public string GoldMinPoints { get => _goldMinPoints; set => SetProperty(ref _goldMinPoints, value); }
        private string _goldDiscount = "7"; public string GoldDiscount { get => _goldDiscount; set => SetProperty(ref _goldDiscount, value); }
        private string _goldBenefits = ""; public string GoldBenefits { get => _goldBenefits; set => SetProperty(ref _goldBenefits, value); }
        private string _goldDescription = ""; public string GoldDescription { get => _goldDescription; set => SetProperty(ref _goldDescription, value); }
        #endregion

        #region Platinum Tier
        private string _platinumMinPoints = "2000"; public string PlatinumMinPoints { get => _platinumMinPoints; set => SetProperty(ref _platinumMinPoints, value); }
        private string _platinumDiscount = "10"; public string PlatinumDiscount { get => _platinumDiscount; set => SetProperty(ref _platinumDiscount, value); }
        private string _platinumBenefits = ""; public string PlatinumBenefits { get => _platinumBenefits; set => SetProperty(ref _platinumBenefits, value); }
        private string _platinumDescription = ""; public string PlatinumDescription { get => _platinumDescription; set => SetProperty(ref _platinumDescription, value); }
        #endregion

        private bool _autoUpdateTiers = false;
        public bool AutoUpdateTiers { get => _autoUpdateTiers; set => SetProperty(ref _autoUpdateTiers, value); }

        public ICommand SaveCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand CloseCommand { get; }

        private readonly Window _owner;

        public TierSettingsViewModel(Window owner)
        {
            _owner = owner;
            SaveCommand = new RelayCommand(_ => Save());
            ResetCommand = new RelayCommand(_ => ResetToDefaults());
            CloseCommand = new RelayCommand(_ => _owner.Close());
            Load();
        }

        private void Load()
        {
            try
            {
                var s = TierSettingsManager.Load();
                RegularMinPoints = s.RegularMinPoints.ToString();
                RegularDiscount = s.RegularDiscountPercent.ToString();
                RegularBenefits = s.RegularBenefits;
                RegularDescription = s.RegularDescription;

                SilverMinPoints = s.SilverMinPoints.ToString();
                SilverDiscount = s.SilverDiscountPercent.ToString();
                SilverBenefits = s.SilverBenefits;
                SilverDescription = s.SilverDescription;

                GoldMinPoints = s.GoldMinPoints.ToString();
                GoldDiscount = s.GoldDiscountPercent.ToString();
                GoldBenefits = s.GoldBenefits;
                GoldDescription = s.GoldDescription;

                PlatinumMinPoints = s.PlatinumMinPoints.ToString();
                PlatinumDiscount = s.PlatinumDiscountPercent.ToString();
                PlatinumBenefits = s.PlatinumBenefits;
                PlatinumDescription = s.PlatinumDescription;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải cài đặt: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save()
        {
            var settings = new TierSettings
            {
                RegularMinPoints = int.TryParse(RegularMinPoints, out int rMin) ? rMin : 0,
                RegularDiscountPercent = decimal.TryParse(RegularDiscount, out decimal rDisc) ? rDisc : 0,
                RegularBenefits = RegularBenefits.Trim(),
                RegularDescription = RegularDescription.Trim(),

                SilverMinPoints = int.TryParse(SilverMinPoints, out int sMin) ? sMin : 500,
                SilverDiscountPercent = decimal.TryParse(SilverDiscount, out decimal sDisc) ? sDisc : 3,
                SilverBenefits = SilverBenefits.Trim(),
                SilverDescription = SilverDescription.Trim(),

                GoldMinPoints = int.TryParse(GoldMinPoints, out int gMin) ? gMin : 1000,
                GoldDiscountPercent = decimal.TryParse(GoldDiscount, out decimal gDisc) ? gDisc : 7,
                GoldBenefits = GoldBenefits.Trim(),
                GoldDescription = GoldDescription.Trim(),

                PlatinumMinPoints = int.TryParse(PlatinumMinPoints, out int pMin) ? pMin : 2000,
                PlatinumDiscountPercent = decimal.TryParse(PlatinumDiscount, out decimal pDisc) ? pDisc : 10,
                PlatinumBenefits = PlatinumBenefits.Trim(),
                PlatinumDescription = PlatinumDescription.Trim()
            };

            if (!ValidateSettings(settings)) return;

            if (TierSettingsManager.Save(settings))
            {
                string message = "Cài đặt hạng thành viên đã được lưu thành công!";
                if (AutoUpdateTiers)
                {
                    int updated = TierSettingsManager.UpdateAllCustomerTiers();
                    message += updated > 0 ? $"\n\n🔄 Đã tự động cập nhật hạng cho {updated} khách hàng." : "\n\n✅ Không có khách hàng nào cần cập nhật hạng.";
                }
                else message += "\n\n📝 Lưu ý: Hạng khách hàng chưa được cập nhật tự động.";

                MessageBox.Show(message, "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                DashboardViewModel.TriggerDashboardRefresh();
                _owner.Close();
            }
            else MessageBox.Show("Không thể lưu cài đặt. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool ValidateSettings(TierSettings s)
        {
            if (s.RegularDiscountPercent < 0 || s.RegularDiscountPercent > 100 ||
                s.SilverDiscountPercent < 0 || s.SilverDiscountPercent > 100 ||
                s.GoldDiscountPercent < 0 || s.GoldDiscountPercent > 100 ||
                s.PlatinumDiscountPercent < 0 || s.PlatinumDiscountPercent > 100)
            {
                MessageBox.Show("Phần trăm giảm giá phải từ 0 đến 100.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (s.SilverMinPoints <= s.RegularMinPoints || s.GoldMinPoints <= s.SilverMinPoints || s.PlatinumMinPoints <= s.GoldMinPoints)
            {
                MessageBox.Show("Điểm tối thiểu của các hạng phải tăng dần: Regular < Silver < Gold < Platinum.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (s.SilverDiscountPercent < s.RegularDiscountPercent || s.GoldDiscountPercent < s.SilverDiscountPercent || s.PlatinumDiscountPercent < s.GoldDiscountPercent)
            {
                MessageBox.Show("Phần trăm giảm giá của các hạng phải tăng dần: Regular ≤ Silver ≤ Gold ≤ Platinum.", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void ResetToDefaults()
        {
            var r = MessageBox.Show("Bạn có chắc chắn muốn khôi phục cài đặt mặc định?\nTất cả thay đổi hiện tại sẽ bị mất.", "Xác nhận khôi phục", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes)
            {
                var def = new TierSettings();
                if (TierSettingsManager.Save(def)) { Load(); MessageBox.Show("Đã khôi phục cài đặt mặc định.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information); DashboardViewModel.TriggerDashboardRefresh(); }
                else MessageBox.Show("Không thể khôi phục cài đặt mặc định.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
