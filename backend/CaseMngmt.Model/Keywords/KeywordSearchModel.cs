using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.Keywords
{
    public class KeywordSearchModel
    {
        public Guid KeywordId { get; set; }
        [MaxLength(256)]
        public string? KeywordName { get; set; }
        public int? MaxLength { get; set; }
        public List<string> Metadata { get; set; }
        public int Order { get; set; }
        public Guid TypeId { get; set; }
        public string? TypeName { get; set; }
        public string? TypeValue { get; set; }
        public string Value { get; set; } = string.Empty;
        public string? FromValue { get; set; } = string.Empty;
        public string? ToValue { get; set; } = string.Empty;
        public bool? FromTo { get; set; } = false;
    }
}
