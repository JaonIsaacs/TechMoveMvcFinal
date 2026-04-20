using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public int ContractId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0")]
        [Display(Name = "Cost (USD)")]
        public decimal CostUsd { get; set; }

        [Required]
        [Display(Name = "Cost (ZAR)")]
        public decimal CostZar { get; set; }

        [Display(Name = "Exchange Rate")]
        public decimal ExchangeRate { get; set; }

        [Required]
        public ServiceRequestStatus Status { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        // Navigation property
        public Contract Contract { get; set; } = null!;

        // Computed property for backward compatibility - NOT MAPPED to database
        [NotMapped]
        public decimal Cost => CostZar;
    }

    public enum ServiceRequestStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
}
