using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using TechMove.Patterns.Strategy;

namespace TechMove.Tests
{
    public class PricingStrategyTests
    {
        [Fact]
        public void StandardPricing_AppliesNoMarkup()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<StandardPricingStrategy>>();
            var strategy = new StandardPricingStrategy(mockLogger.Object);
            decimal baseCost = 1000m;

            // Act
            var result = strategy.CalculateFinalCost(baseCost, "Any Region");

            // Assert
            Assert.Equal(1000m, result);
        }

        [Theory]
        [InlineData("Europe", 1000, 1150)]      // 15% markup
        [InlineData("North America", 1000, 1100)] // 10% markup
        [InlineData("South Africa", 1000, 1050)]  // 5% markup
        [InlineData("Other", 1000, 1000)]         // 0% markup
        public void RegionalPricing_AppliesCorrectMarkup(string region, decimal baseCost, decimal expected)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<RegionalPricingStrategy>>();
            var strategy = new RegionalPricingStrategy(mockLogger.Object);

            // Act
            var result = strategy.CalculateFinalCost(baseCost, region);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void PremiumPricing_Applies20PercentMarkup()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PremiumPricingStrategy>>();
            var strategy = new PremiumPricingStrategy(mockLogger.Object);
            decimal baseCost = 1000m;

            // Act
            var result = strategy.CalculateFinalCost(baseCost, "Other");

            // Assert
            Assert.Equal(1200m, result); // 20% markup
        }
    }
}
