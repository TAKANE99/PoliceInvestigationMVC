using System.ComponentModel.DataAnnotations;

namespace PoliceInvestigationMVC.Models
{
    // Stores each photo or document uploaded for an evidence record.
    public class EvidenceFile
    {
        public int Id { get; set; }

        [Required]
        public int EvidenceId { get; set; }

        public Evidence? Evidence { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public DateTime DateUploaded { get; set; } = DateTime.Now;
    }
}