using Xunit;
using ShopManager.Core.Enums;
using ShopManager.Core.DTOs;

namespace ShopManager.Tests
{
    public class PaymentTests
    {
        [Theory]
        [InlineData(PaymentMethod.Cash, "Cash")]
        [InlineData(PaymentMethod.Banking, "Banking")]
        [InlineData(PaymentMethod.QR, "QR")]
        [InlineData(PaymentMethod.Card, "Card")]
        [InlineData(PaymentMethod.Transfer, "Transfer")]
        public void PaymentMethod_ToDbString_ReturnsExpectedString(PaymentMethod method, string expected)
        {
            Assert.Equal(expected, method.ToDbString());
        }

        [Theory]
        [InlineData("cash", PaymentMethod.Cash)]
        [InlineData("Banking", PaymentMethod.Banking)]
        [InlineData("qr", PaymentMethod.QR)]
        [InlineData("CARD", PaymentMethod.Card)]
        [InlineData("transfer", PaymentMethod.Transfer)]
        [InlineData("invalid", PaymentMethod.Cash)]
        [InlineData(null, PaymentMethod.Cash)]
        public void PaymentMethod_ParsePaymentMethod_ReturnsExpectedEnum(string? input, PaymentMethod expected)
        {
            Assert.Equal(expected, PaymentMethodExtensions.ParsePaymentMethod(input));
        }

        [Fact]
        public void CheckoutApiRequest_Properties_SetCorrectly()
        {
            var req = new CheckoutApiRequest
            {
                CustomerId = 1001,
                EmployeeId = 2,
                PaymentMethod = PaymentMethod.QR.ToDbString(),
                PaidAmount = 250000m,
                Note = "Thanh toán QR VietQR"
            };

            Assert.Equal(1001, req.CustomerId);
            Assert.Equal(2, req.EmployeeId);
            Assert.Equal("QR", req.PaymentMethod);
            Assert.Equal(250000m, req.PaidAmount);
        }

        [Fact]
        public void CalculateDiscountResponse_Properties_SetCorrectly()
        {
            var res = new CalculateDiscountResponse
            {
                Subtotal = 1000000m,
                MemberDiscount = 50000m,
                VoucherDiscount = 100000m,
                PromotionDiscount = 0m,
                TotalDiscount = 150000m,
                FinalTotal = 850000m,
                MemberTier = "Gold",
                Message = "Áp dụng thành công"
            };

            Assert.Equal(1000000m, res.Subtotal);
            Assert.Equal(150000m, res.TotalDiscount);
            Assert.Equal(850000m, res.FinalTotal);
        }
    }
}
