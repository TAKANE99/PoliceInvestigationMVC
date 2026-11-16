using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    // Stores details of a person suspected of involvement in an investigation.
    public class InvestigationSuspect
    {
        public int Id { get; set; }

        [Required]
        public int InvestigationId { get; set; }

        public Investigation? Investigation { get; set; }

        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? IDNumber { get; set; }

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [StringLength(500)]
        public string? PhysicalDescription { get; set; }

        [Required]
        public string IdentificationStatus { get; set; } = "Unknown";

        [Required]
        public string ArrestStatus { get; set; } = "Not Arrested";

        [StringLength(150)]
        public string? IdentificationMethod { get; set; }

        public string? Notes { get; set; }

        public DateTime DateRecorded { get; set; } = DateTime.Now;
        // Stores the suspect's photograph.
        public string? IDPhotoFileName { get; set; }
        public string? IDPhotoFilePath { get; set; }

        // Stores the suspect's identity document copy.
        public string? IDCopyFileName { get; set; }
        public string? IDCopyFilePath { get; set; }
    }
}