using System.ComponentModel.DataAnnotations;

namespace Web_dienmay.Models
{
    public class StockEntry
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        [Range(-100000, 100000, ErrorMessage = "Số lượng phải nằm trong khoảng -100000 đến 100000")]
        public int Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } // "Import" hoặc "Export" hoặc "Adjustment"

        [StringLength(500)]
        public string? Note { get; set; }

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } // Username của admin

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
    }
}