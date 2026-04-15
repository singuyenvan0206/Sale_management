using Xunit;
using FashionStore.Services;
using FashionStore.Core.Models;

namespace FashionStore.Tests
{
    public class CalculationServiceTests
    {
        private readonly CalculationService _service;

        public CalculationServiceTests()
        {
            _service = new CalculationService();
        }

        [Fact]
        public void ApplyPercentDiscount_ShouldCalculateCorrectly()
        {
            // Arrange
            decimal price = 1000;
            decimal percent = 10;

            // Act
            decimal result = _service.ApplyPercentDiscount(price, percent);

            // Assert
            Assert.Equal(900, result);
        }

        [Fact]
        public void ApplyPercentDiscount_When100Percent_ShouldBeZero()
        {
            // Act
            decimal result = _service.ApplyPercentDiscount(1000, 100);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateVoucherValue_WithMaxDiscount_ShouldCap()
        {
            // Arrange
            var voucher = new Voucher
            {
                DiscountType = "%",
                DiscountValue = 50,
                MaxDiscountAmount = 200
            };
            decimal subtotal = 1000; // 50% of 1000 is 500, but capped at 200

            // Act
            decimal result = _service.CalculateVoucherValue(subtotal, voucher);

            // Assert
            Assert.Equal(200, result);
        }

        [Fact]
        public void CalculateTaxAmount_ShouldCalculateOnDiscountedPrice()
        {
            // Arrange
            decimal lineTotal = 1000;
            decimal discountRatio = 0.2m; // 20% discount
            decimal taxPercent = 10;      // 10% tax on 800

            // Act
            decimal result = _service.CalculateTaxAmount(lineTotal, discountRatio, taxPercent);

            // Assert
            Assert.Equal(80, result);
        }
    }
}
