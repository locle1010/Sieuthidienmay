using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_dienmay.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastModifiedDate { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
            LastModifiedDate = DateTime.Now;
        }

        public void RemoveItem(int productId)
        {
            var items = Items.Where(i => i.ProductId == productId).ToList();
            foreach (var item in items)
            {
                Items.Remove(item);
            }
            LastModifiedDate = DateTime.Now;
        }

        public void Clear()
        {
            Items.Clear();
            LastModifiedDate = DateTime.Now;
        }

        public bool IncreaseQuantity(int productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity++;
                LastModifiedDate = DateTime.Now;
                return true;
            }
            return false;
        }

        public bool DecreaseQuantity(int productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                    LastModifiedDate = DateTime.Now;
                    return true;
                }
                else
                {
                    RemoveItem(productId);
                    return true;
                }
            }
            return false;
        }
    }
}
