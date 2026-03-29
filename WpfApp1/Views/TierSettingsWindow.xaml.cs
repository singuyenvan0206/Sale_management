using System.Windows;
using FashionStore.ViewModels;

namespace FashionStore
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
