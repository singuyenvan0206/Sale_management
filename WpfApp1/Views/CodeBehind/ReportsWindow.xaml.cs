using System.Windows;
using System.Windows.Input;
using FashionStore.ViewModels;

namespace FashionStore
{
    public partial class ReportsWindow : Window
    {
        public ReportsWindow()
        {
            InitializeComponent();
            DataContext = new ReportsViewModel();
            
            // Keep DataGrid sorting wired up via code-behind for UI column indicator updates
            InvoicesDataGrid.Sorting += InvoicesDataGrid_Sorting;
        }

        private void InvoicesDataGrid_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            e.Handled = true;
            var propertyName = e.Column.SortMemberPath;
            if (string.IsNullOrEmpty(propertyName)) return;

            var direction = e.Column.SortDirection != System.ComponentModel.ListSortDirection.Ascending
                ? System.ComponentModel.ListSortDirection.Ascending
                : System.ComponentModel.ListSortDirection.Descending;

            if (DataContext is ReportsViewModel vm)
                vm.ApplySort(propertyName);

            e.Column.SortDirection = direction;
            foreach (var col in InvoicesDataGrid.Columns)
                if (col != e.Column) col.SortDirection = null;
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Fix: Cho phép ComboBox và các control khác tự xử lý cuộn chuột
            if (e.OriginalSource is DependencyObject source)
            {
                if (IsInsideControl<System.Windows.Controls.ComboBox>(source)) return;
            }

            // Scroll bubbling cho DataGrid
            if (e.Source is DependencyObject obj)
            {
                var scrollViewer = GetScrollViewer(obj);
                if (scrollViewer != null)
                {
                    double newOffset = scrollViewer.VerticalOffset - e.Delta;
                    if (newOffset <= 0 && e.Delta > 0) return; // Bubble up
                    if (newOffset >= scrollViewer.ScrollableHeight && e.Delta < 0) return; // Bubble up

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
