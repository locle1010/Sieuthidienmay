using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_dienmay.Models;
using System.Security.Claims;

namespace Web_dienmay.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class StockController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Stock/Index - Lịch sử nhập xuất kho
        public async Task<IActionResult> Index(int? productId, string? type, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.StockEntries
                .Include(s => s.Product)
                    .ThenInclude(p => p!.Category)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(s => s.ProductId == productId.Value);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(s => s.Type == type);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.CreatedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var toDateEnd = toDate.Value.AddDays(1);
                query = query.Where(s => s.CreatedDate < toDateEnd);
            }

            var stockEntries = await query
                .OrderByDescending(s => s.CreatedDate)
                .Take(500) // Giới hạn 500 bản ghi gần nhất
                .ToListAsync();

            ViewBag.Products = await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            ViewBag.SelectedProductId = productId;
            ViewBag.SelectedType = type;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(stockEntries);
        }

        // GET: Admin/Stock/Import - Trang nhập kho
        public async Task<IActionResult> Import()
        {
            ViewBag.Products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View();
        }

        // POST: Admin/Stock/ImportProducts - Nhập nhiều sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportProducts(List<StockImportItem> items, string? note)
        {
            if (items == null || !items.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm để nhập kho.";
                return RedirectToAction(nameof(Import));
            }

            var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            var errors = new List<string>();
            var successCount = 0;

            foreach (var item in items.Where(i => i.Quantity > 0))
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                {
                    errors.Add($"Không tìm thấy sản phẩm ID: {item.ProductId}");
                    continue;
                }

                var previousQty = product.StockQuantity;
                product.StockQuantity += item.Quantity;

                var stockEntry = new StockEntry
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Type = "Import",
                    Note = note,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    PreviousQuantity = previousQty,
                    NewQuantity = product.StockQuantity
                };

                _context.StockEntries.Add(stockEntry);
                successCount++;
            }

            if (successCount > 0)
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã nhập kho thành công {successCount} sản phẩm!";
            }

            if (errors.Any())
            {
                TempData["Warning"] = string.Join("<br/>", errors);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Stock/QuickAdjust - Điều chỉnh nhanh số lượng
        [HttpPost]
        public async Task<IActionResult> QuickAdjust(int productId, int quantity, string adjustmentType, string? note)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
            var previousQty = product.StockQuantity;
            int actualChange = 0;

            switch (adjustmentType)
            {
                case "set":
                    actualChange = quantity - previousQty;
                    product.StockQuantity = quantity;
                    break;
                case "add":
                    actualChange = quantity;
                    product.StockQuantity += quantity;
                    break;
                case "subtract":
                    actualChange = -quantity;
                    product.StockQuantity = Math.Max(0, product.StockQuantity - quantity);
                    break;
                default:
                    return Json(new { success = false, message = "Loại điều chỉnh không hợp lệ." });
            }

            if (product.StockQuantity < 0)
            {
                product.StockQuantity = 0;
            }

            var stockEntry = new StockEntry
            {
                ProductId = productId,
                Quantity = actualChange,
                Type = "Adjustment",
                Note = note ?? $"Điều chỉnh: {adjustmentType}",
                CreatedBy = userName,
                CreatedDate = DateTime.Now,
                PreviousQuantity = previousQty,
                NewQuantity = product.StockQuantity
            };

            _context.StockEntries.Add(stockEntry);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                newQuantity = product.StockQuantity,
                message = "Đã cập nhật số lượng tồn kho thành công!"
            });
        }

        // GET: Admin/Stock/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var stockEntry = await _context.StockEntries
                .Include(s => s.Product)
                    .ThenInclude(p => p!.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (stockEntry == null)
            {
                return NotFound();
            }

            return View(stockEntry);
        }
    }

    // ViewModel cho việc nhập kho
    public class StockImportItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}