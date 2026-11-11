using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class InvestigationOfficer
    {
        //Foreign key to the investigation.
        public int InvestigationId { get; set; }

        //Foreign key to the assigned police officer.
        public int PoliceOfficerId { get; set; }

        [Required]
        [StringLength(50)]
        public string TeamRole { get; set; } = "Team Member";

        public DateTime AssignedDate { get; set; } = DateTime.Now;

        //Navigation property to the related Investigation.
        public Investigation? Investigation { get; set; }

        //Navigation property to the related PoliceOfficer.
        public PoliceOfficer? PoliceOfficer { get; set; }
    }
}
