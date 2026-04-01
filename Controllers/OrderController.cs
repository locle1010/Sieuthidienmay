using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_dienmay.Models;
using Web_dienmay.Services;

namespace Web_dienmay.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly InvoiceService _invoiceService;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,InvoiceService invoiceService)
        {
            _context = context;
            _userManager = userManager;
            _invoiceService = invoiceService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        public async Task<IActionResult> Invoice(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var order = await _invoiceService.GetOrderDetailsAsync(id);

            if (order == null || order.UserId != user.Id)
            {
                return NotFound();
            }

            ViewBag.InvoiceHtml = _invoiceService.GenerateInvoiceHtml(order);
            return View(order);
        }

        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var order = await _invoiceService.GetOrderDetailsAsync(id);

            if (order == null || order.UserId != user.Id)
            {
                return NotFound();
            }

            var pdfBytes = _invoiceService.GenerateInvoicePdf(order);

            return File(pdfBytes, "application/pdf", $"HoaDon-{order.Id}.pdf");
        }

    }
}
