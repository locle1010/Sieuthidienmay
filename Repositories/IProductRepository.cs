using Web_dienmay.Models;

namespace Web_dienmay.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetAllAvailableAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Product>> GetAvailableByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Product>> GetAllVisibleAsync();
        Task<IEnumerable<Product>> GetVisibleByCategoryIdAsync(int categoryId);

    }
}
