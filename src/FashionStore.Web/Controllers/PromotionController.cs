using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using FashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class PromotionController : Controller
    {
        private readonly IPromotionService _promotionService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public PromotionController(
            IPromotionService promotionService,
            IProductService productService,
            ICategoryService categoryService)
        {
            _promotionService = promotionService;
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var promotions = await _promotionService.GetAllPromotionsAsync();
            return View(promotions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _productService.GetAllProductsWithCategoriesAsync();
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(new Promotion());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                await _promotionService.AddPromotionAsync(promotion);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Products = await _productService.GetAllProductsWithCategoriesAsync();
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(promotion);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var promotion = await _promotionService.GetPromotionByIdAsync(id);
            if (promotion == null) return NotFound();
            
            ViewBag.Products = await _productService.GetAllProductsWithCategoriesAsync();
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(promotion);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                await _promotionService.UpdatePromotionAsync(promotion);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Products = await _productService.GetAllProductsWithCategoriesAsync();
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(promotion);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _promotionService.DeletePromotionAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
