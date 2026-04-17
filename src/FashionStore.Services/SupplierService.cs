using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            return await _supplierRepository.GetAllAsync();
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            return await _supplierRepository.GetByIdAsync(id);
        }

        public async Task<bool> AddSupplierAsync(Supplier supplier)
        {
            if (string.IsNullOrWhiteSpace(supplier.Name)) return false;
            return await _supplierRepository.AddAsync(supplier);
        }

        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            if (supplier.Id <= 0 || string.IsNullOrWhiteSpace(supplier.Name)) return false;
            return await _supplierRepository.UpdateAsync(supplier);
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            if (await _supplierRepository.HasProductsAsync(id)) return false;
            return await _supplierRepository.DeleteAsync(id);
        }

        // Bridge for legacy static calls
        private static ISupplierService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<ISupplierService>() ?? throw new InvalidOperationException("DI not initialized");
        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();

        public static List<Supplier> GetAllSuppliers() => RunSync(() => GetService().GetAllSuppliersAsync()).ToList();
        public static Supplier? GetSupplierById(int id) => RunSync(() => GetService().GetSupplierByIdAsync(id));
        public static bool AddSupplier(Supplier supplier) => RunSync(() => GetService().AddSupplierAsync(supplier));
        public static bool UpdateSupplier(Supplier supplier) => RunSync(() => GetService().UpdateSupplierAsync(supplier));
        public static bool DeleteSupplier(int id) => RunSync(() => GetService().DeleteSupplierAsync(id));
    }
}
