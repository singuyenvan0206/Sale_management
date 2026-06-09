using ShopManager.App.ViewModels;
using System.Windows;

namespace ShopManager.App.Views
{
    public partial class TierSettingsWindow : Window
    {
        public TierSettingsWindow()
        {
            InitializeComponent();
            DataContext = new TierSettingsViewModel(this);
        }
    }
}
