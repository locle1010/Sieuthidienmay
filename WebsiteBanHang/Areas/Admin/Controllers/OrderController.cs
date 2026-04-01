using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Web_dienmay.Models;
using Web_dienmay.Services;

namespace Web_dienmay.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly InvoiceService _invoiceService;

        public OrderController(ApplicationDbContext context, InvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }

        public async Task<IActionResult> Index(string searchString, string orderStatus, int page = 1)
        {
            const int pageSize = 10;
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = orderStatus;

            ViewData["OrderStatuses"] = new List<string>
            {
                "Tất cả",
                "Đang xử lý",
                "Đã xác nhận",
                "Đang giao hàng",
                "Đã giao hàng",
                "Đã hủy"
            };

            var ordersQuery = _context.Orders
                .Include(o => o.ApplicationUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.Id.ToString().Contains(searchString) ||
                    o.ApplicationUser.FullName.Contains(searchString) ||
                    o.ApplicationUser.Email.Contains(searchString) ||
                    o.ShippingAddress.Contains(searchString)
                );
            }

            if (!string.IsNullOrEmpty(orderStatus) && orderStatus != "Tất cả")
            {
                ordersQuery = ordersQuery.Where(o => o.OrderStatus == orderStatus);
            }

            ordersQuery = ordersQuery.OrderByDescending(o => o.OrderDate);

            var count = await ordersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(count / (double)pageSize);

            page = Math.Max(1, Math.Min(page, totalPages));

            var orders = await ordersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = status;

            if (status == "Đã giao hàng" && !order.ShippedDate.HasValue)
            {
                order.ShippedDate = DateTime.Now;
            }

            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            _context.OrderDetails.RemoveRange(order.OrderDetails);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đơn hàng đã được xóa thành công!";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _invoiceService.GetOrderDetailsAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.InvoiceHtml = _invoiceService.GenerateInvoiceHtml(order);
            return View(order);
        }

        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var order = await _invoiceService.GetOrderDetailsAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            var pdfBytes = _invoiceService.GenerateInvoicePdf(order);

            return File(pdfBytes, "application/pdf", $"HoaDon-{order.Id}.pdf");
        }
    }
}
