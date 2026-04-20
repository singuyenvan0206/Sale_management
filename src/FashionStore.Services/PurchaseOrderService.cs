using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _poRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockMovementRepository _stockMovementRepository;

        public PurchaseOrderService(
            IPurchaseOrderRepository poRepository,
            IProductRepository productRepository,
            IStockMovementRepository stockMovementRepository)
        {
            _poRepository = poRepository;
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllOrdersAsync()
        {
            return await _poRepository.GetAllAsync();
        }

        public async Task<PurchaseOrder?> GetOrderByIdAsync(int id)
        {
            return await _poRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateOrderAsync(PurchaseOrder po)
        {
            po.CreatedDate = DateTime.Now;
            po.UpdatedDate = DateTime.Now;
            return await _poRepository.CreateAsync(po);
        }

        public async Task<bool> ReceiveOrderAsync(int id)
        {
            var po = await _poRepository.GetByIdAsync(id);
            if (po == null || po.Status == "Received") return false;

            // Update stock and record movement for each item
            foreach (var item in po.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    var previousStock = product.StockQuantity;
                    product.StockQuantity += item.Quantity;
                    await _productRepository.UpdateAsync(product);

                    // Record the stock movement
                    await _stockMovementRepository.AddAsync(new StockMovement
                    {
                        ProductId = item.ProductId,
                        MovementType = "Import",
                        Quantity = item.Quantity,
                        PreviousStock = previousStock,
                        NewStock = product.StockQuantity,
                        ReferenceType = "PurchaseOrder",
                        ReferenceId = po.Id,
                        Notes = $"Nhập hàng đơn #PO-{po.Id:D5}",
                        EmployeeId = po.EmployeeId,
                        CreatedDate = DateTime.Now
                    });
                }
            }

            return await _poRepository.UpdateStatusAsync(id, "Received");
        }

        public async Task<bool> UpdatePaymentAsync(int id, decimal amount)
        {
            return await _poRepository.UpdatePaidAmountAsync(id, amount);
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            return await _poRepository.UpdateStatusAsync(id, "Cancelled");
        }

        // Bridge for legacy static calls (WPF)
        private static IPurchaseOrderService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<IPurchaseOrderService>() ?? throw new InvalidOperationException("DI not initialized");
        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();

        public static List<PurchaseOrder> GetAllOrders() => RunSync(() => GetService().GetAllOrdersAsync()).ToList();
        public static PurchaseOrder? GetOrderById(int id) => RunSync(() => GetService().GetOrderByIdAsync(id));
        public static int CreateOrder(PurchaseOrder po) => RunSync(() => GetService().CreateOrderAsync(po));
        public static bool ReceiveOrder(int id) => RunSync(() => GetService().ReceiveOrderAsync(id));
        public static bool CancelOrder(int id) => RunSync(() => GetService().CancelOrderAsync(id));
}
}
