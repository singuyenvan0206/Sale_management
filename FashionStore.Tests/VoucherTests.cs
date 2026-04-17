using System;
using Xunit;
using Xunit;
using FashionStore.Core.Models;

namespace FashionStore.Tests
{
    public class VoucherTests
    {
        [Fact]
        public void IsValid_InactiveVoucher_ReturnsFalse()
        {
            var voucher = new Voucher { IsActive = false, StartDate = DateTime.Today.AddDays(-1), EndDate = DateTime.Today.AddDays(1) };
            Assert.False(voucher.IsValid());
        }

        [Fact]
        public void IsValid_ExpiredVoucher_ReturnsFalse()
        {
            var voucher = new Voucher { IsActive = true, StartDate = DateTime.Today.AddDays(-10), EndDate = DateTime.Today.AddDays(-1) };
            Assert.False(voucher.IsValid());
        }

        [Fact]
        public void IsValid_UpcomingVoucher_ReturnsFalse()
        {
            var voucher = new Voucher { IsActive = true, StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(10) };
            Assert.False(voucher.IsValid());
        }

        [Fact]
        public void IsValid_UsageLimitReached_ReturnsFalse()
        {
            var voucher = new Voucher 
            { 
                IsActive = true, 
                StartDate = DateTime.Today.AddDays(-1), 
                EndDate = DateTime.Today.AddDays(1),
                UsageLimit = 5,
                UsedCount = 5
            };
            Assert.False(voucher.IsValid());
        }

        [Fact]
        public void IsValid_MinInvoiceAmountNotMet_ReturnsFalse()
        {
            var voucher = new Voucher 
            { 
                IsActive = true, 
                StartDate = DateTime.Today.AddDays(-1), 
                EndDate = DateTime.Today.AddDays(1),
                MinInvoiceAmount = 100000
            };
            Assert.False(voucher.IsValid(subtotal: 50000));
        }

        [Fact]
        public void IsValid_CustomerUsageLimitReached_ReturnsFalse()
        {
            var voucher = new Voucher 
            { 
                IsActive = true, 
                StartDate = DateTime.Today.AddDays(-1), 
                EndDate = DateTime.Today.AddDays(1),
                UsageLimitPerCustomer = 1
            };
            Assert.False(voucher.IsValid(currentCustomerUsage: 1));
        }

        [Fact]
        public void IsValid_ValidVoucher_ReturnsTrue()
        {
            var voucher = new Voucher 
            { 
                IsActive = true, 
                StartDate = DateTime.Today.AddDays(-1), 
                EndDate = DateTime.Today.AddDays(1),
                UsageLimit = 10,
                UsedCount = 2,
                MinInvoiceAmount = 50000
            };
            Assert.True(voucher.IsValid(subtotal: 100000, currentCustomerUsage: 0));
        }

        [Theory]
        [InlineData("%", Voucher.TypePercentage)]
        [InlineData("VND", Voucher.TypeFixedAmount)]
        [InlineData("Percentage", Voucher.TypePercentage)]
        [InlineData("FixedAmount", Voucher.TypeFixedAmount)]
        public void DiscountType_Normalization_Works(string input, string expected)
        {
            var voucher = new Voucher { DiscountType = input };
            Assert.Equal(expected, voucher.DiscountType);
        }
    }
}
