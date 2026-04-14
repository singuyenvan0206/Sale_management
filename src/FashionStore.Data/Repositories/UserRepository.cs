using Dapper;
using FashionStore.Core.Interfaces;

namespace FashionStore.Data.Repositories
{
    public class UserRepository : MySqlRepositoryBase, IUserRepository
    {
        public async Task<IEnumerable<(int Id, string Username, string EmployeeName, string Role)>> GetAllAsync()
        {
            using var connection = GetConnection();
            string sql = "SELECT Id, Username, COALESCE(EmployeeName, '') as EmployeeName, Role FROM Accounts ORDER BY Id ASC;";
            var result = await connection.QueryAsync<dynamic>(sql);
            return result.Select(r => ((int)r.Id, (string)r.Username, (string)r.EmployeeName, (string)r.Role));
        }

        public async Task<(int Id, string Username, string EmployeeName, string Role)?> GetByUsernameAsync(string username)
        {
            using var connection = GetConnection();
            string sql = "SELECT Id, Username, COALESCE(EmployeeName, '') as EmployeeName, Role FROM Accounts WHERE Username = @Username LIMIT 1;";
            var r = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Username = username });
            if (r != null)
            {
                return ((int)r.Id, (string)r.Username, (string)r.EmployeeName, (string)r.Role);
            }
            return null;
        }

        public async Task<string?> GetPasswordHashAsync(string username)
        {
            using var connection = GetConnection();
            string sql = "SELECT Password FROM Accounts WHERE Username = @Username;";
            return await connection.ExecuteScalarAsync<string>(sql, new { Username = username });
        }

        public async Task<bool> AddAsync(string username, string employeeName, string hashedPassword, string role)
        {
            using var connection = GetConnection();
            string sql = "INSERT INTO Accounts (Username, EmployeeName, Password, Role) VALUES (@Username, @EmployeeName, @Password, @Role);";
            return await connection.ExecuteAsync(sql, new { Username = username, EmployeeName = employeeName, Password = hashedPassword, Role = role }) > 0;
        }

        public async Task<bool> UpdateAsync(string username, string? hashedPassword, string? role, string? employeeName)
        {
            var sets = new List<string>();
            var p = new DynamicParameters();
            p.Add("@Username", username);

            if (!string.IsNullOrWhiteSpace(hashedPassword))
            {
                sets.Add("Password=@Password");
                p.Add("@Password", hashedPassword);
            }
            if (!string.IsNullOrWhiteSpace(role))
            {
                sets.Add("Role=@Role");
                p.Add("@Role", role);
            }
            if (!string.IsNullOrWhiteSpace(employeeName))
            {
                sets.Add("EmployeeName=@EmployeeName");
                p.Add("@EmployeeName", employeeName);
            }

            if (sets.Count == 0) return true;

            using var connection = GetConnection();
            string sql = $"UPDATE Accounts SET {string.Join(", ", sets)} WHERE Username=@Username;";
            return await connection.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> UpdatePasswordAsync(string username, string newHashedPassword)
        {
            using var connection = GetConnection();
            string sql = "UPDATE Accounts SET Password=@Password WHERE Username=@Username;";
            return await connection.ExecuteAsync(sql, new { Password = newHashedPassword, Username = username }) > 0;
        }

        public async Task<bool> DeleteAsync(string username)
        {
            using var connection = GetConnection();
            string sql = "DELETE FROM Accounts WHERE Username=@Username;";
            return await connection.ExecuteAsync(sql, new { Username = username }) > 0;
        }
    }
}
