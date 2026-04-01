// WebsiteBanHang/Repositories/IProductRatingRepository.cs
using Web_dienmay.Models;

namespace Web_dienmay.Repositories
{
    public interface IProductRatingRepository
    {
        Task<IEnumerable<ProductRating>> GetAllAsync();
        Task<IEnumerable<ProductRating>> GetByProductIdAsync(int productId);
        Task<IEnumerable<ProductRating>> GetByUserIdAsync(string userId);
        Task<ProductRating> GetByIdAsync(int id);
        Task DeleteAsync(int id);
        Task<int> GetRatingCountForProductAsync(int productId);
        Task<double> GetAverageRatingForProductAsync(int productId);
    }
}
