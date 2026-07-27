using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ShopManager.Core.Interfaces;
using ShopManager.Core.Models;

namespace ShopManager.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Account user)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"] ?? "ShopManagerERP_Super_Secret_Security_Key_2026!";
            var issuer = _configuration["JwtSettings:Issuer"] ?? "ShopManagerWebAPI";
            var audience = _configuration["JwtSettings:Audience"] ?? "ShopManagerClients";
            var expiryMinutes = int.TryParse(_configuration["JwtSettings:ExpiryMinutes"], out var minutes) ? minutes : 480;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, string.IsNullOrEmpty(user.EmployeeName) ? user.Username : user.EmployeeName),
                new Claim(ClaimTypes.Role, user.Role ?? "Cashier"),
                new Claim("UserRole", user.Role ?? "Cashier")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            var secretKey = _configuration["JwtSettings:SecretKey"] ?? "ShopManagerERP_Super_Secret_Security_Key_2026!";
            var issuer = _configuration["JwtSettings:Issuer"] ?? "ShopManagerWebAPI";
            var audience = _configuration["JwtSettings:Audience"] ?? "ShopManagerClients";

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
