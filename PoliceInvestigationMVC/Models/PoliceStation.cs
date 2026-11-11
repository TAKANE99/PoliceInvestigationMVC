using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class PoliceStation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Station Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Station Code")]
        public string StationCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Physical Address")]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(10)]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Station Commander")]
        public string StationCommander { get; set; } = string.Empty;

        public ICollection<PoliceOfficer> PoliceOfficers { get; set; } = new List<PoliceOfficer>();
    }
}