using FashionStore.App.Core;
using FashionStore.Core.Models;
using FashionStore.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FashionStore.App.ViewModels
{
    public class FinanceViewModel : BaseViewModel
    {
        public ObservableCollection<Expense> Expenses { get; set; } = new();

        private string _category = string.Empty;
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private Expense? _selectedExpense;
        public Expense? SelectedExpense
        {
            get => _selectedExpense;
            set
            {
                if (SetProperty(ref _selectedExpense, value) && value != null)
                {
                    Category = value.Category;
                    Amount = value.Amount;
                    Description = value.Description ?? string.Empty;
                }
            }
        }

        private string _totalExpensesText = "0 đ";
        public string TotalExpensesText
        {
            get => _totalExpensesText;
            set => SetProperty(ref _totalExpensesText, value);
        }

        public List<string> Categories { get; } = new()
        {
            "Thuê mặt bằng", "Điện nước", "Lương nhân viên", "Vận chuyển",
            "Marketing", "Sửa chữa", "Văn phòng phẩm", "Khác"
        };

        public ICommand AddExpenseCommand { get; }
        public ICommand DeleteExpenseCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand RefreshCommand { get; }

        public FinanceViewModel()
        {
            AddExpenseCommand = new RelayCommand(AddExpense, _ => !string.IsNullOrWhiteSpace(Category) && Amount > 0);
            DeleteExpenseCommand = new RelayCommand(DeleteExpense, _ => SelectedExpense != null);
            ClearCommand = new RelayCommand(ClearForm);
            RefreshCommand = new RelayCommand(_ => LoadData());
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var expenses = FinanceService.GetAllExpenses();
                Expenses.Clear();
                decimal total = 0;
                foreach (var e in expenses)
                {
                    Expenses.Add(e);
                    total += e.Amount;
                }
                TotalExpensesText = total.ToString("C0");
            }
            catch (Exception ex)
            {
                AppLogger.Log(ex, "FinanceViewModel.LoadData");
            }
        }

        private void AddExpense(object? obj)
        {
            if (string.IsNullOrWhiteSpace(Category) || Amount <= 0) return;

            var username = Application.Current.Resources["CurrentUser"] as string ?? "";
            var employeeId = UserService.GetEmployeeIdByUsername(username);

            var expense = new Expense
            {
                Category = Category,
                Amount = Amount,
                Description = Description,
                EmployeeId = employeeId > 0 ? employeeId : 1,
                CreatedDate = DateTime.Now
            };

            var success = FinanceService.CreateExpense(expense);
            if (success)
            {
                ClearForm(null);
                LoadData();
                MessageBox.Show("✅ Đã ghi nhận chi phí!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("❌ Không thể lưu chi phí.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteExpense(object? obj)
        {
            if (SelectedExpense == null) return;
            var result = MessageBox.Show($"Xóa chi phí '{SelectedExpense.Category}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            var success = FinanceService.DeleteExpense(SelectedExpense.Id);
            if (success)
            {
                LoadData();
                ClearForm(null);
            }
        }

        private void ClearForm(object? obj)
        {
            SelectedExpense = null;
            Category = string.Empty;
            Amount = 0;
            Description = string.Empty;
        }
    }
}
