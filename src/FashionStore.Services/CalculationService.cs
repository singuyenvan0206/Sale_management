using FashionStore.Core.Models;

namespace FashionStore.Services
{
    public class CalculationService : ICalculationService
    {
        public decimal ApplyPercentDiscount(decimal price, decimal percent)
        {
            if (percent <= 0) return price;
            if (percent >= 100) return 0m;
            return Math.Round(price * (1 - (percent / 100m)), 2);
        }

        public decimal CalculateDiscount(decimal amount, string mode, decimal value)
        {
            if (mode == "%")
            {
                return Math.Round(amount * (value / 100m), 2);
            }
            return value;
        }

        public decimal CalculateTierDiscount(decimal amount, decimal tierDiscountPercent)
        {
            return Math.Round(amount * (tierDiscountPercent / 100m), 2);
        }

        public decimal CalculateVoucherValue(decimal subtotal, Voucher voucher)
        {
            if (voucher == null) return 0m;

            decimal discount;
            if (voucher.DiscountType == Voucher.TypePercentage || voucher.DiscountType == "%")
            {
                discount = Math.Round(subtotal * (voucher.DiscountValue / 100m), 2);
            }
            else
            {
                discount = voucher.DiscountValue;
            }

            if (voucher.MaxDiscountAmount > 0)
            {
                discount = Math.Min(discount, voucher.MaxDiscountAmount);
            }

            return discount;
        }

        public decimal CalculateTaxAmount(decimal lineTotal, decimal discountRatio, decimal categoryTaxPercent)
        {
            return (lineTotal * (1 - discountRatio)) * (categoryTaxPercent / 100m);
        }
    }
}
