using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class Evidence
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EvidenceType { get; set; } = string.Empty;

        // Foreign key: the investigation this evidence belongs to
        [Required]
        public int InvestigationId { get; set; }

        // Navigation property
        public Investigation? Investigation { get; set; }
    }
}
