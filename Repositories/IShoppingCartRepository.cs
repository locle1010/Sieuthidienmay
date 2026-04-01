using Web_dienmay.Models;

namespace Web_dienmay.Repositories
{
    public interface IShoppingCartRepository
    {
        Task<ShoppingCart> GetCartAsync(string userId);
        Task<CartItem> GetCartItemAsync(int cartItemId);
        Task AddItemToCartAsync(string userId, CartItem cartItem);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(int cartItemId);
        Task ClearCartAsync(string userId);
        Task SaveCartAsync(ShoppingCart cart);
    }
}

