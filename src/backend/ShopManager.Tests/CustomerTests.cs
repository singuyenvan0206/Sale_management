using Xunit;
using ShopManager.Core.Models;
using ShopManager.Core.Enums;

namespace ShopManager.Tests
{
    public class CustomerTests
    {
        [Theory]
        [InlineData(CustomerType.Regular, "Regular")]
        [InlineData(CustomerType.Silver, "Silver")]
        [InlineData(CustomerType.Gold, "Gold")]
        [InlineData(CustomerType.VIP, "VIP")]
        public void CustomerType_ToDbString_ReturnsExpectedString(CustomerType customerType, string expected)
        {
            Assert.Equal(expected, customerType.ToDbString());
        }

        [Theory]
        [InlineData("regular", CustomerType.Regular)]
        [InlineData("Silver", CustomerType.Silver)]
        [InlineData("GOLD", CustomerType.Gold)]
        [InlineData("vip", CustomerType.VIP)]
        [InlineData("invalid", CustomerType.Regular)]
        [InlineData(null, CustomerType.Regular)]
        public void CustomerType_ParseCustomerType_ReturnsExpectedEnum(string? input, CustomerType expected)
        {
            Assert.Equal(expected, CustomerTypeExtensions.ParseCustomerType(input));
        }

        [Fact]
        public void TierSettings_DefaultValues_AreCorrect()
        {
            var settings = new TierSettings();
            Assert.Equal(10000m, settings.SpendPerPoint);
            Assert.Equal(500, settings.SilverMinPoints);
            Assert.Equal(1000, settings.GoldMinPoints);
            Assert.Equal(2000, settings.VIPMinPoints);
        }

        [Fact]
        public void TierSettings_DetermineTierByPoints_WorksCorrectly()
        {
            var settings = new TierSettings();
            Assert.Equal("Regular", settings.DetermineTierByPoints(0));
            Assert.Equal("Silver", settings.DetermineTierByPoints(500));
            Assert.Equal("Gold", settings.DetermineTierByPoints(1000));
            Assert.Equal("VIP", settings.DetermineTierByPoints(2000));
        }

        [Fact]
        public void Customer_DefaultValues_AreCorrect()
        {
            var customer = new Customer();
            Assert.Equal(CustomerType.Regular.ToDbString(), customer.CustomerType);
            Assert.Equal(0, customer.Points);
            Assert.Equal(0m, customer.TotalSpent);
        }
    }
}
