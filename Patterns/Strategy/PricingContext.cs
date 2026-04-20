namespace TechMove.Patterns.Strategy
{
    public class PricingContext
    {
        private IPricingStrategy _strategy;
        private readonly ILogger<PricingContext> _logger;

        public PricingContext(IPricingStrategy strategy, ILogger<PricingContext> logger)
        {
            _strategy = strategy;
            _logger = logger;
        }

        public void SetStrategy(IPricingStrategy strategy)
        {
            _strategy = strategy;
            _logger.LogInformation("Pricing strategy changed to: {Strategy}", strategy.GetStrategyName());
        }

        public decimal CalculatePrice(decimal baseCost, string region)
        {
            return _strategy.CalculateFinalCost(baseCost, region);
        }

        public string GetCurrentStrategy() => _strategy.GetStrategyName();
    }
}
