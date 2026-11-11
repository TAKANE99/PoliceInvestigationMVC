using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class Evidence
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Evidence Reference")]
        public string EvidenceReference { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Evidence Type")]
        public string EvidenceType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Collection Date")]
        public DateTime? DateCollected { get; set; }

        [Required]
        [Display(Name = "Collected By")]
        public int? CollectedById { get; set; }

        public PoliceOfficer? CollectedBy { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Storage Location")]
        public string StorageLocation { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Chain of Custody")]
        public string ChainOfCustody { get; set; } = string.Empty;

        // Investigation relationship
        [Required]
        public int InvestigationId { get; set; }

        public Investigation? Investigation { get; set; }

        // Existing file upload properties
        public string? FileName { get; set; }

        public string? FilePath { get; set; }
    }
}
