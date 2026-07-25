using CaseMngmt.Models.ApplicationRoles;
using CaseMngmt.Models.CaseKeywords;
using CaseMngmt.Models.Cases;
using CaseMngmt.Models.KeywordRoles;
using CaseMngmt.Models.Templates;
using Type = CaseMngmt.Models.Types.Type;

namespace CaseMngmt.Models.Keywords
{
    public class Keyword : BaseModel
    {
        public Guid TypeId { get; set; }
        public Guid TemplateId { get; set; }
        public int? MaxLength { get; set; }
        public bool IsRequired { get; set; }
        public bool CaseSearchable { get; set; }
        public bool DocumentSearchable { get; set; }
        public bool IsShowOnTemplate { get; set; } = true;
        public bool IsShowOnCaseList { get; set; }
        public int Order { get; set; }
        public bool IsHidden { get; set; } = false;
        public bool IsHiddenForUser { get; set; } = false;
        public string? OptionsList { get; set; }
        public Type Type { get; set; } = null!;
        public Template Template { get; set; } = null!;

        public List<Case> Cases { get; set; } = new();
        public List<CaseKeyword> CaseKeywords { get; set; }

        public List<ApplicationRole> Roles { get; set; } = new();
        public List<KeywordRole> KeywordRoles { get; set; }
    }
}
