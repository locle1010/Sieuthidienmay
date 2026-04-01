using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_dienmay.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string ShippingAddress { get; set; }
        public string Notes { get; set; }

        public string OrderStatus { get; set; } = "Đang xử lý"; // Default status

        public DateTime? ShippedDate { get; set; }

        public string PaymentMethod { get; set; } = "COD"; // COD, Banking, MoMo
        public string? PaymentStatus { get; set; } = "Chưa thanh toán"; // Chưa thanh toán, Đã thanh toán
        public string? TransactionId { get; set; } // Mã giao dịch từ MoMo hoặc ngân hàng

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}
