using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class InvestigationProgress
    {
        public int Id { get; set; }

        // Identifies the investigation being updated.
        [Required]
        public int InvestigationId { get; set; }

        // Identifies the officer who recorded the update.
        [Required]
        public int PoliceOfficerId { get; set; }

        // Date on which the investigation activity took place.
        [Required]
        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; } = DateTime.Now;

        // Type of activity performed.
        [Required]
        [StringLength(100)]
        [Display(Name = "Activity Type")]
        public string ActivityType { get; set; } = string.Empty;

        // Details of the work performed during the investigation.
        [Required]
        [StringLength(2000)]
        [Display(Name = "Progress Notes")]
        public string Description { get; set; } = string.Empty;

        // Date and time when the update was recorded.
        public DateTime DateRecorded { get; set; } = DateTime.Now;

        // Navigation properties.
        public Investigation? Investigation { get; set; }

        public PoliceOfficer? PoliceOfficer { get; set; }
    }
}