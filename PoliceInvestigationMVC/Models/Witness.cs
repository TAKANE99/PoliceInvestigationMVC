using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class Witness
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ContactInformation { get; set; } = string.Empty;

        [Required]
        public string Statement { get; set; } = string.Empty;

        // Foreign key: the investigation this witness belongs to
        [Required]
        public int InvestigationId { get; set; }

        // Navigation property
        public Investigation? Investigation { get; set; }
    }
}
