using CaseMngmt.Models.FileUploads;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CaseMngmt.Models.CaseKeywords
{
    public class CaseKeywordFileUpload
    {
        [Required]
        public Guid CaseId { get; set; }
        [Required]
        public Guid FileTypeId { get; set; }
        public string? FileName { get; set; }
        [Required]
        public IFormFile FileToUpload { get; set; }

        public bool Validate(FileUploadSetting fileSetting)
        {
            try
            {
                if (string.IsNullOrEmpty(FileName))
                {
                    FileName = FileToUpload.FileName;
                }

                if (!IsValidFilename(FileName))
                {
                    return false;
                }

                string fileExt = Path.GetExtension(FileName).ToLower();
                if (string.IsNullOrEmpty(fileExt))
                {
                    fileExt = Path.GetExtension(FileToUpload.FileName).ToLower();
                    FileName = $"{FileName}{fileExt}";
                }

                var validFileTypes = fileSetting.AcceptTypes.Split(',').ToList();

                if (validFileTypes.Contains(fileExt))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool IsValidFilename(string fileName)
        {
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]");
            if (containsABadCharacter.IsMatch(fileName))
            {
                return false;
            }

            return true;
        }
    }
}
