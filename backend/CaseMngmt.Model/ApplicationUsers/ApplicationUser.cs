using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaseMngmt.Models.ApplicationUsers
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    [Table("AspNetUsers")]
    public class ApplicationUser : IdentityUser<Guid>
    {
        public Guid CompanyId { get; set; }
    }
}
