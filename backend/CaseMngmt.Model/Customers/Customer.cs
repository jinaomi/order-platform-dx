using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.Customers
{
    public class Customer : BaseModel
    {
        [MaxLength(12)]
        [RegularExpression(@"^(\d{12})$", ErrorMessage = "Wrong PhoneNumber")]
        [Required]
        public string PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? PostCode1 { get; set; }

        [MaxLength(50)]
        public string? PostCode2 { get; set; }

        [MaxLength(256)]
        public string? StateProvince { get; set; }

        [MaxLength(256)]
        public string? City { get; set; }

        [MaxLength(256)]
        public string? Street { get; set; }

        [MaxLength(256)]
        public string? BuildingName { get; set; }

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        [MaxLength(3000)]
        public string? Note { get; set; }

        [Required]
        public Guid CompanyId { get; set; }
    }
}
