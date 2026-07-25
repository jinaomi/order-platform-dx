namespace CaseMngmt.Models.FileTypes
{
    public class FileTypeViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
