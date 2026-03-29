using System.Windows;
using System.Windows.Input;
using FashionStore.ViewModels;

namespace FashionStore
{
    public partial class VoucherManagementWindow : Window
    {
        public VoucherManagementWindow()
        {
            InitializeComponent();
            DataContext = new VoucherManagementViewModel();
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DiscountTypeComboBox?.IsDropDownOpen == true ||
                FilterActiveComboBox?.IsDropDownOpen == true ||
                FilterDiscountTypeComboBox?.IsDropDownOpen == true ||
                FilterValidityComboBox?.IsDropDownOpen == true ||
                FilterUsageComboBox?.IsDropDownOpen == true)
            {
                e.Handled = true;
                return;
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
