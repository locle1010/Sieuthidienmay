using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_dienmay.Models;

namespace Web_dienmay.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Đang xử lý");
            ViewBag.ShippingOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Đang giao hàng");
            ViewBag.CompletedOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Đã giao hàng");
            ViewBag.CancelledOrders = await _context.Orders.CountAsync(o => o.OrderStatus == "Đã hủy");

            var totalRevenue = await _context.Orders
                .Where(o => o.OrderStatus != "Đã hủy")
                .SumAsync(o => o.TotalPrice);

            var monthlyRevenue = await _context.Orders
                .Where(o => o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth && o.OrderStatus != "Đã hủy")
                .SumAsync(o => o.TotalPrice);

            var lastMonthRevenue = await _context.Orders
                .Where(o => o.OrderDate >= startOfLastMonth && o.OrderDate <= endOfLastMonth && o.OrderStatus != "Đã hủy")
                .SumAsync(o => o.TotalPrice);

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.LastMonthRevenue = lastMonthRevenue;

            ViewBag.RecentOrders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TopProducts = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .Join(
                    _context.Products,
                    tp => tp.ProductId,
                    p => p.Id,
                    (tp, p) => new {
                        Product = p,
                        tp.TotalQuantity
                    }
                )
                .ToListAsync();

            ViewBag.LowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity <= 5 && p.StockQuantity > 0 && p.IsAvailable)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToListAsync();

            var lastMonthOrderCount = await _context.Orders
                .CountAsync(o => o.OrderDate >= startOfLastMonth && o.OrderDate <= endOfLastMonth);
            var currentMonthOrderCount = await _context.Orders
                .CountAsync(o => o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth);

            if (lastMonthOrderCount > 0)
            {
                ViewBag.OrderGrowth = Math.Round(((double)currentMonthOrderCount - lastMonthOrderCount) / lastMonthOrderCount * 100, 1);
            }
            else
            {
                ViewBag.OrderGrowth = null;
            }

            if (lastMonthRevenue > 0)
            {
                var revenueGrowth = ((monthlyRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;
                ViewBag.RevenueGrowth = Math.Round((double)revenueGrowth, 1);
            }
            else
            {
                ViewBag.RevenueGrowth = null;
            }

            var dayOfMonth = today.Day;
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            if (dayOfMonth > 0)
            {
                var dailyAverage = monthlyRevenue / dayOfMonth;
                ViewBag.EstimatedRevenue = dailyAverage * daysInMonth;
            }
            else
            {
                ViewBag.EstimatedRevenue = 0;
            }

            var last6Months = new List<DateTime>();
            var revenueChartLabels = new List<string>();
            var revenueChartData = new List<decimal>();

            for (int i = 5; i >= 0; i--)
            {
                var month = startOfMonth.AddMonths(-i);
                last6Months.Add(month);
                revenueChartLabels.Add(month.ToString("MMM"));
            }

            foreach (var month in last6Months)
            {
                var startOfCurrentMonth = new DateTime(month.Year, month.Month, 1);
                var endOfCurrentMonth = startOfCurrentMonth.AddMonths(1).AddDays(-1);

                var monthlyRevenueInChart = await _context.Orders
                    .Where(o => o.OrderDate >= startOfCurrentMonth && o.OrderDate <= endOfCurrentMonth && o.OrderStatus != "Đã hủy")
                    .SumAsync(o => o.TotalPrice);

                revenueChartData.Add(monthlyRevenueInChart);
            }

            ViewBag.RevenueChartLabels = revenueChartLabels;
            ViewBag.RevenueChartData = revenueChartData;

            var categories = await _context.Categories.ToListAsync();
            var categoryLabels = categories.Select(c => c.Name).ToList();
            var categoryProductCounts = new List<int>();

            foreach (var category in categories)
            {
                var count = await _context.Products.CountAsync(p => p.CategoryId == category.Id && p.IsAvailable);
                categoryProductCounts.Add(count);
            }

            ViewBag.CategoryLabels = categoryLabels;
            ViewBag.CategoryProductCounts = categoryProductCounts;

            var categoryRevenue = new List<decimal>();
            foreach (var category in categories)
            {
                var revenue = await _context.OrderDetails
                    .Join(_context.Products,
                        od => od.ProductId,
                        p => p.Id,
                        (od, p) => new { OrderDetail = od, Product = p })
                    .Join(_context.Orders,
                        jp => jp.OrderDetail.OrderId,
                        o => o.Id,
                        (jp, o) => new { jp.OrderDetail, jp.Product, Order = o })
                    .Where(x => x.Product.CategoryId == category.Id &&
                                x.Order.OrderDate >= startOfMonth &&
                                x.Order.OrderDate <= endOfMonth &&
                                x.Order.OrderStatus != "Đã hủy")
                    .SumAsync(x => x.OrderDetail.Price * x.OrderDetail.Quantity);

                categoryRevenue.Add(revenue / 1000000); 
            }

            ViewBag.CategoryRevenue = categoryRevenue;

            var recentOrders = await _context.Orders
                .Include(o => o.ApplicationUser)
                .OrderByDescending(o => o.OrderDate)
                .Take(3)
                .ToListAsync();

            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity <= 3 && p.StockQuantity > 0 && p.IsAvailable)
                .OrderBy(p => p.StockQuantity)
                .Take(2)
                .ToListAsync();

            var recentOrderActivities = recentOrders.Select(o => new
            {
                Title = o.OrderStatus == "Đã hủy" ? "Đơn hàng bị hủy" :
                       (o.OrderStatus == "Đang xử lý" ? "Đơn hàng mới" :
                       (o.OrderStatus == "Đã giao hàng" ? "Đơn hàng đã giao" : "Cập nhật đơn hàng")),
                Description = $"Đơn hàng #{o.Id} - {o.ApplicationUser.FullName} - {o.TotalPrice.ToString("N0")} ₫",
                TimeAgo = GetTimeAgo(o.OrderDate),
                Icon = o.OrderStatus == "Đã hủy" ? "bi-x-circle" :
                      (o.OrderStatus == "Đang xử lý" ? "bi-bag-plus" :
                      (o.OrderStatus == "Đã giao hàng" ? "bi-check-circle" : "bi-arrow-repeat")),
                IconClass = o.OrderStatus == "Đã hủy" ? "bg-danger" :
                           (o.OrderStatus == "Đang xử lý" ? "bg-success" :
                           (o.OrderStatus == "Đã giao hàng" ? "bg-primary" : "bg-info"))
            }).ToList();

            var lowStockActivities = lowStockProducts.Select(p => new
            {
                Title = "Sản phẩm sắp hết hàng",
                Description = $"Sản phẩm {p.Name} chỉ còn {p.StockQuantity} sản phẩm trong kho",
                TimeAgo = "Hôm nay",
                Icon = "bi-exclamation-triangle",
                IconClass = "bg-warning"
            }).ToList();

            var allActivities = new List<object>();
            allActivities.AddRange(recentOrderActivities);
            allActivities.AddRange(lowStockActivities);

            ViewBag.RecentActivities = allActivities.OrderBy(a =>
                ((dynamic)a).TimeAgo.ToString().Contains("phút") ? 1 :
                ((dynamic)a).TimeAgo.ToString().Contains("giờ") ? 2 :
                ((dynamic)a).TimeAgo.ToString().Contains("Hôm nay") ? 3 : 4
            ).Take(5).ToList();

            return View();
        }

        private static string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 60)
            {
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            }
            else if (timeSpan.TotalHours < 24)
            {
                return $"{(int)timeSpan.TotalHours} giờ trước";
            }
            else if (timeSpan.TotalDays < 7)
            {
                return $"{(int)timeSpan.TotalDays} ngày trước";
            }
            else
            {
                return dateTime.ToString("dd/MM/yyyy");
            }
        }
    }
}
