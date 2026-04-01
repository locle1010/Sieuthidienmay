using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web_dienmay.Models;
using Web_dienmay.Repositories;

namespace Web_dienmay.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ApplicationDbContext _context;


        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index(int? categoryId, string filter = "all", string searchTerm = "")
        {
            var products = categoryId.HasValue
                ? await _productRepository.GetByCategoryIdAsync(categoryId.Value)
                : await _productRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                products = products.Where(p =>
                    p.Id.ToString().Contains(searchTerm) ||
                    p.Name.ToLower().Contains(searchTerm));
            }

            products = filter switch
            {
                "available" => products.Where(p => p.IsAvailable),
                "unavailable" => products.Where(p => !p.IsAvailable),
                "outofstock" => products.Where(p => p.StockQuantity <= 0),
                _ => products 
            };

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.CurrentFilter = filter;
            ViewBag.SearchTerm = searchTerm;

            return View(products);
        }

        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                if (product.StockQuantity < 0)
                {
                    product.StockQuantity = 0;
                }

                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl"); 

            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                if (imageUrl == null)
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
                else
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }

                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.IsAvailable = product.IsAvailable;

                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", image.FileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + image.FileName; 
        }
        [HttpPost]
        public async Task<IActionResult> ToggleAvailability(int id, bool isAvailable)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return Json(new { success = false });
            }

            product.IsAvailable = isAvailable;
            await _productRepository.UpdateAsync(product);

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int stockQuantity)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return Json(new { success = false });
            }

            var previousQty = product.StockQuantity;
            product.StockQuantity = stockQuantity >= 0 ? stockQuantity : 0;
            await _productRepository.UpdateAsync(product);

            // Tạo log trong StockEntry
            var userName = User.Identity?.Name ?? "Unknown";
            var stockEntry = new StockEntry
            {
                ProductId = id,
                Quantity = product.StockQuantity - previousQty,
                Type = "Adjustment",
                Note = "Điều chỉnh từ trang quản lý sản phẩm",
                CreatedBy = userName,
                CreatedDate = DateTime.Now,
                PreviousQuantity = previousQty,
                NewQuantity = product.StockQuantity
            };

            _context.StockEntries.Add(stockEntry);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


    }
}
