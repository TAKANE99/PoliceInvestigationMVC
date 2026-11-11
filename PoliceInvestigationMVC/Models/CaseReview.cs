using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class CaseReview
    {
        public int Id { get; set; }

        public int InvestigationId { get; set; }

        [Required]
        [StringLength(1000)]
        public string ReviewComments { get; set; } = string.Empty;

        public DateTime ReviewDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string ReviewStatus { get; set; } = "Pending";

        // Navigation property to the related Investigation being reviewed.
        public Investigation? Investigation { get; set; }
    }
}
