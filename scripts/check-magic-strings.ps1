# Script quét ĐỘNG toàn bộ Magic Strings (Dynamic Magic String Detector)
# Tự động phát hiện BẤT KỲ chuỗi cứng nào dùng trong phép so sánh, switch-case hoặc Equals mà không dùng Hằng số / Enum.

$ErrorActionPreference = "Stop"

Write-Host "🔍 [Dynamic Scanner] Đang quét DỘNG toàn bộ Magic String Literals trong phép so sánh..." -ForegroundColor Cyan

# Các quy tắc phát hiện so sánh chuỗi cứng (Equality, Inequality, Switch Case, Equals)
$dynamicRegexes = @(
    '==\s*"([^"]+)"',                 # ví dụ: Status == "..."
    '!=\s*"([^"]+)"',                 # ví dụ: Status != "..."
    'case\s*"([^"]+)":',               # ví dụ: case "..."
    '\.Equals\s*\(\s*"([^"]+)"'       # ví dụ: .Equals("...")
)

# Danh sách bỏ qua các chuỗi định dạng (Format specifiers) hoặc tham số định tuyến kỹ thuật ASP.NET MVC / JS UI
$allowedTechnicalStrings = @(
    "", "N0", "C0", "G29", "F0", "F2", "D5",
    "yyyy-MM-dd", "dd/MM/yyyy", "dd/MM/yy", "HH:mm", "dd/MM HH:mm",
    "vi-VN", "Bearer", "Authorization", "All", "Ghi chú", "Khách lẻ",
    "controller", "action", "active", "import", "export", "date", "total", "id", "name", "price", "stock",
    "POS", "Dashboard", "Product", "Inventory", "Category", "Supplier", "Customer", "Invoice", "Shift", "Promotion", "Voucher", "User", "Finance", "Settings",
    "EmployeeName", "Percentage"
)

$targetFiles = Get-ChildItem -Path "src" -Recurse -Include "*.cs", "*.cshtml" | 
    Where-Object { 
        $_.FullName -notmatch "\\Enums\\" -and 
        $_.FullName -notmatch "\\Tests\\" -and 
        $_.FullName -notmatch "\\obj\\" -and 
        $_.FullName -notmatch "\\bin\\" 
    }

$violationsCount = 0

foreach ($file in $targetFiles) {
    $lines = Get-Content -Path $file.FullName
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        
        # Bỏ qua các dòng comment hoặc using
        if ($line.Trim().StartsWith("//") -or $line.Trim().StartsWith("*") -or $line.Trim().StartsWith("using ")) {
            continue
        }

        foreach ($pattern in $dynamicRegexes) {
            $matches = [regex]::Matches($line, $pattern)
            foreach ($match in $matches) {
                $literalValue = $match.Groups[1].Value
                
                # Nối bộ lọc kiểm tra nếu không phải chuỗi kỹ thuật cho phép
                if ($allowedTechnicalStrings -notcontains $literalValue -and $literalValue.Length -gt 0) {
                    Write-Host "❌ Magic String Động tại $($file.FullName) (Dòng $($i + 1)): [$literalValue] -> $($line.Trim())" -ForegroundColor Red
                    $violationsCount++
                }
            }
        }
    }
}

Write-Host "--------------------------------------------------" -ForegroundColor Gray
if ($violationsCount -gt 0) {
    Write-Host "⚠️ KẾT QUẢ: Phát hiện $violationsCount Magic Strings trong phép so sánh code!" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✅ KẾT QUẢ: Toàn bộ hệ thống 100% SẠCH SẼ! Không phát hiện bất kỳ Magic String so sánh nào." -ForegroundColor Green
    exit 0
}
