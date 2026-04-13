using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FashionStore.Data.Interfaces;
using FashionStore.Models;

namespace FashionStore.Data.Repositories
{
    public class CategoryRepository : MySqlRepositoryBase, ICategoryRepository
    {
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            using var connection = GetConnection();
            string sql = "SELECT Id, Name, TaxPercent, Description, IsActive FROM Categories ORDER BY Id;";
            return await connection.QueryAsync<Category>(sql);
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            using var connection = GetConnection();
            string sql = "SELECT Id, Name, TaxPercent, Description, IsActive FROM Categories WHERE Id = @Id;";
            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { Id = id });
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            using var connection = GetConnection();
            string sql = "SELECT Id, Name, TaxPercent, Description, IsActive FROM Categories WHERE Name = @Name LIMIT 1;";
            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { Name = name });
        }

        public async Task<bool> AddAsync(Category entity)
        {
            using var connection = GetConnection();
            string sql = "INSERT INTO Categories (Name, TaxPercent, Description, IsActive) VALUES (@Name, @TaxPercent, @Description, @IsActive);";
            return await connection.ExecuteAsync(sql, entity) > 0;
        }

        public async Task<bool> UpdateAsync(Category entity)
        {
            using var connection = GetConnection();
            string sql = "UPDATE Categories SET Name = @Name, TaxPercent = @TaxPercent, Description = @Description, IsActive = @IsActive WHERE Id = @Id;";
            return await connection.ExecuteAsync(sql, entity) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            string sql = "DELETE FROM Categories WHERE Id = @Id;";
            return await connection.ExecuteAsync(sql, new { Id = id }) > 0;
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            using var connection = GetConnection();
            string sql = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId;";
            var count = await connection.ExecuteScalarAsync<long>(sql, new { CategoryId = categoryId });
            return count > 0;
        }
    }
}
