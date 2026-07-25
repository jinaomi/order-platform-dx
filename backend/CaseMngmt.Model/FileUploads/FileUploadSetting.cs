namespace CaseMngmt.Models.FileUploads
{
    public class FileUploadSetting
    {
        public FileUploadSetting()
        {
        }

        public string AcceptTypes { get; set; }
        public string ValidFileTypes { get; set; }
        public string InvalidFileExtensions { get; set; }
        public string UploadFolder { get; set; }
    }
}
