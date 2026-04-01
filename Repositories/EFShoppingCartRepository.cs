using Microsoft.EntityFrameworkCore;
using Web_dienmay.Models;

namespace Web_dienmay.Repositories
{
    public class EFShoppingCartRepository : IShoppingCartRepository
    {
        private readonly ApplicationDbContext _context;

        public EFShoppingCartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ShoppingCart> GetCartAsync(string userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Create a new cart if it doesn't exist
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now
                };

                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartItem> GetCartItemAsync(int cartItemId)
        {
            return await _context.CartItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == cartItemId);
        }

        public async Task AddItemToCartAsync(string userId, CartItem cartItem)
        {
            var cart = await GetCartAsync(userId);

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == cartItem.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += cartItem.Quantity;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                cartItem.ShoppingCartId = cart.Id;
                _context.CartItems.Add(cartItem);
            }

            cart.LastModifiedDate = DateTime.Now;
            _context.ShoppingCarts.Update(cart);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);

            var cart = await _context.ShoppingCarts.FindAsync(cartItem.ShoppingCartId);
            if (cart != null)
            {
                cart.LastModifiedDate = DateTime.Now;
                _context.ShoppingCarts.Update(cart);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItemAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                var cart = await _context.ShoppingCarts.FindAsync(cartItem.ShoppingCartId);

                _context.CartItems.Remove(cartItem);

                if (cart != null)
                {
                    cart.LastModifiedDate = DateTime.Now;
                    _context.ShoppingCarts.Update(cart);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetCartAsync(userId);

            var cartItems = await _context.CartItems
                .Where(i => i.ShoppingCartId == cart.Id)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);

            cart.LastModifiedDate = DateTime.Now;
            _context.ShoppingCarts.Update(cart);

            await _context.SaveChangesAsync();
        }

        public async Task SaveCartAsync(ShoppingCart cart)
        {
            cart.LastModifiedDate = DateTime.Now;
            _context.ShoppingCarts.Update(cart);
            await _context.SaveChangesAsync();
        }
    }
}

