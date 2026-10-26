using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class PoliceOfficer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Rank { get; set; } = string.Empty;

        [Required]
        public int StationId { get; set; }

        // Navigation property: the station this officer belongs to
        public PoliceStation? Station { get; set; }
    }
}
