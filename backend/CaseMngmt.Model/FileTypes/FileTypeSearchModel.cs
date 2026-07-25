namespace CaseMngmt.Models.FileTypes
{
    public class FileTypeSearchModel
    {
        public string Name { get; set; }
        public List<FileTypeModel> FileTypes { get; set; }
    }

    public class FileTypeModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
