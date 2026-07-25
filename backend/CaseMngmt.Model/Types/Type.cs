using CaseMngmt.Models.ApplicationRoles;
using CaseMngmt.Models.Keywords;
using CaseMngmt.Models.RoleFileTypes;

namespace CaseMngmt.Models.Types
{
    public class Type : BaseModel
    {
        public bool IsDefaultType { get; set; }
        public bool IsFileType { get; set; }
        public string Value { get; set; }
        public string? Source { get; set; }
        public string? Metadata { get; set; }
        public List<Keyword> Keywords { get; set; }
        public List<ApplicationRole> Roles { get; set; }
        public List<RoleFileType> RoleFileTypes { get; set; }
    }
}
