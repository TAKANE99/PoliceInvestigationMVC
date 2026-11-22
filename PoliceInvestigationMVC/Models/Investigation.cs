using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class Investigation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CaseNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Complainant")]
        public string Complainant { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Crime Category")]
        public string CrimeCategory { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Open";

        public bool IsReviewed { get; set; } = false;

        [Required]
        [Display(Name = "Case Type")]
        public string CaseType { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Foreign key: the officer assigned to investigate the case.
        // Nullable because a case can exist before an officer is assigned.
        public int? InvestigatingOfficerId { get; set; }

        // Navigation properties linking the investigation to its review and assigned officers.
        public CaseReview? CaseReview { get; set; }
        public PoliceOfficer? InvestigatingOfficer { get; set; }

        // Officers assigned to this investigation team.
        public ICollection<InvestigationOfficer> InvestigationOfficers
        { get; set; } = new List<InvestigationOfficer>();

        // Allows one investigation to have multiple suspected persons.
        public ICollection<InvestigationSuspect> InvestigationSuspects { get; set; }
            = new List<InvestigationSuspect>();

        // Stores all progress updates recorded for this investigation.
        public ICollection<InvestigationProgress> ProgressUpdates { get; set; }
            = new List<InvestigationProgress>();

    }
}
