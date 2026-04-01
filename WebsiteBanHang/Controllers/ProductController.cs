using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Web_dienmay.Models;
using Web_dienmay.Repositories;

namespace Web_dienmay.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _context;


        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository,
            IOrderRepository orderRepository,ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _orderRepository = orderRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string searchTerm, string sortOrder, int page = 1)
        {
            int pageSize = 12;

            ViewBag.CurrentSort = sortOrder;
            ViewBag.PriceSortParm = string.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewBag.PriceDescSortParm = sortOrder == "price_asc" ? "" : "price_asc";

            IEnumerable<Product> allProducts;

            if (categoryId.HasValue)
            {
                allProducts = await _productRepository.GetVisibleByCategoryIdAsync(categoryId.Value);
            }
            else
            {
                allProducts = await _productRepository.GetAllVisibleAsync();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                allProducts = allProducts.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description != null && p.Description.ToLower().Contains(searchTerm) ||
                    p.Category != null && p.Category.Name.ToLower().Contains(searchTerm)
                );
            }

            switch (sortOrder)
            {
                case "price_desc":
                    allProducts = allProducts.OrderByDescending(p => p.Price);
                    break;
                case "price_asc":
                    allProducts = allProducts.OrderBy(p => p.Price);
                    break;
                default:
                    break;
            }

            var totalItems = allProducts.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(searchTerm) || categoryId.HasValue)
            {
                products = allProducts.ToList();
            }
            else
            {
                products = allProducts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", products);
            }

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreProducts(int page = 1, int? categoryId = null, string searchTerm = null, string sortOrder = null)
        {
            int pageSize = 12;

            try
            {
                IEnumerable<Product> allProducts;

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    allProducts = await _productRepository.GetVisibleByCategoryIdAsync(categoryId.Value);
                }
                else
                {
                    allProducts = await _productRepository.GetAllVisibleAsync();
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    allProducts = allProducts.Where(p =>
                        p.Name.ToLower().Contains(searchTerm) ||
                        (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                        (p.Category != null && p.Category.Name.ToLower().Contains(searchTerm))
                    );
                }

                switch (sortOrder)
                {
                    case "price_desc":
                        allProducts = allProducts.OrderByDescending(p => p.Price);
                        break;
                    case "price_asc":
                        allProducts = allProducts.OrderBy(p => p.Price);
                        break;
                    default:
                        break;
                }

                var totalItems = allProducts.Count();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                page = Math.Max(1, Math.Min(page, totalPages));

                var products = allProducts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                string html = await RenderPartialViewToStringAsync("_ProductList", products);

                return Json(new
                {
                    success = true,
                    html = html,
                    currentPage = page,
                    totalPages = totalPages,
                    hasMorePages = page < totalPages
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Ensures only logged-in users can rate
        public async Task<IActionResult> Rate(int productId, int rating, string comment)
        {
            // Verify the product exists
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Get current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user has purchased this product
            bool hasPurchased = await _orderRepository.HasUserPurchasedProduct(userId, productId);

            if (!hasPurchased)
            {
                TempData["ErrorMessage"] = "Bạn cần mua sản phẩm này trước khi đánh giá.";
                return RedirectToAction("Display", new { id = productId });
            }

            // Check if user has already rated this product
            var existingRating = await _context.ProductRatings
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.Rating = rating;
                existingRating.Comment = comment;
                existingRating.CreatedAt = DateTime.Now;

                _context.ProductRatings.Update(existingRating);
            }
            else
            {
                // Create new rating
                var newRating = new ProductRating
                {
                    ProductId = productId,
                    UserId = userId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };

                _context.ProductRatings.Add(newRating);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            return RedirectToAction("Display", new { id = productId });
        }

        // Add this method to fetch ratings for display
        private async Task<IEnumerable<ProductRating>> GetProductRatings(int productId)
        {
            return await _context.ProductRatings
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }





        // Show product details
        // Show product details
        // Update the Display action in ProductController.cs
        // Update the Display action in ProductController.cs
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || !product.IsAvailable)
            {
                return NotFound();
            }

            // Get related products
            var relatedProducts = await _productRepository.GetVisibleByCategoryIdAsync(product.CategoryId);
            relatedProducts = relatedProducts
                .Where(p => p.Id != id)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            // Get product ratings
            ViewBag.Ratings = await GetProductRatings(id);

            // Check if current user has purchased this product (for rating eligibility)
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.HasPurchased = await _orderRepository.HasUserPurchasedProduct(userId, id);

                // Check if user has already rated
                var existingRating = await _context.ProductRatings
                    .FirstOrDefaultAsync(r => r.ProductId == id && r.UserId == userId);
                ViewBag.UserRating = existingRating;
            }

            return View(product);
        }









    }
}
