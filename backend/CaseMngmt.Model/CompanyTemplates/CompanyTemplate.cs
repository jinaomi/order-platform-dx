using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models.CompanyTemplates
{
    public class CompanyTemplate
    {
        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public Guid TemplateId { get; set; }
    }
}
