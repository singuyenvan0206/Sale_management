using System;
using FashionStore.Models;

namespace FashionStore.Services
{
    public interface ICalculationService
    {
        decimal ApplyPercentDiscount(decimal price, decimal percent);
        decimal CalculateDiscount(decimal amount, string mode, decimal value);
        decimal CalculateTierDiscount(decimal amount, decimal tierDiscountPercent);
        decimal CalculateVoucherValue(decimal subtotal, Voucher voucher);
        decimal CalculateTaxAmount(decimal lineTotal, decimal discountRatio, decimal categoryTaxPercent);
    }
}
