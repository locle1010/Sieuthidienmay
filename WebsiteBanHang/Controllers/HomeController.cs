using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_dienmay.Models;
using Web_dienmay.Repositories;

namespace Web_dienmay.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductRepository _productRepository;
    private readonly ApplicationDbContext _context;

    public HomeController(
        ILogger<HomeController> logger,
        IProductRepository productRepository,
        ApplicationDbContext context)
    {
        _logger = logger;
        _productRepository = productRepository;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var topSellingProducts = await _context.OrderDetails
            .GroupBy(od => od.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalSold = g.Sum(od => od.Quantity)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(10) // Lấy nhiều hơn để đảm bảo đủ sản phẩm sau khi lọc
            .Select(x => x.ProductId)
            .ToListAsync();

        var bestSellingProducts = new List<Product>();

        foreach (var productId in topSellingProducts)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product != null && product.IsAvailable && product.StockQuantity > 0)
            {
                bestSellingProducts.Add(product);
                if (bestSellingProducts.Count >= 5) break;
            }
        }

        if (bestSellingProducts.Count < 5)
        {
            var availableProducts = await _productRepository.GetAllAvailableAsync(); //Đã lọc IsAvailable và StockQuantity > 0
            var additionalProducts = availableProducts
                .Where(p => !bestSellingProducts.Any(bp => bp.Id == p.Id))
                .OrderBy(x => Guid.NewGuid())
                .Take(5 - bestSellingProducts.Count);

            bestSellingProducts.AddRange(additionalProducts);
        }

        var allAvailableProducts = await _productRepository.GetAllAvailableAsync();
        var randomProducts = allAvailableProducts
            .Where(p => !bestSellingProducts.Any(bp => bp.Id == p.Id))
            .OrderBy(x => Guid.NewGuid())
            .Take(4)
            .ToList();

        ViewBag.BestSellingProducts = bestSellingProducts;
        ViewBag.RandomProducts = randomProducts;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult About()
    {
        return View();
    }
    public IActionResult Contact()
    {
        return View();
    }
    public IActionResult Showroom()
    {
        return View();
    }
}
