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
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Open";

        [Required]
        public string CaseType { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Foreign key: the investigating officer
        [Required]
        public int InvestigatingOfficerId { get; set; }

        // Navigation property
        public PoliceOfficer? InvestigatingOfficer { get; set; }
    }
}
