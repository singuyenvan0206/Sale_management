using System;
using Xunit;
using FashionStore.Services;
using FashionStore.Core.Models;

namespace FashionStore.Tests
{
    public class CalculationTests
    {
        private readonly ICalculationService _service = new CalculationService();

        [Theory]
        [InlineData(1000, 10, 900)]
        [InlineData(1000, 0, 1000)]
        [InlineData(1000, 100, 0)]
        [InlineData(1000, 150, 0)] // Cap at 100%
        public void ApplyPercentDiscount_Works(decimal price, decimal percent, decimal expected)
        {
            var result = _service.ApplyPercentDiscount(price, percent);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateDiscount_PercentageMode_Works()
        {
            var result = _service.CalculateDiscount(1000000, "%", 10);
            Assert.Equal(100000, result);
        }

        [Fact]
        public void CalculateDiscount_FixedAmountMode_Works()
        {
            var result = _service.CalculateDiscount(1000000, "VND", 50000);
            Assert.Equal(50000, result);
        }

        [Fact]
        public void CalculateTierDiscount_Works()
        {
            var result = _service.CalculateTierDiscount(1000000, 5); // 5% tier discount
            Assert.Equal(50000, result);
        }

        [Fact]
        public void CalculateVoucherValue_CappedAtMaxDiscount()
        {
            var voucher = new Voucher 
            { 
                DiscountType = Voucher.TypePercentage, 
                DiscountValue = 50, 
                MaxDiscountAmount = 10000 
            };
            var result = _service.CalculateVoucherValue(100000, voucher);
            Assert.Equal(10000, result); // 50% of 100k is 50k, but capped at 10k
        }

        [Fact]
        public void CalculateTaxAmount_ShouldApplyDiscountCorrectly()
        {
            // Arrange
            decimal lineTotal = 100000;
            decimal discountRatio = 0.1m; // 10% discount
            decimal taxPercent = 10;      // 10% tax

            // Act
            // Formula: (100k * 0.9) * 0.1 = 9k
            var result = _service.CalculateTaxAmount(lineTotal, discountRatio, taxPercent);

            // Assert
            Assert.Equal(9000, result);
        }

        [Fact]
        public void Integrated_TierAndVoucher_ShouldWork()
        {
            // Arrange
            decimal subtotal = 1000000; // 1M
            string tier = "Gold"; // 10%
            var voucher = new Voucher { DiscountType = "%", DiscountValue = 5, MaxDiscountAmount = 100000 };

            // Act
            decimal tierDiscPercent = _service.GetTierDiscountPercent(tier);
            decimal tierDiscAmount = _service.CalculateTierDiscount(subtotal, tierDiscPercent);
            
            decimal remaining = subtotal - tierDiscAmount; // 930k (1M - 7%)
            decimal voucherDisc = _service.CalculateVoucherValue(remaining, voucher); // 5% of 930k = 46.5k

            // Assert
            Assert.Equal(7, tierDiscPercent);
            Assert.Equal(70000, tierDiscAmount);
            Assert.Equal(46500, voucherDisc);
            Assert.Equal(883500, subtotal - tierDiscAmount - voucherDisc);
        }

        [Fact]
        public void ApplyPercentDiscount_CapTest()
        {
            Assert.Equal(0, _service.ApplyPercentDiscount(100, 100));
            Assert.Equal(0, _service.ApplyPercentDiscount(100, 150));
            Assert.Equal(100, _service.ApplyPercentDiscount(100, -10));
        }
    }
}
