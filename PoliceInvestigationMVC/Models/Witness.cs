using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class Witness
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 10,
            ErrorMessage = "Contact number must be exactly 10 digits.")]
        [RegularExpression(@"^\d{10}$",
            ErrorMessage = "Contact number must contain exactly 10 digits.")]
        [Display(Name = "Contact Details")]
        public string ContactInformation { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Witness Statement")]
        public string Statement { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Statement Date")]
        [DataType(DataType.Date)]
        public DateTime StatementDate { get; set; } = DateTime.Today;

        // Foreign key: associated investigation
        [Required]
        [Display(Name = "Associated Case")]
        public int InvestigationId { get; set; }

        // Navigation property
        public Investigation? Investigation { get; set; }
    }
}
