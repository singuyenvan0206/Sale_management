using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class ProductSearchWindow : Window
    {
        private List<ProductSearchItem> _allProducts = new();
        private List<ProductSearchItem> _filteredProducts = new();

        public ProductSearchWindow()
        {
            InitializeComponent();
            LoadProducts();
            SearchTextBox.Focus();
        }

        private void LoadProducts()
        {
            try
            {
                var products = DatabaseHelper.GetAllProductsWithCategories();
                _allProducts = products.Select(p => new ProductSearchItem
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    CategoryName = p.CategoryName,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity
                }).ToList();

                _filteredProducts = _allProducts.ToList();
                ProductsDataGrid.ItemsSource = _filteredProducts;
                StatusTextBlock.Text = $"Tìm thấy {_filteredProducts.Count} sản phẩm";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách sản phẩm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            FilterProducts();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            FilterProducts();
        }

        private void FilterProducts()
        {
            try
            {
                string searchText = SearchTextBox.Text?.ToLower() ?? "";
                
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    _filteredProducts = _allProducts.ToList();
                }
                else
                {
                    _filteredProducts = _allProducts.Where(p => 
                        p.Name.ToLower().Contains(searchText) ||
                        p.Code.ToLower().Contains(searchText) ||
                        p.CategoryName.ToLower().Contains(searchText)
                    ).ToList();
                }

                ProductsDataGrid.ItemsSource = _filteredProducts;
                StatusTextBlock.Text = $"Tìm thấy {_filteredProducts.Count} sản phẩm";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProductsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectCurrentProduct();
        }

        private void SelectProductButton_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentProduct();
        }

        private void SelectCurrentProduct()
        {
            if (ProductsDataGrid.SelectedItem is ProductSearchItem selectedProduct)
            {
                // Hiển thị thông tin sản phẩm được chọn
                string message = $"Bạn đã chọn sản phẩm:\n" +
                               $"Tên: {selectedProduct.Name}\n" +
                               $"Mã: {selectedProduct.Code}\n" +
                               $"Giá: {selectedProduct.Price:N0} VND\n" +
                               $"Tồn kho: {selectedProduct.StockQuantity}\n\n" +
                               $"Bạn có muốn đóng cửa sổ tìm kiếm không?";
                
                var result = MessageBox.Show(message, "Sản phẩm đã chọn", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // Tạo một event để thông báo sản phẩm được chọn
                    ProductSelected?.Invoke(selectedProduct);
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một sản phẩm.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Event để thông báo sản phẩm được chọn
        public event Action<ProductSearchItem> ProductSelected;
    }

    public class ProductSearchItem
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
