using TechMove.Models;
using Microsoft.Extensions.Logging;

namespace TechMove.Patterns.Factory
{
    public class StandardServiceRequestFactory : IServiceRequestFactory
    {
        private readonly ILogger<StandardServiceRequestFactory> _logger;

        public StandardServiceRequestFactory(ILogger<StandardServiceRequestFactory> logger)
        {
            _logger = logger;
        }

        public ServiceRequest CreateServiceRequest(int contractId, string description, decimal costUsd, decimal costZar, decimal exchangeRate)
        {
            _logger.LogInformation("Creating STANDARD service request for contract #{ContractId}", contractId);

            return new ServiceRequest
            {
                ContractId = contractId,
                Description = description,
                CostUsd = costUsd,
                CostZar = costZar,
                ExchangeRate = exchangeRate,
                Status = ServiceRequestStatus.Pending,
                CreatedDate = DateTime.Now
            };
        }
    }
}
