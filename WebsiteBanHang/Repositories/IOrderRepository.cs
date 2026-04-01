using Web_dienmay.Models;

namespace Web_dienmay.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
        Task<Order> GetByIdAsync(int id);
        Task<bool> HasUserPurchasedProduct(string userId, int productId);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
    }
}
