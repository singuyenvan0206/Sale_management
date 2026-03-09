using System;
using Xunit;
using WpfApp1;

namespace WpfApp1.Tests
{
    public class CalculationTests
    {
        [Theory]
        [InlineData(1000, 10, 900)]
        [InlineData(1000, 0, 1000)]
        [InlineData(1000, 100, 0)]
        [InlineData(1000, 150, 0)] // Cap at 100%
        public void ApplyPercentDiscount_Works(decimal price, decimal percent, decimal expected)
        {
            var result = InvoiceManagementWindow.ApplyPercentDiscount(price, percent);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateDiscount_PercentageMode_Works()
        {
            var result = InvoiceManagementWindow.CalculateDiscount(1000000, "%", 10);
            Assert.Equal(100000, result);
        }

        [Fact]
        public void CalculateDiscount_FixedAmountMode_Works()
        {
            var result = InvoiceManagementWindow.CalculateDiscount(1000000, "VND", 50000);
            Assert.Equal(50000, result);
        }

        [Fact]
        public void CalculateTierDiscount_Works()
        {
            var result = InvoiceManagementWindow.CalculateTierDiscount(1000000, 5); // 5% tier discount
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
            var result = InvoiceManagementWindow.CalculateVoucherValue(100000, voucher);
            Assert.Equal(10000, result); // 50% of 100k is 50k, but capped at 10k
        }

        [Fact]
        public void CalculateVoucherValue_PercentageWithoutCap()
        {
            var voucher = new Voucher 
            { 
                DiscountType = Voucher.TypePercentage, 
                DiscountValue = 10 
            };
            var result = InvoiceManagementWindow.CalculateVoucherValue(100000, voucher);
            Assert.Equal(10000, result);
        }
    }
}
