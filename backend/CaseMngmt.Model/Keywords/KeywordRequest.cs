using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.Keywords
{
    public class KeywordRequest
    {
        [MaxLength(256)]
        [Required]
        public string Name { get; set; }
        [Required]
        public Guid TypeId { get; set; }
        public int MaxLength { get; set; }
        public bool IsRequired { get; set; }
        public bool CaseSearchable { get; set; }
        public bool DocumentSearchable { get; set; }
        public bool IsShowOnCaseList { get; set; }
        public bool IsShowOnTemplate { get; set; }
        public int Order { get; set; }
        public string? Metadata { get; set; }
        public string? Source { get; set; }
        [Required]
        public Guid TemplateId { get; set; }
        public string? OptionsList { get; set; }
        public bool IsHidden { get; set; } = false;
        public bool IsHiddenForUser { get; set; } = false;
    }
}
