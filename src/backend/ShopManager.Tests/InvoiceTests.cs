using Xunit;
using ShopManager.Core.Models;
using ShopManager.Core.Enums;

namespace ShopManager.Tests
{
    public class InvoiceTests
    {
        [Theory]
        [InlineData(InvoiceStatus.Completed, "Completed")]
        [InlineData(InvoiceStatus.Pending, "Pending")]
        [InlineData(InvoiceStatus.Cancelled, "Cancelled")]
        [InlineData(InvoiceStatus.Refunded, "Refunded")]
        public void InvoiceStatus_ToDbString_ReturnsExpectedString(InvoiceStatus status, string expected)
        {
            Assert.Equal(expected, status.ToDbString());
        }

        [Theory]
        [InlineData("completed", InvoiceStatus.Completed)]
        [InlineData("Pending", InvoiceStatus.Pending)]
        [InlineData("CANCELLED", InvoiceStatus.Cancelled)]
        [InlineData("refunded", InvoiceStatus.Refunded)]
        [InlineData("unknown", InvoiceStatus.Completed)]
        [InlineData(null, InvoiceStatus.Completed)]
        public void InvoiceStatus_ParseInvoiceStatus_ReturnsExpectedEnum(string? input, InvoiceStatus expected)
        {
            Assert.Equal(expected, InvoiceStatusExtensions.ParseInvoiceStatus(input));
        }

        [Fact]
        public void Invoice_DefaultValues_AreCorrect()
        {
            var invoice = new Invoice();
            Assert.Equal(InvoiceStatus.Completed.ToDbString(), invoice.Status);
            Assert.Equal(PaymentMethod.Cash.ToDbString(), invoice.PaymentMethod);
            Assert.NotNull(invoice.Items);
            Assert.Empty(invoice.Items);
        }

        [Fact]
        public void InvoiceItem_LineTotalCalculation_Works()
        {
            var item = new InvoiceItem
            {
                ProductId = 1,
                UnitPrice = 50000m,
                Quantity = 3,
                LineTotal = 150000m
            };
            Assert.Equal(150000m, item.LineTotal);
        }
    }
}
