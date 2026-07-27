using System.Collections.Generic;
using System.Threading.Tasks;
using ShopManager.Core.Models;

namespace ShopManager.Services
{
    public interface IPurchaseOrderService
    {
        Task<IEnumerable<PurchaseOrder>> GetAllOrdersAsync();
        Task<PurchaseOrder?> GetOrderByIdAsync(int id);
        Task<int> CreateOrderAsync(PurchaseOrder po);
        Task<bool> ReceiveOrderAsync(int id); // Marks as Received and updates stock
        Task<bool> UpdatePaymentAsync(int id, decimal amount);
        Task<bool> CancelOrderAsync(int id);
    }
}
