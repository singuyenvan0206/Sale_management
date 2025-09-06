using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class CustomerManagementWindow : Window
    {
        private List<CustomerViewModel> _customers = new();
        private CustomerViewModel _selectedCustomer;

        public CustomerManagementWindow()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            var customers = DatabaseHelper.GetAllCustomers();
            _customers = customers.ConvertAll(c => new CustomerViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                CustomerType = c.CustomerType
            });
            CustomerDataGrid.ItemsSource = _customers;
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            int count = _customers.Count;
            StatusTextBlock.Text = count == 1 ? "1 customer found" : $"{count} customers found";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            var customer = new CustomerViewModel
            {
                Name = CustomerNameTextBox.Text.Trim(),
                Phone = PhoneTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim(),
                CustomerType = (CustomerTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Regular",
                Address = AddressTextBox.Text.Trim()
            };

            if (DatabaseHelper.AddCustomer(customer.Name, customer.Phone, customer.Email, customer.CustomerType, customer.Address))
            {
                LoadCustomers();
                ClearForm();
                MessageBox.Show($"Customer '{customer.Name}' added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to add customer. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Please select a customer to update.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!ValidateInput()) return;

            var customer = new CustomerViewModel
            {
                Id = _selectedCustomer.Id,
                Name = CustomerNameTextBox.Text.Trim(),
                Phone = PhoneTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim(),
                CustomerType = (CustomerTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Regular",
                Address = AddressTextBox.Text.Trim()
            };

            if (DatabaseHelper.UpdateCustomer(customer.Id, customer.Name, customer.Phone, customer.Email, customer.CustomerType, customer.Address))
            {
                LoadCustomers();
                ClearForm();
                MessageBox.Show($"Customer '{customer.Name}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to update customer. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Please select a customer to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string customerName = _selectedCustomer.Name;
            int customerId = _selectedCustomer.Id;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the customer '{customerName}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (DatabaseHelper.DeleteCustomer(customerId))
                {
                    LoadCustomers();
                    ClearForm();
                    MessageBox.Show($"Customer '{customerName}' deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to delete customer. It may be in use by existing invoices.", "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            CustomerNameTextBox.Clear();
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
            CustomerTypeComboBox.SelectedIndex = 0;
            AddressTextBox.Clear();
            _selectedCustomer = null;
            CustomerDataGrid.SelectedItem = null;
            CustomerNameTextBox.Focus();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
            {
                MessageBox.Show("Please enter a customer name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                CustomerNameTextBox.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(EmailTextBox.Text) && !IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailTextBox.Focus();
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void CustomerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCustomer = CustomerDataGrid.SelectedItem as CustomerViewModel;
            if (_selectedCustomer != null)
            {
                CustomerNameTextBox.Text = _selectedCustomer.Name ?? "";
                PhoneTextBox.Text = _selectedCustomer.Phone ?? "";
                EmailTextBox.Text = _selectedCustomer.Email ?? "";
                AddressTextBox.Text = _selectedCustomer.Address ?? "";
                
                // Set the customer type in combo box
                foreach (ComboBoxItem item in CustomerTypeComboBox.Items)
                {
                    if (item.Content.ToString() == (_selectedCustomer.CustomerType ?? ""))
                    {
                        CustomerTypeComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterCustomers();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            FilterCustomers();
        }

        private void FilterCustomers()
        {
            string searchTerm = SearchTextBox.Text.ToLower();
            var filteredCustomers = _customers.Where(c =>
                c.Name.ToLower().Contains(searchTerm) ||
                c.Phone.ToLower().Contains(searchTerm) ||
                c.Email.ToLower().Contains(searchTerm) ||
                c.CustomerType.ToLower().Contains(searchTerm) ||
                c.Address.ToLower().Contains(searchTerm)
            ).ToList();
            CustomerDataGrid.ItemsSource = filteredCustomers;
        }
    }

    public class CustomerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string CustomerType { get; set; } = "Regular";
    }
}
