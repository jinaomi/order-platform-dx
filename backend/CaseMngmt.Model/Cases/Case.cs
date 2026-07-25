using CaseMngmt.Models.CaseKeywords;
using CaseMngmt.Models.Keywords;

namespace CaseMngmt.Models.Cases
{
    public class Case : BaseModel
    {
        public string Status { get; set; }
        public List<Keyword> Keywords { get; set; } = new();
        public List<CaseKeyword> CaseKeywords { get; set; }
    }
}
