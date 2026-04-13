using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FashionStore.Data.Interfaces;
using FashionStore.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name)) return false;
            return await _categoryRepository.AddAsync(category);
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            if (category.Id <= 0 || string.IsNullOrWhiteSpace(category.Name)) return false;
            return await _categoryRepository.UpdateAsync(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            if (await _categoryRepository.HasProductsAsync(id)) return false;
            return await _categoryRepository.DeleteAsync(id);
        }

        public async Task<int> EnsureCategoryAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return 0;
            name = name.Trim();
            
            var existing = await _categoryRepository.GetByNameAsync(name);
            if (existing != null) return existing.Id;

            var newCategory = new Category { Name = name, TaxPercent = 10 };
            if (await _categoryRepository.AddAsync(newCategory))
            {
                var created = await _categoryRepository.GetByNameAsync(name);
                return created?.Id ?? 0;
            }
            return 0;
        }

        #region Legacy Static Bridge - SHOULD BE DEPRECATED

        private static ICategoryService GetService() => App.ServiceProvider?.GetRequiredService<ICategoryService>() ?? throw new InvalidOperationException("DI not initialized");

        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();

        public static List<(int Id, string Name, decimal TaxPercent)> GetAllCategories()
            => RunSync(() => GetService().GetAllCategoriesAsync())
               .Select(c => (c.Id, c.Name, c.TaxPercent)).ToList();

        public static bool AddCategory(string name, decimal taxPercent)
            => RunSync(() => GetService().AddCategoryAsync(new Category { Name = name, TaxPercent = taxPercent }));

        public static bool UpdateCategory(int id, string name, decimal taxPercent)
            => RunSync(() => GetService().UpdateCategoryAsync(new Category { Id = id, Name = name, TaxPercent = taxPercent }));

        public static bool DeleteCategory(int id)
            => RunSync(() => GetService().DeleteCategoryAsync(id));

        public static int EnsureCategory(string name)
            => RunSync(() => GetService().EnsureCategoryAsync(name));

        public static bool DeleteAllCategories()
        {
            // Placeholder: Not implemented in service yet
            return false;
        }

        #endregion
    }
}
