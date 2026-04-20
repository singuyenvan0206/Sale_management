using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;

namespace FashionStore.Data.Repositories
{
    public class PurchaseOrderRepository : MySqlRepositoryBase, IPurchaseOrderRepository
    {
        public async Task<IEnumerable<PurchaseOrder>> GetAllAsync()
        {
            using var conn = GetConnection();
            const string sql = @"
                SELECT po.*, s.Name as SupplierName, a.EmployeeName
                FROM purchaseorders po
                JOIN suppliers s ON po.SupplierId = s.Id
                JOIN accounts a ON po.EmployeeId = a.Id
                ORDER BY po.CreatedDate DESC";
            return await conn.QueryAsync<PurchaseOrder>(sql);
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            using var conn = GetConnection();
            const string poSql = @"
                SELECT po.*, s.Name as SupplierName, a.EmployeeName
                FROM purchaseorders po
                JOIN suppliers s ON po.SupplierId = s.Id
                JOIN accounts a ON po.EmployeeId = a.Id
                WHERE po.Id = @id";
            
            var po = await conn.QueryFirstOrDefaultAsync<PurchaseOrder>(poSql, new { id });
            if (po == null) return null;

            const string itemsSql = @"
                SELECT poi.*, p.Name as ProductName, p.Code as ProductCode
                FROM purchaseorderitems poi
                JOIN products p ON poi.ProductId = p.Id
                WHERE poi.PurchaseOrderId = @id";
            
            var items = await conn.QueryAsync<PurchaseOrderItem>(itemsSql, new { id });
            po.Items = items.ToList();
            
            return po;
        }

        public async Task<int> CreateAsync(PurchaseOrder po)
        {
            using var conn = GetConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                const string poSql = @"
                    INSERT INTO purchaseorders (SupplierId, EmployeeId, TotalAmount, PaidAmount, Status, Notes)
                    VALUES (@SupplierId, @EmployeeId, @TotalAmount, @PaidAmount, @Status, @Notes);
                    SELECT LAST_INSERT_ID();";
                
                var id = await conn.ExecuteScalarAsync<int>(poSql, po, trans);
                
                const string itemSql = @"
                    INSERT INTO purchaseorderitems (PurchaseOrderId, ProductId, Quantity, UnitPrice, LineTotal)
                    VALUES (@PurchaseOrderId, @ProductId, @Quantity, @UnitPrice, @LineTotal)";
                
                foreach (var item in po.Items)
                {
                    item.PurchaseOrderId = id;
                    await conn.ExecuteAsync(itemSql, new
                    {
                        item.PurchaseOrderId,
                        item.ProductId,
                        item.Quantity,
                        item.UnitPrice,
                        LineTotal = item.LineTotal  // computed: Quantity * UnitPrice
                    }, trans);
                }
                
                trans.Commit();
                return id;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            using var conn = GetConnection();
            const string sql = "UPDATE purchaseorders SET Status = @status WHERE Id = @id";
            return await conn.ExecuteAsync(sql, new { id, status }) > 0;
        }

        public async Task<bool> UpdatePaidAmountAsync(int id, decimal amount)
        {
            using var conn = GetConnection();
            const string sql = "UPDATE purchaseorders SET PaidAmount = @amount WHERE Id = @id";
            return await conn.ExecuteAsync(sql, new { id, amount }) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = GetConnection();
            const string sql = "DELETE FROM purchaseorders WHERE Id = @id";
            return await conn.ExecuteAsync(sql, new { id }) > 0;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetBySupplierIdAsync(int supplierId)
        {
            using var conn = GetConnection();
            const string sql = @"
                SELECT po.*, s.Name as SupplierName, a.EmployeeName
                FROM purchaseorders po
                JOIN suppliers s ON po.SupplierId = s.Id
                JOIN accounts a ON po.EmployeeId = a.Id
                WHERE po.SupplierId = @supplierId
                ORDER BY po.CreatedDate DESC";
            return await conn.QueryAsync<PurchaseOrder>(sql, new { supplierId });
        }
    }
}
