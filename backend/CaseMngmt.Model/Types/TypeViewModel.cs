namespace CaseMngmt.Models.Types
{
    public class TypeViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Value { get; set; }
        public bool IsDefaultType { get; set; }
        public string? Source { get; set; }
        public string? Metadata { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
