using TechMove.Models;

namespace TechMove.Patterns.Factory
{
    public interface IServiceRequestFactory
    {
        ServiceRequest CreateServiceRequest(int contractId, string description, decimal costUsd, decimal costZar, decimal exchangeRate);
    }
}
