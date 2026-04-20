namespace TechMove.Patterns.Strategy
{
    public interface IPricingStrategy
    {
        decimal CalculateFinalCost(decimal baseCost, string region);
        string GetStrategyName();
    }
}
