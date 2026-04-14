using FashionStore.App.ViewModels;
using System.Windows;

namespace FashionStore.App.Views
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
