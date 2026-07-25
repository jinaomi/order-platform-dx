using CaseMngmt.Models.FileTypes;
using CaseMngmt.Models.KeywordRoles;
using CaseMngmt.Models.Keywords;
using CaseMngmt.Models.RoleFileTypes;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaseMngmt.Models.ApplicationRoles
{
    [Table("AspNetRoles")]
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string name) : base(name) { }

        public List<Keyword> Keywords { get; set; } = new();
        public List<KeywordRole> KeywordRoles { get; }

        public List<Types.Type> FileTypes { get; set; } = new();
        public List<RoleFileType> RoleFileTypes { get; set; }
    }
}
