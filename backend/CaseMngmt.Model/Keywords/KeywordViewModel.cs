using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.Keywords
{
    public class KeywordViewModel
    {
        public Guid KeywordId { get; set; }
        [MaxLength(256)]
        public string? KeywordName { get; set; }
        public Guid TypeId { get; set; }
        public string? TypeName { get; set; }
        public string? TypeValue { get; set; }
        public Guid TemplateId { get; set; }
        public int? MaxLength { get; set; }
        public bool IsRequired { get; set; }
        public bool Searchable { get; set; }
        public bool DocumentSearchable { get; set; }
        public bool IsShowOnTemplate { get; set; }
        public bool IsShowOnCaseList { get; set; }
        public int Order { get; set; }
        public List<string> Metadata { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public bool IsHidden { get; set; }
        public bool IsHiddenForUser { get; set; }
        public string? OptionsList { get; set; }
    }
}
