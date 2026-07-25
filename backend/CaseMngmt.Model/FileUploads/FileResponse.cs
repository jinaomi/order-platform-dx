namespace CaseMngmt.Models.FileUploads
{
    public class FileResponse : FileUploadResponse
    {
        public Guid KeywordId { get; set; }
    }

    public class FileUploadResponse
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public bool IsImage { get; set; } = false;
    }
}
