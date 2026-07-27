using System.Collections.Generic;
using System.Threading.Tasks;
using ShopManager.Core.Models;

namespace ShopManager.Core.Interfaces
{
    public interface IPurchaseOrderRepository
    {
        Task<IEnumerable<PurchaseOrder>> GetAllAsync();
        Task<PurchaseOrder?> GetByIdAsync(int id);
        Task<int> CreateAsync(PurchaseOrder po);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> UpdatePaidAmountAsync(int id, decimal amount);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<PurchaseOrder>> GetBySupplierIdAsync(int supplierId);
    }
}
