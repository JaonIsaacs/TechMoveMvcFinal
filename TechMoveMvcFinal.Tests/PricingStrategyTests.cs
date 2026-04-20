using TechMove.Models;
using Xunit;

namespace TechMoveMvcFinal.Tests
{
    public class PricingStrategyTests
    {
        [Theory]
        [InlineData(100, 0, 100)]
        [InlineData(100, 10, 90)]
        [InlineData(100, 50, 50)]
        [InlineData(100, 100, 0)]
        public void CalculateDiscountedPrice_WithVariousDiscounts_ReturnsCorrectPrice(
            decimal basePrice, decimal discountPercent, decimal expected)
        {
            // Act
            var result = PricingStrategy.CalculateDiscountedPrice(basePrice, discountPercent);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateDiscountedPrice_WithNegativePrice_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                PricingStrategy.CalculateDiscountedPrice(-100, 10));
        }

        [Fact]
        public void CalculateDiscountedPrice_WithInvalidDiscount_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                PricingStrategy.CalculateDiscountedPrice(100, 101));
        }

        [Theory]
        [InlineData(ServiceType.LocalMove, 100, 100)]
        [InlineData(ServiceType.LongDistanceMove, 100, 150)]
        [InlineData(ServiceType.InternationalMove, 100, 300)]
        public void CalculateServicePrice_WithDifferentServiceTypes_ReturnsCorrectMultiplier(
            ServiceType serviceType, decimal basePrice, decimal expected)
        {
            // Act
            var result = PricingStrategy.CalculateServicePrice(basePrice, serviceType);

            // Assert
            Assert.Equal(expected, result);
        }
    }

    public static class PricingStrategy
    {
        public static decimal CalculateDiscountedPrice(decimal basePrice, decimal discountPercent)
        {
            if (basePrice < 0)
                throw new ArgumentException("Base price cannot be negative", nameof(basePrice));
            
            if (discountPercent < 0 || discountPercent > 100)
                throw new ArgumentException("Discount must be between 0 and 100", nameof(discountPercent));

            return basePrice - (basePrice * discountPercent / 100);
        }

        public static decimal CalculateServicePrice(decimal basePrice, ServiceType serviceType)
        {
            return serviceType switch
            {
                ServiceType.LocalMove => basePrice,
                ServiceType.LongDistanceMove => basePrice * 1.5m,
                ServiceType.InternationalMove => basePrice * 3.0m,
                _ => basePrice
            };
        }
    }

    public enum ServiceType
    {
        LocalMove,
        LongDistanceMove,
        InternationalMove
    }
}