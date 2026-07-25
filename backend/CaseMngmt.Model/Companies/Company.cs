using CaseMngmt.Models.ApplicationUsers;
using CaseMngmt.Models.CompanyTemplates;
using CaseMngmt.Models.Customers;
using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.Companies
{
    public class Company : BaseModel
    {
        [MaxLength(12)]
        [Required]
        public string PhoneNumber { get; set; }

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

        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Customer> Customers { get; set; }
        public ICollection<CompanyTemplate> CompanyTemplates { get; set; }
    }
}
