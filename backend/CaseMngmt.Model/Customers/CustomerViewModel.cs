using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.Customers
{
    public class CustomerViewModel
    {
        public Guid Id { get; set; }

        [MaxLength(256)]
        public string? Name { get; set; }

        [MaxLength(12)]
        [Required]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        [Required]
        public string? PostCode1 { get; set; }

        [MaxLength(50)]
        [Required]
        public string? PostCode2 { get; set; }

        [MaxLength(256)]
        [Required]
        public string? StateProvince { get; set; }

        [MaxLength(256)]
        [Required]
        public string? City { get; set; }

        [MaxLength(256)]
        [Required]
        public string? Street { get; set; }

        [MaxLength(256)]
        [Required]
        public string? BuildingName { get; set; }

        [MaxLength(50)]
        [Required]
        public string? RoomNumber { get; set; }

        [MaxLength(3000)]
        public string? Note { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
