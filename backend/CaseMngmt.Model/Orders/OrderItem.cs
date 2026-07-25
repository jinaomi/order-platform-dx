using CaseMngmt.Models.Products;
using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.Orders
{
    public class OrderItem : BaseModel
    {
        [Required]
        public Guid OrderId { get; set; }

        public Guid? ProductId { get; set; }

        [Required]
        [MaxLength(256)]
        public string ProductNameRaw { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal LineAmount { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public Order? Order { get; set; }

        public Product? Product { get; set; }
    }
}
