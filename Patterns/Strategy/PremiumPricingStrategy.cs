namespace TechMove.Patterns.Strategy
{
    public class PremiumPricingStrategy : IPricingStrategy
    {
        private readonly ILogger<PremiumPricingStrategy> _logger;

        public PremiumPricingStrategy(ILogger<PremiumPricingStrategy> logger)
        {
            _logger = logger;
        }

        public decimal CalculateFinalCost(decimal baseCost, string region)
        {
            // Premium: 20% markup + regional adjustment
            decimal baseMarkup = 1.20m;
            decimal regionalFactor = region switch
            {
                "Europe" => 1.05m,
                "North America" => 1.03m,
                _ => 1.0m
            };

            var finalCost = baseCost * baseMarkup * regionalFactor;
            
            _logger.LogInformation("Applying PREMIUM pricing strategy for {Region}. " +
                "Base: R{BaseCost}, Markup: 20%, Regional: {Regional}, Final: R{FinalCost}", 
                region, baseCost, regionalFactor, finalCost);
            
            return Math.Round(finalCost, 2);
        }

        public string GetStrategyName() => "Premium Pricing";
    }
}
