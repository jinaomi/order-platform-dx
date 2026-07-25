namespace CaseMngmt.Models
{
    public class AWSSetting
    {
        public AWSSetting()
        {
        }

        public string S3Bucket { get; set; }
        public string ACCESS_KEY { get; set; }
        public string SECRET_KEY { get; set; }
        public string UploadFolder { get; set; }
    }
}
