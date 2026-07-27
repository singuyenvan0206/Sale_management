using Xunit;
using ShopManager.Core.Enums;
using ShopManager.Core.Models;

namespace ShopManager.Tests
{
    public class PromotionAndStockTests
    {
        [Theory]
        [InlineData(PromotionType.FlashSale, "FlashSale")]
        [InlineData(PromotionType.BOGO, "BOGO")]
        [InlineData(PromotionType.Combo, "Combo")]
        public void PromotionType_ToDbString_ReturnsExpectedString(PromotionType type, string expected)
        {
            Assert.Equal(expected, type.ToDbString());
        }

        [Theory]
        [InlineData("flashsale", PromotionType.FlashSale)]
        [InlineData("BOGO", PromotionType.BOGO)]
        [InlineData("combo", PromotionType.Combo)]
        [InlineData("invalid", PromotionType.FlashSale)]
        [InlineData(null, PromotionType.FlashSale)]
        public void PromotionType_ParsePromotionType_ReturnsExpectedEnum(string? input, PromotionType expected)
        {
            Assert.Equal(expected, PromotionTypeExtensions.ParsePromotionType(input));
        }

        [Theory]
        [InlineData(StockMovementType.Import, "Import")]
        [InlineData(StockMovementType.Export, "Export")]
        [InlineData(StockMovementType.Sale, "Sale")]
        [InlineData(StockMovementType.Adjustment, "Adjustment")]
        [InlineData(StockMovementType.Return, "Return")]
        [InlineData(StockMovementType.Transfer, "Transfer")]
        public void StockMovementType_ToDbString_ReturnsExpectedString(StockMovementType movement, string expected)
        {
            Assert.Equal(expected, movement.ToDbString());
        }

        [Theory]
        [InlineData("import", StockMovementType.Import)]
        [InlineData("Export", StockMovementType.Export)]
        [InlineData("sale", StockMovementType.Sale)]
        [InlineData("ADJUSTMENT", StockMovementType.Adjustment)]
        [InlineData("return", StockMovementType.Return)]
        [InlineData("transfer", StockMovementType.Transfer)]
        [InlineData("invalid", StockMovementType.Import)]
        [InlineData(null, StockMovementType.Import)]
        public void StockMovementType_ParseStockMovementType_ReturnsExpectedEnum(string? input, StockMovementType expected)
        {
            Assert.Equal(expected, StockMovementTypeExtensions.ParseStockMovementType(input));
        }

        [Theory]
        [InlineData(PurchaseOrderStatus.Draft, "Draft")]
        [InlineData(PurchaseOrderStatus.Pending, "Pending")]
        [InlineData(PurchaseOrderStatus.Received, "Received")]
        [InlineData(PurchaseOrderStatus.Cancelled, "Cancelled")]
        public void PurchaseOrderStatus_ToDbString_ReturnsExpectedString(PurchaseOrderStatus status, string expected)
        {
            Assert.Equal(expected, status.ToDbString());
        }

        [Theory]
        [InlineData("draft", PurchaseOrderStatus.Draft)]
        [InlineData("Pending", PurchaseOrderStatus.Pending)]
        [InlineData("RECEIVED", PurchaseOrderStatus.Received)]
        [InlineData("cancelled", PurchaseOrderStatus.Cancelled)]
        [InlineData("invalid", PurchaseOrderStatus.Draft)]
        [InlineData(null, PurchaseOrderStatus.Draft)]
        public void PurchaseOrderStatus_ParsePurchaseOrderStatus_ReturnsExpectedEnum(string? input, PurchaseOrderStatus expected)
        {
            Assert.Equal(expected, PurchaseOrderStatusExtensions.ParsePurchaseOrderStatus(input));
        }
    }
}
