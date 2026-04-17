using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using FashionStore.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public class ProductService : FashionStore.Core.Interfaces.IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICacheService _cacheService;
        private const string CacheKey = "AllProductsWithCategories";

        public ProductService(IProductRepository productRepository, ICacheService cacheService)
        {
            _productRepository = productRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<Product>> GetAllProductsWithCategoriesAsync()
        {
            var products = await _productRepository.GetAllWithCategoriesAsync();
            _cacheService.Set(CacheKey, products, TimeSpan.FromMinutes(5));
            return products;
        }

        public async Task<PaginatedList<Product>> GetPagedProductsAsync(int pageIndex, int pageSize)
        {
            var (items, totalCount) = await _productRepository.GetPagedWithCategoriesAsync(pageIndex, pageSize);
            return PaginatedList<Product>.Create(items, totalCount, pageIndex, pageSize);
        }

        public async Task<Product?> GetProductByCodeAsync(string code)
        {
            return await _productRepository.GetByCodeAsync(code);
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            bool success = await _productRepository.AddAsync(product);
            if (success) _cacheService.Remove(CacheKey);
            return success;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            bool success = await _productRepository.UpdateAsync(product);
            if (success) _cacheService.Remove(CacheKey);
            return success;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            bool success = await _productRepository.DeleteAsync(id);
            if (success) _cacheService.Remove(CacheKey);
            return success;
        }

        public async Task<int> GetProductStockQuantityAsync(int productId)
        {
            return await _productRepository.GetStockQuantityAsync(productId);
        }

        public async Task<IEnumerable<(string ProductName, int StockQuantity, string CategoryName)>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _productRepository.GetLowStockProductsAsync(threshold);
        }

        public async Task<bool> UpdateProductStockAsync(int productId, int delta)
        {
            // Use the atomic repository method to prevent race conditions during concurrent sales
            bool success = await _productRepository.AdjustStockAsync(productId, delta);
            if (success) _cacheService.Remove(CacheKey);
            return success;
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            // If GetAllWithCategoriesAsync returns all products, filter by id
            var products = await _productRepository.GetAllWithCategoriesAsync();
            return products.FirstOrDefault(p => p.Id == id);
        }

        public async Task<int> GetTotalProductsAsync()
        {
            return await _productRepository.GetTotalProductsAsync();
        }

        public async Task<bool> DeleteAllProductsAsync()
        {
            bool success = await _productRepository.DeleteAllProductsAsync();
            if (success) _cacheService.Remove(CacheKey);
            return success;
        }

        // Bridge for legacy static calls - SHOULD BE DEPRECATED
        private static IProductService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<IProductService>() ?? throw new InvalidOperationException("DI not initialized");

        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();

        public static List<(int Id, string Name, string Code, int CategoryId, string CategoryName, decimal SalePrice, decimal PromoDiscountPercent, DateTime? PromoStartDate, DateTime? PromoEndDate, decimal PurchasePrice, string PurchaseUnit, int ImportQuantity, int StockQuantity, string Description, decimal CategoryTaxPercent, int SupplierId, string SupplierName)> GetAllProductsWithCategories()
        {
            var products = RunSync(() => GetService().GetAllProductsWithCategoriesAsync());
            return products.Select(p => (
                p.Id, p.Name, p.Code, p.CategoryId, p.CategoryName, p.SalePrice, p.PromoDiscountPercent,
                p.PromoStartDate, p.PromoEndDate, p.PurchasePrice, p.PurchaseUnit, p.ImportQuantity,
                p.StockQuantity, p.Description, p.CategoryTaxPercent, p.SupplierId, p.SupplierName
            )).ToList();
        }

        public static bool AddProduct(string name, string code, int categoryId, decimal salePrice, decimal purchasePrice, string purchaseUnit, int importQuantity, int stockQuantity, string description = "", decimal promoDiscountPercent = 0m, DateTime? promoStartDate = null, DateTime? promoEndDate = null, int supplierId = 0)
            => RunSync(() => GetService().AddProductAsync(new Product
            {
                Name = name,
                Code = code,
                CategoryId = categoryId,
                SalePrice = salePrice,
                PurchasePrice = purchasePrice,
                PurchaseUnit = purchaseUnit,
                ImportQuantity = importQuantity,
                StockQuantity = stockQuantity,
                Description = description,
                PromoDiscountPercent = promoDiscountPercent,
                PromoStartDate = promoStartDate,
                PromoEndDate = promoEndDate,
                SupplierId = supplierId
            }));

        public static bool UpdateProduct(int id, string name, string code, int categoryId, decimal salePrice, decimal purchasePrice, string purchaseUnit, int importQuantity, int stockQuantity, string description = "", decimal promoDiscountPercent = 0m, DateTime? promoStartDate = null, DateTime? promoEndDate = null, int supplierId = 0)
            => RunSync(() => GetService().UpdateProductAsync(new Product
            {
                Id = id,
                Name = name,
                Code = code,
                CategoryId = categoryId,
                SalePrice = salePrice,
                PurchasePrice = purchasePrice,
                PurchaseUnit = purchaseUnit,
                ImportQuantity = importQuantity,
                StockQuantity = stockQuantity,
                Description = description,
                PromoDiscountPercent = promoDiscountPercent,
                PromoStartDate = promoStartDate,
                PromoEndDate = promoEndDate,
                SupplierId = supplierId
            }));

        public static int GetProductStockQuantity(int productId) => RunSync(() => GetService().GetProductStockQuantityAsync(productId));
        public static bool DeleteProduct(int id) => RunSync(() => GetService().DeleteProductAsync(id));
        public static bool DeleteAllProducts() => RunSync(() => GetService().DeleteAllProductsAsync());
        public static int GetTotalProducts() => RunSync(() => GetService().GetTotalProductsAsync());
        public static List<(string ProductName, int StockQuantity, string CategoryName)> GetLowStockProducts(int threshold = 10) => RunSync(() => GetService().GetLowStockProductsAsync(threshold)).ToList();
        // Add this method to the IProductService interface

        public async Task<IEnumerable<(string ProductName, int Quantity, decimal Revenue)>> GetTopProductsAsync(int topN = 10) => await _productRepository.GetTopProductsAsync(topN);
        public static List<(string ProductName, int Quantity, decimal Revenue)> GetTopProducts(int topN = 10) => RunSync(() => GetService().GetTopProductsAsync(topN)).ToList();

        public async Task<bool> DoesProductIdExistAsync(int productId) => await _productRepository.DoesProductIdExistAsync(productId);
        public static bool DoesProductIdExist(int productId) => RunSync(() => GetService().DoesProductIdExistAsync(productId));

        public async Task<int> FindProductIdByNameAsync(string productName) => await _productRepository.FindProductIdByNameAsync(productName);
        public static int FindProductIdByName(string productName) => RunSync(() => GetService().FindProductIdByNameAsync(productName));

        // Variant Support Implementation
        public async Task<IEnumerable<ProductVariant>> GetProductVariantsAsync(int productId)
        {
            return await _productRepository.GetVariantsAsync(productId);
        }

        public async Task<ProductVariant?> GetVariantByBarcodeAsync(string barcode)
        {
            return await _productRepository.GetVariantByBarcodeAsync(barcode);
        }

        public async Task<ProductVariant?> GetVariantByIdAsync(int variantId)
        {
            return await _productRepository.GetVariantByIdAsync(variantId);
        }

        public async Task<bool> AddVariantAsync(ProductVariant variant)
        {
            bool success = await _productRepository.AddVariantAsync(variant);
            if (success) _cacheService.Remove(CacheKey);
            return success;
        }

        public async Task<bool> UpdateVariantAsync(ProductVariant variant)
        {
            bool success = await _productRepository.UpdateVariantAsync(variant);
            if (success) _cacheService.Remove(CacheKey);
            return success;
        }

        public async Task<bool> DeleteVariantAsync(int variantId)
        {
            bool success = await _productRepository.DeleteVariantAsync(variantId);
            if (success) _cacheService.Remove(CacheKey);
            return success;
        }

        public async Task<bool> UpdateVariantStockAsync(int variantId, int delta)
        {
            // Use atomic adjustment for variants too
            bool success = await _productRepository.AdjustVariantStockAsync(variantId, delta);
            if (success) _cacheService.Remove(CacheKey);
            return success;
        }

        public static Product? GetProductByCode(string code) => RunSync(() => GetService().GetProductByCodeAsync(code));

        public static bool UpdateProductStock(int id, int qty) => RunSync(() => GetService().UpdateProductStockAsync(id, qty));
        public static Product? GetProductById(int id) => RunSync(() => GetService().GetProductByIdAsync(id));

        public static int ImportProductsFromCsv(string filePath) => RunSync(() => GetService().ImportProductsFromCsvAsync(filePath));
        public static bool ExportProductsToCsv(string filePath) => RunSync(() => GetService().ExportProductsToCsvAsync(filePath));

        public async Task<bool> ExportProductsToCsvAsync(string filePath)
        {
            try
            {
                var products = await _productRepository.GetAllWithCategoriesAsync();
                var lines = new List<string> { "Mã,Tên,Danh mục,Giá bán,Giá nhập,Đơn vị,Số lượng,Tồn kho,Mô tả,Giảm giá,Bắt đầu KM,Kết thúc KM,Nhà cung cấp" };
                foreach (var p in products)
                {
                    lines.Add($"{CsvHelper.Escape(p.Code)},{CsvHelper.Escape(p.Name)},{CsvHelper.Escape(p.CategoryName)},{p.SalePrice:F0},{p.PurchasePrice:F0},{CsvHelper.Escape(p.PurchaseUnit)},{p.ImportQuantity},{p.StockQuantity},{CsvHelper.Escape(p.Description)},{p.PromoDiscountPercent:F0},{p.PromoStartDate:dd/MM/yyyy},{p.PromoEndDate:dd/MM/yyyy},{CsvHelper.Escape(p.SupplierName)}");
                }

                await File.WriteAllLinesAsync(filePath, lines, System.Text.Encoding.UTF8);
                return true;
            }
            catch { return false; }
        }

        public async Task<int> ImportProductsFromCsvAsync(string filePath)
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(filePath, System.Text.Encoding.UTF8);
                if (lines.Length < 2) return 0;

                var categories = (await _productRepository.GetAllWithCategoriesAsync())
                    .Select(p => new { p.CategoryId, p.CategoryName })
                    .Distinct()
                    .ToDictionary(x => x.CategoryName ?? "", x => x.CategoryId);

                int count = 0;
                foreach (var line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = CsvHelper.ParseLine(line);
                    if (cols.Length < 8) continue;

                    string code = cols[0].Trim();
                    string name = cols[1].Trim();
                    string catName = cols[2].Trim();
                    
                    if (!categories.TryGetValue(catName, out int catId)) catId = 1; // Default category

                    var product = new Product
                    {
                        Code = code,
                        Name = name,
                        CategoryId = catId,
                        SalePrice = decimal.TryParse(cols[3], out var sp) ? sp : 0,
                        PurchasePrice = decimal.TryParse(cols[4], out var pp) ? pp : 0,
                        PurchaseUnit = cols[5].Trim(),
                        ImportQuantity = int.TryParse(cols[6], out var iq) ? iq : 0,
                        StockQuantity = int.TryParse(cols[7], out var sq) ? sq : 0,
                        Description = cols.Length > 8 ? cols[8].Trim() : ""
                    };

                    // Update existing or add new
                    var existing = await _productRepository.GetByCodeAsync(code);
                    if (existing != null)
                    {
                        product.Id = existing.Id;
                        await _productRepository.UpdateAsync(product);
                    }
                    else
                    {
                        await _productRepository.AddAsync(product);
                    }
                    count++;
                }

                _cacheService.Remove(CacheKey);
                return count;
            }
            catch { return -1; }
        }
    }
}
