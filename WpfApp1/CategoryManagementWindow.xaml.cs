using System.Windows;
using System.Collections.Generic;

namespace WpfApp1
{
    public partial class CategoryManagementWindow : Window
    {
        private List<(int Id, string Name)> _categories = new();

        public CategoryManagementWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            _categories = DatabaseHelper.GetAllCategories();
            CategoryListBox.ItemsSource = null;
            CategoryListBox.ItemsSource = _categories.ConvertAll(c => new { c.Id, c.Name });
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            int count = _categories.Count;
            StatusTextBlock.Text = count == 1 ? "1 category found" : $"{count} categories found";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string name = CategoryNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a category name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                CategoryNameTextBox.Focus();
                return;
            }
            
            if (name.Length > 255)
            {
                MessageBox.Show("Category name is too long. Maximum 255 characters allowed.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (DatabaseHelper.AddCategory(name))
            {
                LoadCategories();
                CategoryNameTextBox.Clear();
                MessageBox.Show($"Category '{name}' added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Category '{name}' already exists or an error occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = CategoryListBox.SelectedItem;
            if (selectedItem != null)
            {
                var selected = new { Id = 0, Name = "" };
                try
                {
                    selected = (dynamic)selectedItem;
                }
                catch
                {
                    MessageBox.Show("Invalid selection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string name = CategoryNameTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Please enter a category name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CategoryNameTextBox.Focus();
                    return;
                }
                
                if (name.Length > 255)
                {
                    MessageBox.Show("Category name is too long. Maximum 255 characters allowed.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (DatabaseHelper.UpdateCategory(selected.Id, name))
                {
                    LoadCategories();
                    CategoryNameTextBox.Clear();
                    MessageBox.Show($"Category updated successfully to '{name}'!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to update category. The name '{name}' might already exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a category to update.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = CategoryListBox.SelectedItem;
            if (selectedItem != null)
            {
                var selected = new { Id = 0, Name = "" };
                try
                {
                    selected = (dynamic)selectedItem;
                }
                catch
                {
                    MessageBox.Show("Invalid selection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string categoryName = selected.Name;
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the category '{categoryName}'?\n\nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    if (DatabaseHelper.DeleteCategory(selected.Id))
                    {
                        LoadCategories();
                        CategoryNameTextBox.Clear();
                        MessageBox.Show($"Category '{categoryName}' deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Cannot delete category '{categoryName}'. It may be in use by existing products.", "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a category to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CategoryListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selectedItem = CategoryListBox.SelectedItem;
            if (selectedItem != null)
            {
                try
                {
                    var selected = (dynamic)selectedItem;
                    CategoryNameTextBox.Text = selected.Name;
                }
                catch
                {
                    CategoryNameTextBox.Text = "";
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryNameTextBox.Clear();
            CategoryListBox.SelectedItem = null;
            CategoryNameTextBox.Focus();
        }
    }
}