using TechMove.Models;

namespace TechMove.Patterns.Factory
{
    public class PremiumServiceRequestFactory : IServiceRequestFactory
    {
        private readonly ILogger<PremiumServiceRequestFactory> _logger;

        public PremiumServiceRequestFactory(ILogger<PremiumServiceRequestFactory> logger)
        {
            _logger = logger;
        }

        public ServiceRequest CreateServiceRequest(int contractId, string description, decimal costUsd, decimal costZar, decimal exchangeRate)
        {
            _logger.LogInformation("Creating PREMIUM service request for contract #{ContractId} with priority processing", contractId);

            // Premium service requests get priority status
            return new ServiceRequest
            {
                ContractId = contractId,
                Description = $"[PREMIUM] {description}",
                CostUsd = costUsd,
                CostZar = costZar,
                ExchangeRate = exchangeRate,
                Status = ServiceRequestStatus.InProgress, // Premium starts as InProgress
                CreatedDate = DateTime.Now
            };
        }
    }
}
