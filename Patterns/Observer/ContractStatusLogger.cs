namespace TechMove.Patterns.Observer
{
    public class ContractStatusLogger : IContractObserver
    {
        private readonly ILogger<ContractStatusLogger> _logger;

        public ContractStatusLogger(ILogger<ContractStatusLogger> logger)
        {
            _logger = logger;
        }

        public void OnStatusChanged(int contractId, string oldStatus, string newStatus)
        {
            _logger.LogInformation("Contract #{ContractId} status changed from {OldStatus} to {NewStatus}", 
                contractId, oldStatus, newStatus);
        }
    }
}
