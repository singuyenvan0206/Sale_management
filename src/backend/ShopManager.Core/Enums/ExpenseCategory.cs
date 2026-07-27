namespace ShopManager.Core.Enums
{
    public enum ExpenseCategory
    {
        Rent,
        Utilities,
        Salary,
        Marketing,
        Other
    }

    public static class ExpenseCategoryExtensions
    {
        public static string ToDbString(this ExpenseCategory category) => category switch
        {
            ExpenseCategory.Rent => "Rent",
            ExpenseCategory.Utilities => "Utilities",
            ExpenseCategory.Salary => "Salary",
            ExpenseCategory.Marketing => "Marketing",
            ExpenseCategory.Other => "Other",
            _ => "Other"
        };

        public static ExpenseCategory ParseExpenseCategory(string? value) => value?.Trim().ToLowerInvariant() switch
        {
            "rent" => ExpenseCategory.Rent,
            "utilities" => ExpenseCategory.Utilities,
            "salary" => ExpenseCategory.Salary,
            "marketing" => ExpenseCategory.Marketing,
            _ => ExpenseCategory.Other
        };
    }
}
