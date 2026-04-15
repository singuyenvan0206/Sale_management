using FashionStore.App.ViewModels;
using System.Windows;

namespace FashionStore.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UsernameTextBox?.Focus();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (System.Windows.Input.FocusManager.GetFocusedElement(this) == UsernameTextBox)
                {
                    PasswordBox.Focus();
                }
                else if (DataContext is MainViewModel vm && vm.LoginCommand.CanExecute(PasswordBox))
                {
                    vm.LoginCommand.Execute(PasswordBox);
                }
                e.Handled = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb) tb.SelectAll();
        }
    }
}
