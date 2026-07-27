namespace ShopManager.App.Constants
{
    /// <summary>
    /// Centralised UI string constants for tab names and confirmation prompts.
    /// Prevents magic strings from appearing in code-behind comparisons.
    /// </summary>
    public static class UiStrings
    {
        // ── Dashboard tab identifiers ──────────────────────────────────────────
        public const string HomeTabVi     = "🏠 Trang Chủ";
        public const string HomeTabEn     = "Home";
        public const string LogoutTabVi   = "🚪 Đăng Xuất";
        public const string LogoutTabEn   = "Logout";

        // ── Inventory / Web tab route keys ────────────────────────────────────
        public const string TabHistory = "history";
        public const string TabPo      = "po";
        public const string TabImport  = "import";

        // ── Confirmation prompts ──────────────────────────────────────────────
        public const string DeleteAllConfirmation = "DELETE ALL";

        // ── Login result ──────────────────────────────────────────────────────
        public const string LoginSuccess = "true";
    }
}
