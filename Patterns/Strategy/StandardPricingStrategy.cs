namespace TechMove.Patterns.Strategy
{
    public class StandardPricingStrategy : IPricingStrategy
    {
        private readonly ILogger<StandardPricingStrategy> _logger;

        public StandardPricingStrategy(ILogger<StandardPricingStrategy> logger)
        {
            _logger = logger;
        }

        public decimal CalculateFinalCost(decimal baseCost, string region)
        {
            _logger.LogInformation("Applying STANDARD pricing strategy for {Region}", region);
            
            // Standard pricing: no markup
            return baseCost;
        }

        public string GetStrategyName() => "Standard Pricing";
    }
}
