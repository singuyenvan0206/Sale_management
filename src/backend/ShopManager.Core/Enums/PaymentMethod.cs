namespace ShopManager.Core.Enums
{
    public enum PaymentMethod
    {
        Cash,
        Banking,
        QR,
        Card,
        Transfer
    }

    public static class PaymentMethodExtensions
    {
        public static string ToDbString(this PaymentMethod method) => method switch
        {
            PaymentMethod.Cash => "Cash",
            PaymentMethod.Banking => "Banking",
            PaymentMethod.QR => "QR",
            PaymentMethod.Card => "Card",
            PaymentMethod.Transfer => "Transfer",
            _ => "Cash"
        };

        public static PaymentMethod ParsePaymentMethod(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "banking" => PaymentMethod.Banking,
            "qr" or "vietqr" => PaymentMethod.QR,
            "card" => PaymentMethod.Card,
            "transfer" => PaymentMethod.Transfer,
            _ => PaymentMethod.Cash
        };
    }
}
