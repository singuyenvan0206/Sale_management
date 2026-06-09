using ShopManager.App.Core;
using ShopManager.Core.Models;
using ShopManager.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ShopManager.App.ViewModels
{
    public class ShiftManagementViewModel : BaseViewModel
    {
        public ObservableCollection<EmployeeShift> Shifts { get; set; } = new();

        private EmployeeShift? _activeShift;
        public EmployeeShift? ActiveShift
        {
            get => _activeShift;
            set { SetProperty(ref _activeShift, value); OnPropertyChanged(nameof(HasActiveShift)); OnPropertyChanged(nameof(NoActiveShift)); }
        }

        public bool HasActiveShift => ActiveShift != null;
        public bool NoActiveShift => ActiveShift == null;

        private decimal _openingBalance;
        public decimal OpeningBalance
        {
            get => _openingBalance;
            set => SetProperty(ref _openingBalance, value);
        }

        private decimal _closingBalance;
        public decimal ClosingBalance
        {
            get => _closingBalance;
            set => SetProperty(ref _closingBalance, value);
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        private string _employeeName = string.Empty;
        public string EmployeeName
        {
            get => _employeeName;
            set => SetProperty(ref _employeeName, value);
        }

        public ICommand ClockInCommand { get; }
        public ICommand ClockOutCommand { get; }
        public ICommand RefreshCommand { get; }

        public ShiftManagementViewModel()
        {
            ClockInCommand = new RelayCommand(ClockIn, _ => NoActiveShift);
            ClockOutCommand = new RelayCommand(ClockOut, _ => HasActiveShift);
            RefreshCommand = new RelayCommand(_ => LoadData());

            _ = InitializeAsync();
        }

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            string? username = Application.Current.Resources["CurrentUser"] as string;
            int employeeId = 0;
            string empName = "";

            if (!string.IsNullOrEmpty(username))
            {
                empName = await System.Threading.Tasks.Task.Run(() => UserService.GetEmployeeName(username) ?? username);
                employeeId = await System.Threading.Tasks.Task.Run(() => UserService.GetEmployeeIdByUsername(username));
            }

            EmployeeName = empName;

            var activeShiftTask = employeeId > 0
                ? System.Threading.Tasks.Task.Run(() => ShiftService.GetActiveShiftByEmployeeId(employeeId))
                : System.Threading.Tasks.Task.FromResult<EmployeeShift?>(null);

            var historyTask = System.Threading.Tasks.Task.Run(() => ShiftService.GetShiftHistory(50));

            await System.Threading.Tasks.Task.WhenAll(activeShiftTask, historyTask);

            ActiveShift = activeShiftTask.Result;

            Shifts.Clear();
            foreach (var s in historyTask.Result)
            {
                Shifts.Add(s);
            }
        }

        private void LoadActiveShift(int employeeId)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var shift = ShiftService.GetActiveShiftByEmployeeId(employeeId);
                    Application.Current.Dispatcher.Invoke(() => ActiveShift = shift);
                }
                catch (Exception ex)
                {
                    AppLogger.Log(ex, "ShiftManagementViewModel.LoadActiveShift");
                }
            });
        }

        private void LoadData()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var history = ShiftService.GetShiftHistory(50);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Shifts.Clear();
                        foreach (var s in history) Shifts.Add(s);
                    });
                }
                catch (Exception ex)
                {
                    AppLogger.Log(ex, "ShiftManagementViewModel.LoadData");
                }
            });
        }

        private void ClockIn(object? obj)
        {
            try
            {
                var username = Application.Current.Resources["CurrentUser"] as string ?? "";
                var employeeId = UserService.GetEmployeeIdByUsername(username);

                var shift = new EmployeeShift
                {
                    EmployeeId = employeeId,
                    ClockIn = DateTime.Now,
                    OpeningBalance = OpeningBalance,
                    Notes = Notes
                };

                var id = ShiftService.ClockIn(shift);
                if (id > 0)
                {
                    shift.Id = id;
                    ActiveShift = shift;
                    Notes = string.Empty;
                    LoadData();
                    MessageBox.Show("✅ Bắt đầu ca làm việc thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("❌ Không thể bắt đầu ca. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClockOut(object? obj)
        {
            if (ActiveShift == null) return;
            var result = MessageBox.Show("Bạn có chắc muốn kết thúc ca làm việc?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var success = ShiftService.ClockOut(ActiveShift.Id, ClosingBalance, Notes);
                if (success)
                {
                    ActiveShift = null;
                    ClosingBalance = 0;
                    Notes = string.Empty;
                    LoadData();
                    MessageBox.Show("✅ Kết thúc ca làm việc thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("❌ Không thể kết thúc ca. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
