using Moq;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using TechMove.Services;
using Xunit;

namespace TechMove.Tests
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task ConvertUsdToZar_WithValidRate_ReturnsCorrectConversion()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockLogger = new Mock<ILogger<CurrencyService>>();

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            decimal usdAmount = 100m;
            decimal expectedRate = 18.50m;
            decimal expectedZar = 1850m;

            // Act
            var result = await service.ConvertUsdToZarAsync(usdAmount);

            // Assert - using fallback rate
            Assert.Equal(expectedZar, result);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 18.50)]
        [InlineData(100, 1850)]
        [InlineData(50.50, 934.25)]
        public async Task ConvertUsdToZar_WithDifferentAmounts_CalculatesCorrectly(decimal usd, decimal expectedZar)
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            // Act
            var result = await service.ConvertUsdToZarAsync(usd);

            // Assert
            Assert.Equal(expectedZar, result);
        }

        [Fact]
        public async Task GetUsdToZarRate_WhenApiFails_ReturnsFallbackRate()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API unavailable"));

            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            // Act
            var result = await service.GetUsdToZarRateAsync();

            // Assert
            Assert.Equal(18.50m, result); // Fallback rate
        }
    }
}