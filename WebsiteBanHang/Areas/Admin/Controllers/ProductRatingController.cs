// WebsiteBanHang/Areas/Admin/Controllers/ProductRatingController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_dienmay.Models;
using Web_dienmay.Repositories;

namespace Web_dienmay.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductRatingController : Controller
    {
        private readonly IProductRatingRepository _ratingRepository;
        private readonly IProductRepository _productRepository;

        public ProductRatingController(IProductRatingRepository ratingRepository, IProductRepository productRepository)
        {
            _ratingRepository = ratingRepository;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index(int? productId, string searchTerm = "")
        {
            IEnumerable<ProductRating> ratings;

            if (productId.HasValue)
            {
                ratings = await _ratingRepository.GetByProductIdAsync(productId.Value);
                ViewBag.ProductName = (await _productRepository.GetByIdAsync(productId.Value))?.Name;
            }
            else
            {
                ratings = await _ratingRepository.GetAllAsync();
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                ratings = ratings.Where(r =>
                    r.Comment != null && r.Comment.ToLower().Contains(searchTerm) ||
                    r.User.UserName.ToLower().Contains(searchTerm) ||
                    r.User.FullName.ToLower().Contains(searchTerm) ||
                    r.Product.Name.ToLower().Contains(searchTerm)
                );
            }

            ViewBag.ProductId = productId;
            ViewBag.SearchTerm = searchTerm;

            return View(ratings);
        }

        // Show rating details
        public async Task<IActionResult> Details(int id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            if (rating == null)
            {
                return NotFound();
            }

            return View(rating);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            if (rating == null)
            {
                return NotFound();
            }

            int productId = rating.ProductId;
            await _ratingRepository.DeleteAsync(id);

            TempData["SuccessMessage"] = "Đánh giá đã được xóa thành công.";

            if (Request.Query.ContainsKey("returnToProduct"))
            {
                return RedirectToAction(nameof(Index), new { productId });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRating(int id)
        {
            var rating = await _ratingRepository.GetByIdAsync(id);
            if (rating == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });
            }

            await _ratingRepository.DeleteAsync(id);
            return Json(new { success = true, message = "Đánh giá đã được xóa thành công" });
        }

        public async Task<IActionResult> GetProductRatings(int productId)
        {
            var ratings = await _ratingRepository.GetByProductIdAsync(productId);
            return PartialView("_ProductRatings", ratings);
        }
    }
}
