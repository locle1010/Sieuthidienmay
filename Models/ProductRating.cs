using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_dienmay.Models
{
    public class ProductRating
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string UserId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
