using Microsoft.EntityFrameworkCore;
using Web_dienmay.Models;

namespace Web_dienmay.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }

        

        

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAvailableByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsAvailable && p.StockQuantity > 0)
                .ToListAsync();
        }

        // Add to EFProductRepository.cs
        public async Task<IEnumerable<Product>> GetAllVisibleAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Ratings)
                .Where(p => p.IsAvailable)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetVisibleByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Ratings)
                .Where(p => p.CategoryId == categoryId && p.IsAvailable)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllAvailableAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Ratings)
                .Where(p => p.IsAvailable && p.StockQuantity > 0)
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

    }
}
