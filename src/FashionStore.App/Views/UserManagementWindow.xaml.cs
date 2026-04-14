using FashionStore.App.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace FashionStore.App.Views
{
    public partial class UserManagementWindow : Window
    {
        public UserManagementWindow()
        {
            InitializeComponent();
            DataContext = new UserManagementViewModel();
        }

        private void CloseWindow_Click(object sender, System.Windows.RoutedEventArgs e) => Close();

        private void UsersDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is UserManagementViewModel vm && vm.SelectedUser != null)
            {
                vm.EditUserCommand.Execute(vm.SelectedUser);
            }
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Fix: Cho phép ComboBox và các control khác tự xử lý cuộn chuột
            if (e.OriginalSource is DependencyObject source)
            {
                if (IsInsideControl<System.Windows.Controls.ComboBox>(source)) return;
                if (IsInsideControl<System.Windows.Controls.ListBox>(source)) return;
            }

            // Scroll bubbling
            if (e.Source is DependencyObject obj)
            {
                var scrollViewer = GetScrollViewer(obj);
                if (scrollViewer != null)
                {
                    double newOffset = scrollViewer.VerticalOffset - e.Delta;
                    if (newOffset <= 0 && e.Delta > 0) return;
                    if (newOffset >= scrollViewer.ScrollableHeight && e.Delta < 0) return;

                    scrollViewer.ScrollToVerticalOffset(newOffset);
                    e.Handled = true;
                }
            }
        }

        private bool IsInsideControl<T>(DependencyObject obj) where T : DependencyObject
        {
            DependencyObject? parent = obj;
            while (parent != null)
            {
                if (parent is T) return true;
                parent = (parent is System.Windows.Media.Visual || parent is System.Windows.Media.Media3D.Visual3D)
                    ? System.Windows.Media.VisualTreeHelper.GetParent(parent)
                    : null;
            }
            return false;
        }

        private System.Windows.Controls.ScrollViewer? GetScrollViewer(DependencyObject obj)
        {
            if (obj is System.Windows.Controls.ScrollViewer sv) return sv;
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
