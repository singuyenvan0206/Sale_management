using System.Windows;

namespace WpfApp1
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string oldPassword = OldPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            bool success = DatabaseHelper.ChangePassword(username, oldPassword, newPassword);
            if (success)
            {
                MessageBox.Show("Password changed successfully.");
                this.Close();
            }
            else
            {
                MessageBox.Show("Incorrect username or old password.");
            }
        }
    }
}