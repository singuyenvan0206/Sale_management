using System.Windows;
using System.Threading.Tasks;
using FashionStore.App.ViewModels;

namespace FashionStore.App.Views
{
    public partial class BankTransferWindow : Window
    {
        public bool IsPaid { get; private set; } = false;

        public BankTransferWindow(InvoiceManagementViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Listen for payment confirmation to close automatically
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.VerificationStatusText) && 
                    viewModel.VerificationStatusText.Contains("✅"))
                {
                    IsPaid = true;
                    // Give the user a moment to see the success message
                    Task.Delay(1500).ContinueWith(_ => Dispatcher.Invoke(() => DialogResult = true));
                }
            };
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
