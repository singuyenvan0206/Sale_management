using FashionStore.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FashionStore.App.Views
{
    public partial class InvoiceManagementWindow : Window
    {
        public InvoiceManagementWindow()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<InvoiceManagementViewModel>() ?? new InvoiceManagementViewModel();
        }

        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is DependencyObject obj)
            {
                var scrollViewer = GetScrollViewer(obj);
                if (scrollViewer != null)
                {
                    double newOffset = scrollViewer.VerticalOffset - e.Delta;
                    if (newOffset <= 0 && e.Delta > 0)
                    {
                        // At the top and scrolling up - bubble to parent
                        return;
                    }
                    if (newOffset >= scrollViewer.ScrollableHeight && e.Delta < 0)
                    {
                        // At the bottom and scrolling down - bubble to parent
                        return;
                    }

                    scrollViewer.ScrollToVerticalOffset(newOffset);
                    e.Handled = true;
                }
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                double newOffset = scrollViewer.VerticalOffset - e.Delta;
                if (newOffset <= 0 && e.Delta > 0) return;
                if (newOffset >= scrollViewer.ScrollableHeight && e.Delta < 0) return;

                scrollViewer.ScrollToVerticalOffset(newOffset);
                e.Handled = true;
            }
        }

        private ScrollViewer? GetScrollViewer(DependencyObject obj)
        {
            if (obj is ScrollViewer sv) return sv;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                var result = GetScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }

        private void Grid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Fix: Cho phép ComboBox và các control khác tự xử lý cuộn chuột
            if (e.OriginalSource is DependencyObject source)
            {
                // Riêng với Popup của CustomerSuggestions, chúng ta vẫn có thể giữ logic đặc biệt nếu cần, 
                // nhưng ComboBox nên được để tự nhiên.
                if (IsInsideControl<System.Windows.Controls.ComboBox>(source)) return;
                if (IsInsideControl<System.Windows.Controls.ListBox>(source)) return;
            }

            // Mặc định không làm gì nếu không phải là scroll bubbling cho nội dung chính
        }

        private bool IsInsideControl<T>(DependencyObject obj) where T : DependencyObject
        {
            DependencyObject? parent = obj;
            while (parent != null)
            {
                if (parent is T) return true;
                parent = (parent is System.Windows.Media.Visual || parent is System.Windows.Media.Media3D.Visual3D)
                    ? VisualTreeHelper.GetParent(parent)
                    : null;
            }
            return false;
        }
    }
}
