namespace TechMove.Patterns.Observer
{
    public interface IContractObserver
    {
        void OnStatusChanged(int contractId, string oldStatus, string newStatus);
    }
}
