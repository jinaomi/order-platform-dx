using CaseMngmt.Models.ApplicationRoles;
using CaseMngmt.Models.Keywords;
using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.KeywordRoles
{
    public class KeywordRole
    {
        [Required]
        public Guid KeywordId { get; set; }

        [Required]
        public Guid RoleId { get; set; }

        public ApplicationRole ApplicationRole { get; set; } = null!;
        public Keyword Keyword { get; set; } = null!;
    }
}
