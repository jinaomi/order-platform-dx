using System.ComponentModel.DataAnnotations;

namespace CaseMngmt.Models
{
    public class CommonModel
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
