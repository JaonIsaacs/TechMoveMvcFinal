using System.Text.Json;
using System.Text.Json.Serialization;

namespace TechMove.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;
        private const string API_URL = "https://api.exchangerate-api.com/v4/latest/USD";
        private decimal _cachedRate = 0;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private readonly TimeSpan _cacheLifetime = TimeSpan.FromHours(1);

        public CurrencyService(HttpClient httpClient, ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            // Return cached rate if still valid
            if (_cachedRate > 0 && DateTime.Now < _cacheExpiry)
            {
                _logger.LogInformation("Using cached USD to ZAR rate: {Rate}", _cachedRate);
                return _cachedRate;
            }

            try
            {
                _logger.LogInformation("Fetching USD to ZAR exchange rate from API...");
                
                var response = await _httpClient.GetAsync(API_URL);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response received. Length: {Length}", json.Length);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<ExchangeRateResponse>(json, options);

                if (data?.Rates != null && data.Rates.Count > 0)
                {
                    _logger.LogInformation("Found {Count} rates in response", data.Rates.Count);
                    
                    // Log first few rates to see what's available
                    foreach (var rate in data.Rates.Take(5))
                    {
                        _logger.LogInformation("Rate: {Currency} = {Value}", rate.Key, rate.Value);
                    }

                    // Try to find ZAR
                    if (data.Rates.TryGetValue("ZAR", out var zarRate))
                    {
                        _cachedRate = zarRate;
                        _cacheExpiry = DateTime.Now.Add(_cacheLifetime);
                        
                        _logger.LogInformation("Successfully fetched USD to ZAR rate: {Rate}", _cachedRate);
                        return _cachedRate;
                    }
                    else
                    {
                        _logger.LogWarning("ZAR not found in rates. Available currencies: {Currencies}", 
                            string.Join(", ", data.Rates.Keys.Take(10)));
                    }
                }
                else
                {
                    _logger.LogWarning("No rates found in API response");
                }

                _logger.LogWarning("ZAR rate not found in API response. Using fallback rate of 18.50");
                _cachedRate = 18.50m;
                _cacheExpiry = DateTime.Now.Add(_cacheLifetime);
                return _cachedRate;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching exchange rate. Using fallback rate.");
                _cachedRate = 18.50m;
                _cacheExpiry = DateTime.Now.Add(_cacheLifetime);
                return _cachedRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rate. Using fallback rate.");
                _cachedRate = 18.50m;
                _cacheExpiry = DateTime.Now.Add(_cacheLifetime);
                return _cachedRate;
            }
        }

        public async Task<decimal> ConvertUsdToZarAsync(decimal usdAmount)
        {
            try
            {
                var rate = await GetUsdToZarRateAsync();
                var zarAmount = usdAmount * rate;
                
                _logger.LogInformation("Converted ${UsdAmount} to R{ZarAmount} (Rate: {Rate})", 
                    usdAmount, zarAmount, rate);
                
                return Math.Round(zarAmount, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting currency. Using fallback rate.");
                return Math.Round(usdAmount * 18.50m, 2);
            }
        }

        private class ExchangeRateResponse
        {
            [JsonPropertyName("rates")]
            public Dictionary<string, decimal>? Rates { get; set; }

            [JsonPropertyName("base")]
            public string? Base { get; set; }

            [JsonPropertyName("date")]
            public string? Date { get; set; }
        }
    }
}
