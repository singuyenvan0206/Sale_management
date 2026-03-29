using System.Windows;

namespace FashionStore
{
    using FashionStore.Services;
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string oldPassword = OldPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Validation
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Vui lĂ²ng nháº­p tĂªn Ä‘Äƒng nháº­p.", "ThĂ´ng bĂ¡o", MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                MessageBox.Show("Vui lĂ²ng nháº­p máº­t kháº©u hiá»‡n táº¡i.", "ThĂ´ng bĂ¡o", MessageBoxButton.OK, MessageBoxImage.Warning);
                OldPasswordBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Vui lĂ²ng nháº­p máº­t kháº©u má»›i.", "ThĂ´ng bĂ¡o", MessageBoxButton.OK, MessageBoxImage.Warning);
                NewPasswordBox.Focus();
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Máº­t kháº©u má»›i pháº£i cĂ³ Ă­t nháº¥t 6 kĂ½ tá»±.", "ThĂ´ng bĂ¡o", MessageBoxButton.OK, MessageBoxImage.Warning);
                NewPasswordBox.Focus();
                return;
            }

            if (oldPassword == newPassword)
            {
                MessageBox.Show("Máº­t kháº©u má»›i pháº£i khĂ¡c máº­t kháº©u hiá»‡n táº¡i.", "ThĂ´ng bĂ¡o", MessageBoxButton.OK, MessageBoxImage.Warning);
                NewPasswordBox.Focus();
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Máº­t kháº©u má»›i vĂ  xĂ¡c nháº­n máº­t kháº©u khĂ´ng khá»›p.", "ThĂ´ng bĂ¡o", MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Clear();
                ConfirmPasswordBox.Focus();
                return;
            }

            // Attempt to change password
            bool success = UserService.ChangePassword(username, oldPassword, newPassword);
            if (success)
            {
                MessageBox.Show("âœ… Máº­t kháº©u Ä‘Ă£ Ä‘Æ°á»£c thay Ä‘á»•i thĂ nh cĂ´ng!", "ThĂ nh cĂ´ng", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("âŒ TĂªn Ä‘Äƒng nháº­p hoáº·c máº­t kháº©u hiá»‡n táº¡i khĂ´ng Ä‘Ăºng.", "Lá»—i", MessageBoxButton.OK, MessageBoxImage.Error);
                OldPasswordBox.Clear();
                OldPasswordBox.Focus();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
