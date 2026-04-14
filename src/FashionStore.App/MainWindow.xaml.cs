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
                if (DataContext is MainViewModel vm && vm.LoginCommand.CanExecute(PasswordBox))
                {
                    vm.LoginCommand.Execute(PasswordBox);
                }
                e.Handled = true;
            }
        }
    }
}
