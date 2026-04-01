using Microsoft.EntityFrameworkCore;
using Web_dienmay.Models;

namespace Web_dienmay.Repositories
{
    public class EFProductRatingRepository : IProductRatingRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRatingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductRating>> GetAllAsync()
        {
            return await _context.ProductRatings
                .Include(r => r.Product)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductRating>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductRatings
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductRating>> GetByUserIdAsync(string userId)
        {
            return await _context.ProductRatings
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductRating> GetByIdAsync(int id)
        {
            return await _context.ProductRatings
                .Include(r => r.Product)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task DeleteAsync(int id)
        {
            var rating = await _context.ProductRatings.FindAsync(id);
            if (rating != null)
            {
                _context.ProductRatings.Remove(rating);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetRatingCountForProductAsync(int productId)
        {
            return await _context.ProductRatings
                .Where(r => r.ProductId == productId)
                .CountAsync();
        }

        public async Task<double> GetAverageRatingForProductAsync(int productId)
        {
            var ratings = await _context.ProductRatings
                .Where(r => r.ProductId == productId)
                .Select(r => r.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }
    }
}
