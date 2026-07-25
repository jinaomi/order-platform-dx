namespace CaseMngmt.Models.FileUploads
{
    public class DeleteFileRequest
    {
        public Guid KeywordId { get; set; }
        public Guid CaseId { get; set; }
        public string FileName { get; set; }
    }

    public class DownloadFileRequest
    {
        public Guid CaseId { get; set; }
        public string FileName { get; set; }
    }
}
