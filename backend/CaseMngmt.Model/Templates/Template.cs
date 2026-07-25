using CaseMngmt.Models.CompanyTemplates;
using CaseMngmt.Models.Keywords;

namespace CaseMngmt.Models.Templates
{
    public class Template : BaseModel
    {
        public bool IsDefault { get; set; } = false;
        public virtual ICollection<Keyword> Keywords { get; set; }
        public virtual ICollection<CompanyTemplate> CompanyTemplate { get; set; }
    }
}
