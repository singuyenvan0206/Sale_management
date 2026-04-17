using Dapper;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;

namespace FashionStore.Data.Repositories
{
    public class SupplierRepository : MySqlRepositoryBase, ISupplierRepository
    {
        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            using var connection = GetConnection();
            return await connection.QueryAsync<Supplier>("SELECT * FROM Suppliers ORDER BY Id");
        }

        public async Task<Supplier?> GetByIdAsync(int id)
        {
            using var connection = GetConnection();
            return await connection.QueryFirstOrDefaultAsync<Supplier>("SELECT * FROM Suppliers WHERE Id = @Id", new { Id = id });
        }

        public async Task<bool> AddAsync(Supplier supplier)
        {
            using var connection = GetConnection();
            string sql = "INSERT INTO Suppliers (Name, ContactName, Phone, Email, Address) VALUES (@Name, @ContactName, @Phone, @Email, @Address)";
            return await connection.ExecuteAsync(sql, supplier) > 0;
        }

        public async Task<bool> UpdateAsync(Supplier supplier)
        {
            using var connection = GetConnection();
            string sql = "UPDATE Suppliers SET Name=@Name, ContactName=@ContactName, Phone=@Phone, Email=@Email, Address=@Address WHERE Id=@Id";
            return await connection.ExecuteAsync(sql, supplier) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            string sql = "DELETE FROM Suppliers WHERE Id=@Id";
            return await connection.ExecuteAsync(sql, new { Id = id }) > 0;
        }

        public async Task<bool> HasProductsAsync(int supplierId)
        {
            using var connection = GetConnection();
            string sql = "SELECT COUNT(*) FROM Products WHERE SupplierId = @Id";
            long count = await connection.ExecuteScalarAsync<long>(sql, new { Id = supplierId });
            return count > 0;
        }
    }
}
