using System.ComponentModel.DataAnnotations;

namespace TechMove.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Region { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
