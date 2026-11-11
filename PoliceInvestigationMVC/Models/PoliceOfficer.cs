using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class PoliceOfficer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Service Number")]
        public string ServiceNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Rank { get; set; } = string.Empty;

        [Required]
        public int StationId { get; set; }

        public PoliceStation? Station { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Display(Name = "Employment Status")]
        public string EmploymentStatus { get; set; } = "Active";

        public ICollection<InvestigationOfficer> InvestigationOfficers
        {
            get; set;
        } = new List<InvestigationOfficer>();
    }
}
