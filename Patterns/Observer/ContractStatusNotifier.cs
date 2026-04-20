namespace TechMove.Patterns.Observer
{
    public class ContractStatusNotifier : IContractObserver
    {
        private readonly ILogger<ContractStatusNotifier> _logger;

        public ContractStatusNotifier(ILogger<ContractStatusNotifier> logger)
        {
            _logger = logger;
        }

        public void OnStatusChanged(int contractId, string oldStatus, string newStatus)
        {
            // In a real app, this would send email/SMS notifications
            _logger.LogInformation("NOTIFICATION: Contract #{ContractId} status changed to {NewStatus}. " +
                "Stakeholders have been notified.", contractId, newStatus);

            // Check if contract became expired
            if (newStatus == "Expired")
            {
                _logger.LogWarning("ALERT: Contract #{ContractId} has EXPIRED. " +
                    "No new service requests can be created.", contractId);
            }
        }
    }
}
