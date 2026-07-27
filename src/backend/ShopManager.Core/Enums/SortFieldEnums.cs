namespace ShopManager.Core.Enums
{
    /// <summary>
    /// Sort field keys for Customer queries (used in repositories and view models).
    /// Values correspond to the URL/UI sort parameter strings.
    /// </summary>
    public static class CustomerSortField
    {
        public const string Id     = "id";
        public const string Name   = "name";
        public const string Spent  = "spent";
        public const string Points = "points";
        public const string Type   = "type";
        public const string Phone  = "phone";
        public const string Email  = "email";
        public const string Address = "address";
        public const string Tier   = "tier";

        // DB column mappings
        public static string ToDbColumn(string field) => field?.ToLower() switch
        {
            Id      => "Id",
            Name    => "Name",
            Spent   => "TotalSpent",
            Points  => "Points",
            Type    => "CustomerType",
            _       => "Id"
        };
    }

    /// <summary>
    /// Sort field keys for Product queries (used in repositories and view models).
    /// Values correspond to the URL/UI sort parameter strings.
    /// </summary>
    public static class ProductSortField
    {
        public const string Id            = "id";
        public const string Name          = "name";
        public const string Price         = "price";
        public const string Stock         = "stock";
        public const string Code          = "code";
        public const string CategoryName  = "categoryname";
        public const string SalePrice     = "saleprice";
        public const string StockQuantity = "stockquantity";

        // DB column mappings (with table alias prefix "p.")
        public static string ToDbColumn(string field) => field?.ToLower() switch
        {
            Id    => "p.Id",
            Name  => "p.Name",
            Price => "p.SalePrice",
            Stock => "p.StockQuantity",
            Code  => "p.Code",
            _     => "p.Id"
        };
    }
}
