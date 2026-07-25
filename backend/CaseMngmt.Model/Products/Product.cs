using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.Products
{
    public class Product : BaseModel
    {
        [Required]
        public Guid CompanyId { get; set; }

        [MaxLength(100)]
        public string? ProductCode { get; set; }

        [Required]
        public decimal StockQuantity { get; set; }

        [MaxLength(20)]
        public string? UnitOfMeasure { get; set; }

        public decimal? ProductionCapacityPerDay { get; set; }

        public decimal? UnitPrice { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}
