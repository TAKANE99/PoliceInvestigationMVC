using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    public class PoliceStation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;
    }
}