namespace TechMove.Patterns.Strategy
{
    public class RegionalPricingStrategy : IPricingStrategy
    {
        private readonly ILogger<RegionalPricingStrategy> _logger;

        public RegionalPricingStrategy(ILogger<RegionalPricingStrategy> logger)
        {
            _logger = logger;
        }

        public decimal CalculateFinalCost(decimal baseCost, string region)
        {
            decimal markup = region switch
            {
                "Europe" => 1.15m,        // 15% markup
                "North America" => 1.10m, // 10% markup
                "South Africa" => 1.05m,  // 5% markup
                _ => 1.0m
            };

            var finalCost = baseCost * markup;
            
            _logger.LogInformation("Applying REGIONAL pricing strategy for {Region}. " +
                "Base: R{BaseCost}, Markup: {Markup}, Final: R{FinalCost}", 
                region, baseCost, markup, finalCost);
            
            return Math.Round(finalCost, 2);
        }

        public string GetStrategyName() => "Regional Pricing";
    }
}