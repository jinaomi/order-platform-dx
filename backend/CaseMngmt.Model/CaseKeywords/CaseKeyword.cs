using CaseMngmt.Models.Cases;
using CaseMngmt.Models.Keywords;
using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.CaseKeywords
{
    public class CaseKeyword
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CaseId { get; set; }

        [Required]
        public Guid KeywordId { get; set; }

        public string Value { get; set; }
        public bool Deleted { get; set; }

        public Case Case { get; set; } = null!;
        public Keyword Keyword { get; set; } = null!;
    }
}
