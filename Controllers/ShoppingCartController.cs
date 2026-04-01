using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web_dienmay.Models;
using Web_dienmay.Repositories;
using Web_dienmay.Services;
using PayOS.Models.V2.PaymentRequests;
using System.Text.Json;

namespace Web_dienmay.Controllers
{
    // DTO class để lưu pending order
    public class PendingOrderDto
    {
        public string UserId { get; set; }
        public string ShippingAddress { get; set; }
        public string Notes { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
        public long TempOrderCode { get; set; }
        public string PaymentLinkId { get; set; }
    }

    public class OrderDetailDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IShoppingCartRepository _cartRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PayOSService _payOSService;

        public ShoppingCartController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IProductRepository productRepository,
            IShoppingCartRepository cartRepository,
            PayOSService payOSService)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _cartRepository = cartRepository;
            _payOSService = payOSService;
        }

        public async Task<IActionResult> AddToCart(int Id, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var product = await GetProductFromDatabase(Id);

            if (product == null || !product.IsAvailable || product.StockQuantity < quantity)
            {
                TempData["Error"] = product == null
                    ? "Sản phẩm không tồn tại."
                    : (!product.IsAvailable
                        ? "Sản phẩm này không khả dụng."
                        : $"Sản phẩm '{product.Name}' chỉ còn {product.StockQuantity} trong kho, không đủ để thêm {quantity} sản phẩm.");

                return RedirectToAction("Index", "Product");
            }

            var cartItem = new CartItem
            {
                ProductId = Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity
            };

            await _cartRepository.AddItemToCartAsync(user.Id, cartItem);

            TempData["Success"] = $"Đã thêm {quantity} sản phẩm {product.Name} vào giỏ hàng.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var cart = await _cartRepository.GetCartAsync(user.Id);
            return View(cart);
        }

        private async Task<Product> GetProductFromDatabase(int Id)
        {
            var product = await _productRepository.GetByIdAsync(Id);
            return product;
        }

        public async Task<IActionResult> RemoveFromCart(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var cart = await _cartRepository.GetCartAsync(user.Id);
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == Id);

            if (cartItem != null)
            {
                await _cartRepository.RemoveCartItemAsync(cartItem.Id);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> IncreaseQuantity(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var product = await _productRepository.GetByIdAsync(Id);
            var cart = await _cartRepository.GetCartAsync(user.Id);
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == Id);

            if (cartItem != null)
            {
                if (product != null && product.IsAvailable && product.StockQuantity > cartItem.Quantity)
                {
                    cartItem.Quantity++;
                    await _cartRepository.UpdateCartItemAsync(cartItem);
                }
                else
                {
                    TempData["Error"] = $"Không thể tăng số lượng sản phẩm '{cartItem.Name}'. Chỉ còn {product?.StockQuantity ?? 0} sản phẩm trong kho.";
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DecreaseQuantity(int Id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var cart = await _cartRepository.GetCartAsync(user.Id);
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == Id);

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    await _cartRepository.UpdateCartItemAsync(cartItem);
                }
                else
                {
                    await _cartRepository.RemoveCartItemAsync(cartItem.Id);
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var cart = await _cartRepository.GetCartAsync(user.Id);
            ViewBag.Cart = cart;

            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index");
            }

            return View(new Order());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, string paymentMethod)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var cart = await _cartRepository.GetCartAsync(user.Id);
            ViewBag.Cart = cart;
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            // Kiểm tra stock
            bool stockAvailable = true;
            foreach (var item in cart.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null || !product.IsAvailable || product.StockQuantity < item.Quantity)
                {
                    stockAvailable = false;
                    TempData["Error"] = $"Sản phẩm '{item.Name}' không đủ số lượng trong kho. Hiện chỉ còn {product?.StockQuantity ?? 0} sản phẩm.";
                    break;
                }
            }

            if (!stockAvailable)
            {
                return RedirectToAction("Index");
            }

            // Chuẩn bị thông tin order
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.PaymentMethod = paymentMethod?.ToUpper() ?? "COD";
            order.PaymentStatus = "Chưa thanh toán";
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            // Phân biệt PayOS và COD
            if (order.PaymentMethod == "BANKING" || order.PaymentMethod == "MOMO")
            {
                // ===== PAYOS FLOW =====
                try
                {
                    long tempOrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    var paymentResponse = await _payOSService.CreatePaymentLink(
                        orderCode: tempOrderCode,
                        description: $"Thanh toan don hang #{order.Id}",
                        amount: (int)order.TotalPrice,
                        returnUrl: Url.Action("PaymentCallback", "ShoppingCart", new { tempOrderCode = tempOrderCode }, Request.Scheme),
                        cancelUrl: Url.Action("Checkout", "ShoppingCart", null, Request.Scheme)
                    );

                    // ✅ SỬA: Dùng DTO thay vì anonymous type + dynamic
                    var pendingOrder = new PendingOrderDto
                    {
                        UserId = order.UserId,
                        ShippingAddress = order.ShippingAddress,
                        Notes = order.Notes,
                        PaymentMethod = order.PaymentMethod,
                        TotalPrice = order.TotalPrice,
                        OrderDetails = cart.Items.Select(i => new OrderDetailDto
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            Price = i.Price
                        }).ToList(),
                        TempOrderCode = tempOrderCode,
                        PaymentLinkId = paymentResponse.PaymentLinkId
                    };

                    TempData["PendingOrder"] = JsonSerializer.Serialize(pendingOrder);

                    return Redirect(paymentResponse.CheckoutUrl);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Lỗi tạo link thanh toán: {ex.Message}";
                    return RedirectToAction("Checkout");
                }
            }
            else
            {
                // ===== COD FLOW =====
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();

                        foreach (var item in cart.Items)
                        {
                            var product = await _context.Products.FindAsync(item.ProductId);
                            if (product != null)
                            {
                                product.StockQuantity -= item.Quantity;
                                _context.Products.Update(product);
                            }
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        await _cartRepository.ClearCartAsync(user.Id);

                        return View("OrderCompleted", order.Id);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "Đã xảy ra lỗi khi xử lý đơn hàng. Vui lòng thử lại.";
                        return RedirectToAction("Index");
                    }
                }
            }
        }

        // Xử lý callback từ PayOS sau khi thanh toán
        public async Task<IActionResult> PaymentCallback(long tempOrderCode, string? status)
        {
            try
            {
                var paymentInfo = await _payOSService.GetPaymentLinkInformation(tempOrderCode);

                if (paymentInfo.Status == PaymentLinkStatus.Paid)
                {
                    var pendingOrderJson = TempData["PendingOrder"] as string;
                    if (string.IsNullOrEmpty(pendingOrderJson))
                    {
                        TempData["Error"] = "Không tìm thấy thông tin đơn hàng.";
                        return RedirectToAction("Index", "ShoppingCart");
                    }

                    // ✅ SỬA: Deserialize thành PendingOrderDto thay vì dynamic
                    var pendingOrder = JsonSerializer.Deserialize<PendingOrderDto>(pendingOrderJson);

                    var user = await _userManager.FindByIdAsync(pendingOrder.UserId);
                    if (user == null)
                    {
                        TempData["Error"] = "Không tìm thấy người dùng.";
                        return RedirectToAction("Index", "ShoppingCart");
                    }

                    var cart = await _cartRepository.GetCartAsync(user.Id);
                    if (cart == null || !cart.Items.Any())
                    {
                        TempData["Error"] = "Giỏ hàng trống.";
                        return RedirectToAction("Index", "ShoppingCart");
                    }

                    var order = new Order
                    {
                        UserId = user.Id,
                        OrderDate = DateTime.UtcNow,
                        ShippingAddress = pendingOrder.ShippingAddress,
                        Notes = pendingOrder.Notes,
                        TotalPrice = pendingOrder.TotalPrice,
                        PaymentMethod = pendingOrder.PaymentMethod,
                        PaymentStatus = "Đã thanh toán",
                        TransactionId = paymentInfo.Id,
                        OrderDetails = cart.Items.Select(i => new OrderDetail
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            Price = i.Price
                        }).ToList()
                    };

                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            _context.Orders.Add(order);
                            await _context.SaveChangesAsync();

                            foreach (var item in cart.Items)
                            {
                                var product = await _context.Products.FindAsync(item.ProductId);
                                if (product != null)
                                {
                                    product.StockQuantity -= item.Quantity;
                                    _context.Products.Update(product);
                                }
                            }

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            await _cartRepository.ClearCartAsync(user.Id);

                            TempData["Success"] = "Thanh toán thành công!";
                            return View("OrderCompleted", order.Id);
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            TempData["Error"] = $"Lỗi tạo đơn hàng: {ex.Message}";
                            return RedirectToAction("Index", "ShoppingCart");
                        }
                    }
                }
                else if (paymentInfo.Status == PaymentLinkStatus.Cancelled)
                {
                    TempData["Error"] = "Thanh toán đã bị hủy.";
                    return RedirectToAction("Checkout");
                }
                else
                {
                    TempData["Warning"] = "Trạng thái thanh toán chưa rõ ràng. Vui lòng kiểm tra lại.";
                    return RedirectToAction("Index", "ShoppingCart");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi xác thực thanh toán: {ex.Message}";
                return RedirectToAction("Index", "ShoppingCart");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PaymentWebhook([FromBody] string webhookBody)
        {
            try
            {
                var verified = _payOSService.VerifyWebhookData(webhookBody);

                if (verified)
                {
                    return Ok(new { success = true });
                }

                return BadRequest(new { success = false, message = "Invalid webhook data" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}