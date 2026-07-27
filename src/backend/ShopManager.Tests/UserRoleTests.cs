using Xunit;
using ShopManager.Core.Models;

namespace ShopManager.Tests
{
    public class UserRoleTests
    {
        [Theory]
        [InlineData(UserRole.Admin, "Admin")]
        [InlineData(UserRole.Manager, "Manager")]
        [InlineData(UserRole.Cashier, "Cashier")]
        public void UserRole_ToRoleString_ReturnsExpectedString(UserRole role, string expected)
        {
            Assert.Equal(expected, role.ToRoleString());
        }

        [Theory]
        [InlineData("admin", UserRole.Admin)]
        [InlineData("Manager", UserRole.Manager)]
        [InlineData("CASHIER", UserRole.Cashier)]
        [InlineData("invalid", UserRole.Cashier)]
        [InlineData(null, UserRole.Cashier)]
        public void UserRole_ParseUserRole_ReturnsExpectedEnum(string? input, UserRole expected)
        {
            Assert.Equal(expected, UserRoleExtensions.ParseUserRole(input));
        }

        [Fact]
        public void Account_DefaultRole_IsCashier()
        {
            var account = new Account();
            Assert.Equal(UserRole.Cashier.ToString(), account.Role);
        }
    }
}
