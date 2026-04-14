using Dapper;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;

namespace FashionStore.Data.Repositories
{
    public class VoucherRepository : MySqlRepositoryBase, IVoucherRepository
    {
        private const string SelectColumns = @"
            Id, Code, IFNULL(Name,'') AS Name, DiscountType,
            IFNULL(DiscountValue, 0) AS DiscountValue,
            IFNULL(MaxDiscountAmount, 0) AS MaxDiscountAmount,
            IFNULL(MinInvoiceAmount, 0) AS MinInvoiceAmount,
            StartDate, EndDate,
            IFNULL(UsageLimit, 0) AS UsageLimit,
            IFNULL(UsageLimitPerCustomer, 0) AS UsageLimitPerCustomer,
            IFNULL(UsedCount, 0) AS UsedCount,
            IsActive,
            IFNULL(CreatedBy, 0) AS CreatedBy,
            IFNULL(CreatedDate, NOW()) AS CreatedDate,
            ApplicableCategories, ApplicableProducts, ApplicableCustomerTypes";

        public async Task<IEnumerable<Voucher>> GetAllAsync()
        {
            using var connection = GetConnection();
            string sql = $"SELECT {SelectColumns} FROM Vouchers ORDER BY Id ASC;";
            return await connection.QueryAsync<Voucher>(sql);
        }

        public async Task<Voucher?> GetByIdAsync(int id)
        {
            using var connection = GetConnection();
            string sql = $"SELECT {SelectColumns} FROM Vouchers WHERE Id = @Id;";
            return await connection.QueryFirstOrDefaultAsync<Voucher>(sql, new { Id = id });
        }

        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            using var connection = GetConnection();
            string sql = $"SELECT {SelectColumns} FROM Vouchers WHERE Code = @Code AND IsActive = 1;";
            return await connection.QueryFirstOrDefaultAsync<Voucher>(sql, new { Code = code });
        }

        public async Task<bool> AddAsync(Voucher entity)
        {
            using var connection = GetConnection();
            string sql = @"INSERT INTO Vouchers (Code, Name, DiscountType, DiscountValue, MaxDiscountAmount, 
                           MinInvoiceAmount, StartDate, EndDate, UsageLimit, UsageLimitPerCustomer, 
                           UsedCount, IsActive, CreatedBy, CreatedDate) 
                           VALUES (@Code, @Name, @DiscountType, @DiscountValue, @MaxDiscountAmount, 
                           @MinInvoiceAmount, @StartDate, @EndDate, @UsageLimit, @UsageLimitPerCustomer, 
                           @UsedCount, @IsActive, @CreatedBy, @CreatedDate);";
            return await connection.ExecuteAsync(sql, entity) > 0;
        }

        public async Task<bool> UpdateAsync(Voucher entity)
        {
            using var connection = GetConnection();
            string sql = @"UPDATE Vouchers SET Code=@Code, Name=@Name, DiscountType=@DiscountType, 
                           DiscountValue=@DiscountValue, MaxDiscountAmount=@MaxDiscountAmount, 
                           MinInvoiceAmount=@MinInvoiceAmount, StartDate=@StartDate, EndDate=@EndDate, 
                           UsageLimit=@UsageLimit, UsageLimitPerCustomer=@UsageLimitPerCustomer, 
                           UsedCount=@UsedCount, IsActive=@IsActive WHERE Id=@Id;";
            return await connection.ExecuteAsync(sql, entity) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            string sql = "UPDATE Vouchers SET IsActive = 0 WHERE Id = @Id;";
            return await connection.ExecuteAsync(sql, new { Id = id }) > 0;
        }

        public async Task<int> GetUsageCountForCustomerAsync(int voucherId, int customerId)
        {
            using var connection = GetConnection();
            string sql = "SELECT COUNT(*) FROM Invoices WHERE CustomerId = @CustomerId AND VoucherId = @VoucherId;";
            return await connection.ExecuteScalarAsync<int>(sql, new { CustomerId = customerId, VoucherId = voucherId });
        }

        public async Task<bool> IncrementUsedCountAsync(int voucherId)
        {
            using var connection = GetConnection();
            string sql = "UPDATE Vouchers SET UsedCount = UsedCount + 1 WHERE Id = @Id;";
            return await connection.ExecuteAsync(sql, new { Id = voucherId }) > 0;
        }

    }
}
