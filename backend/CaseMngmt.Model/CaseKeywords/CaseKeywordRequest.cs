using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.CaseKeywords
{
    public class CaseKeywordRequest : UpdateByModel
    {
        [Required]
        public Guid CaseId { get; set; }
        [Required]
        public List<CaseKeywordValue> KeywordValues { get; set; }
    }

    public class CaseKeywordAddRequest : UpdateByModel
    {
        [Required]
        public List<CaseKeywordValue> KeywordValues { get; set; }
    }
}
