using System.ComponentModel.DataAnnotations;

namespace TechMove.Models
{
    public class Contract
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a client")]
        [Display(Name = "Client")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid client")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public ContractStatus Status { get; set; }

        [Display(Name = "Signed Agreement")]
        public string? SignedAgreementPath { get; set; }

        // Navigation properties
        public Client Client { get; set; } = null!;
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }

    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        OnHold
    }
}
