namespace TechMove.Patterns.Observer
{
    public class ContractObservable
    {
        private readonly List<IContractObserver> _observers = new();

        public void Attach(IContractObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IContractObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyStatusChange(int contractId, string oldStatus, string newStatus)
        {
            foreach (var observer in _observers)
            {
                observer.OnStatusChanged(contractId, oldStatus, newStatus);
            }
        }
    }
}
