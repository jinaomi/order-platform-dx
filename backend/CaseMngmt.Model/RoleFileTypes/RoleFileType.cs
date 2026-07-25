using CaseMngmt.Models.ApplicationRoles;
using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.RoleFileTypes
{
    public class RoleFileType
    {
        [Required]
        public Guid RoleId { get; set; }

        [Required]
        public Guid TypeId { get; set; }

        public ApplicationRole ApplicationRole { get; set; } = null!;
        public Types.Type FileType { get; set; } = null!;
    }
}
