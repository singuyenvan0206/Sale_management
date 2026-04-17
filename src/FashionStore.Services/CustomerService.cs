using Dapper;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using FashionStore.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace FashionStore.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        public async Task<Customer?> GetCustomerByPhoneAsync(string phone)
        {
            return await _customerRepository.GetByPhoneAsync(phone);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string query)
        {
            return await _customerRepository.SearchAsync(query);
        }

        public async Task<bool> AddCustomerAsync(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name)) return false;
            return await _customerRepository.AddAsync(customer);
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            if (customer.Id <= 0 || string.IsNullOrWhiteSpace(customer.Name)) return false;
            return await _customerRepository.UpdateAsync(customer);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            if (await _customerRepository.HasInvoicesAsync(id)) return false;
            return await _customerRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateLoyaltyAsync(int customerId, int points, string customerType)
        {
            return await _customerRepository.UpdateLoyaltyAsync(customerId, points, customerType);
        }

        public async Task<int> GetOrCreateCustomerIdAsync(string name, string phone, string email, string address)
        {
            if (!string.IsNullOrWhiteSpace(phone))
            {
                var existing = await _customerRepository.GetByPhoneAsync(phone);
                if (existing != null) return existing.Id;
            }

            var newCustomer = new Customer
            {
                Name = name ?? "Unknown",
                Phone = phone,
                Email = email,
                Address = address,
                CustomerType = "Regular",
                Points = 0
            };

            if (await _customerRepository.AddAsync(newCustomer))
            {
                var created = await _customerRepository.GetByPhoneAsync(phone ?? "");
                if (created != null) return created.Id;

                if (string.IsNullOrWhiteSpace(phone))
                {
                    var results = await _customerRepository.SearchAsync(name ?? "Unknown");
                    return results.FirstOrDefault()?.Id ?? 1;
                }
            }
            return 1;
        }

        public async Task<IEnumerable<(string Name, decimal TotalSpent)>> GetTopCustomersAsync(int topN)
        {
            return await _customerRepository.GetTopCustomersAsync(topN);
        }

        public async Task<int> RefreshAllLoyaltyAsync(decimal spendPerPoint, int silverMin, int goldMin, int vipMin)
        {
            return await _customerRepository.RefreshAllLoyaltyAsync(spendPerPoint, silverMin, goldMin, vipMin);
        }

        public async Task<bool> ExportCustomersToCsvAsync(string filePath)
        {
            try
            {
                var customers = await _customerRepository.GetAllAsync();
                var lines = new List<string>(customers.Count() + 1)
                {
                    "Id,Name,Phone,Email,Address,CustomerType,Points,TotalSpent"
                };

                foreach (var c in customers)
                {
                    lines.Add($"{c.Id},{CsvHelper.Escape(c.Name)},{CsvHelper.Escape(c.Phone)},{CsvHelper.Escape(c.Email)},{CsvHelper.Escape(c.Address)},{CsvHelper.Escape(c.CustomerType)},{c.Points},{c.TotalSpent:F0}");
                }

                await File.WriteAllLinesAsync(filePath, lines, new System.Text.UTF8Encoding(true));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> ImportCustomersFromCsvAsync(string filePath)
        {
            try
            {
                var allLines = await File.ReadAllLinesAsync(filePath, System.Text.Encoding.UTF8);
                if (allLines.Length < 2) return 0;

                var customersToImport = new List<Customer>();
                foreach (var line in allLines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = CsvHelper.ParseLine(line);
                    if (cols.Length < 2) continue; // Min Name

                    customersToImport.Add(new Customer
                    {
                        Name = cols[1].Trim('"'),
                        Phone = cols.Length > 2 ? cols[2].Trim('"') : "",
                        Email = cols.Length > 3 ? cols[3].Trim('"') : "",
                        Address = cols.Length > 4 ? cols[4].Trim('"') : "",
                        CustomerType = cols.Length > 5 ? cols[5].Trim('"') : "Regular",
                        Points = cols.Length > 6 && int.TryParse(cols[6], out int p) ? p : 0
                    });
                }

                return await _customerRepository.BulkImportCustomersAsync(customersToImport);
            }
            catch
            {
                return -1;
            }
        }

        public async Task<IEnumerable<(int InvoiceId, DateTime CreatedAt, int ItemCount, decimal Total)>> GetCustomerPurchaseHistoryAsync(int customerId)
        {
            return await _customerRepository.GetCustomerPurchaseHistoryAsync(customerId);
        }

        #region Legacy Static Bridge - SHOULD BE DEPRECATED

        private static ICustomerService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<ICustomerService>() ?? throw new InvalidOperationException("DI not initialized");

        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();

        public static List<(int Id, string Name, string Phone, string Email, string Address, string CustomerType, int Points)> GetAllCustomers()
            => RunSync(() => GetService().GetAllCustomersAsync())
               .Select(c => (c.Id, c.Name, c.Phone ?? "", c.Email ?? "", c.Address ?? "", c.CustomerType, c.Points)).ToList();

        public static (string Tier, int Points) GetCustomerLoyalty(int customerId)
        {
            var c = RunSync(() => GetService().GetCustomerByIdAsync(customerId));
            return (c?.CustomerType ?? "Regular", c?.Points ?? 0);
        }

        public static bool UpdateCustomerLoyalty(int customerId, int newPoints, string newTier)
            => RunSync(() => GetService().UpdateLoyaltyAsync(customerId, newPoints, newTier));

        public static bool AddCustomer(string name, string phone, string email, string customerType, string address)
            => RunSync(() => GetService().AddCustomerAsync(new Customer { Name = name, Phone = phone, Email = email, CustomerType = customerType, Address = address }));

        public static bool UpdateCustomer(int id, string name, string phone, string email, string customerType, string address)
            => RunSync(() => GetService().UpdateCustomerAsync(new Customer { Id = id, Name = name, Phone = phone, Email = email, CustomerType = customerType, Address = address }));

        public static bool DeleteCustomer(int id)
            => RunSync(() => GetService().DeleteCustomerAsync(id));

        public static int GetOrCreateCustomerId(string name, string phone, string email, string address)
            => RunSync(() => GetService().GetOrCreateCustomerIdAsync(name, phone, email, address));

        public static int GetTotalCustomers()
            => RunSync(() => GetService().GetAllCustomersAsync()).Count();

        public static int ImportCustomersFromCsv(string filePath) => RunSync(() => GetService().ImportCustomersFromCsvAsync(filePath));

        public static bool ExportCustomersToCsv(string filePath) => RunSync(() => GetService().ExportCustomersToCsvAsync(filePath));

        public static bool DeleteAllCustomers()
        {
            try
            {
                const string sql = @"
                    DELETE c
                    FROM Customers c
                    LEFT JOIN Invoices i ON i.CustomerId = c.Id
                    WHERE i.Id IS NULL;";
                using var connection = new MySql.Data.MySqlClient.MySqlConnection(FashionStore.Core.Settings.SettingsManager.BuildConnectionString());
                connection.Open();
                connection.Execute(sql);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<(int InvoiceId, DateTime CreatedAt, int ItemCount, decimal Total)> GetCustomerPurchaseHistory(int customerId)
            => RunSync(() => GetService().GetCustomerPurchaseHistoryAsync(customerId)).ToList();

        public static List<(string Name, decimal TotalSpent)> GetTopCustomers(int topN = 10)
            => RunSync(() => GetService().GetTopCustomersAsync(topN)).ToList();

        public static int RefreshAllLoyalty(decimal spendPerPoint, int silverMin, int goldMin, int vipMin)
            => RunSync(() => GetService().RefreshAllLoyaltyAsync(spendPerPoint, silverMin, goldMin, vipMin));

        #endregion
    }
}
