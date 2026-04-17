using Xunit;
using FashionStore.Services;
using FashionStore.Core.Models;
using System;

namespace FashionStore.Tests
{
    public class LoyaltyTests
    {
        private readonly ICalculationService _service = new CalculationService();

        [Theory]
        [InlineData("Silver", 3)]
        [InlineData("Gold", 7)]
        [InlineData("VIP", 10)]
        [InlineData("Regular", 0)]
        [InlineData("Unknown", 0)]
        [InlineData("", 0)]
        [InlineData(null, 0)]
        public void GetTierDiscountPercent_ShouldReturnCorrectValue(string tier, decimal expected)
        {
            // Act
            decimal result = _service.GetTierDiscountPercent(tier);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateTierDiscount_Math_ShouldBeCorrect()
        {
            // Arrange
            decimal amount = 1000000; // 1M
            decimal discountPercent = 10; // Gold

            // Act
            decimal result = _service.CalculateTierDiscount(amount, discountPercent);

            // Assert
            Assert.Equal(100000, result);
        }
    }
}
