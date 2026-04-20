using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using TechMove.Services;
using Xunit;
using System.Net;

namespace TechMoveMvcFinal.Tests
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task GetUsdToZarRateAsync_WithValidResponse_ReturnsCorrectRate()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            var jsonResponse = @"{
                ""rates"": {
                    ""ZAR"": 18.75
                },
                ""base"": ""USD"",
                ""date"": ""2024-01-01""
            }";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await service.GetUsdToZarRateAsync();

            // Assert
            Assert.Equal(18.75m, result);
        }

        [Fact]
        public async Task GetUsdToZarRateAsync_WithApiFailure_ReturnsFallbackRate()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API unavailable"));

            // Act
            var result = await service.GetUsdToZarRateAsync();

            // Assert
            Assert.Equal(18.50m, result); // Fallback rate
        }

        [Fact]
        public async Task GetUsdToZarRateAsync_WithMissingZAR_ReturnsFallbackRate()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            var jsonResponse = @"{
                ""rates"": {
                    ""EUR"": 0.85,
                    ""GBP"": 0.75
                },
                ""base"": ""USD"",
                ""date"": ""2024-01-01""
            }";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await service.GetUsdToZarRateAsync();

            // Assert
            Assert.Equal(18.50m, result); // Fallback rate
        }

        [Fact]
        public async Task ConvertUsdToZarAsync_With100Usd_ReturnsCorrectConversion()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            var jsonResponse = @"{
                ""rates"": {
                    ""ZAR"": 18.75
                },
                ""base"": ""USD"",
                ""date"": ""2024-01-01""
            }";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await service.ConvertUsdToZarAsync(100);

            // Assert
            Assert.Equal(1875m, result);
        }

        [Fact]
        public async Task ConvertUsdToZarAsync_With50Usd_ReturnsCorrectConversion()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            var jsonResponse = @"{
                ""rates"": {
                    ""ZAR"": 18.50
                },
                ""base"": ""USD"",
                ""date"": ""2024-01-01""
            }";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await service.ConvertUsdToZarAsync(50);

            // Assert
            Assert.Equal(925m, result);
        }

        [Fact]
        public async Task ConvertUsdToZarAsync_WithZeroAmount_ReturnsZero()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            var jsonResponse = @"{
                ""rates"": {
                    ""ZAR"": 18.75
                },
                ""base"": ""USD"",
                ""date"": ""2024-01-01""
            }";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await service.ConvertUsdToZarAsync(0);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task ConvertUsdToZarAsync_WithApiFailure_UsesFallbackRate()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API unavailable"));

            // Act
            var result = await service.ConvertUsdToZarAsync(100);

            // Assert
            Assert.Equal(1850m, result); // 100 * 18.50 fallback rate
        }

        [Fact]
        public async Task GetUsdToZarRateAsync_CachesRate_ReducesApiCalls()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            var jsonResponse = @"{
                ""rates"": {
                    ""ZAR"": 18.75
                },
                ""base"": ""USD"",
                ""date"": ""2024-01-01""
            }";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act - Call twice
            var result1 = await service.GetUsdToZarRateAsync();
            var result2 = await service.GetUsdToZarRateAsync();

            // Assert
            Assert.Equal(18.75m, result1);
            Assert.Equal(18.75m, result2);

            // Verify HTTP request was only made once (second call used cache)
            mockHttpMessageHandler.Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task ConvertUsdToZarAsync_RoundsToTwoDecimalPlaces()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CurrencyService>>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var service = new CurrencyService(httpClient, mockLogger.Object);

            var jsonResponse = @"{
                ""rates"": {
                    ""ZAR"": 18.333333
                },
                ""base"": ""USD"",
                ""date"": ""2024-01-01""
            }";

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            // Act
            var result = await service.ConvertUsdToZarAsync(10);

            // Assert
            Assert.Equal(183.33m, result); // Rounded to 2 decimal places
        }
    }
}