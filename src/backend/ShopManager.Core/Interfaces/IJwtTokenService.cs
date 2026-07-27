using ShopManager.Core.Models;

namespace ShopManager.Core.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(Account user);
        bool ValidateToken(string token);
    }
}
