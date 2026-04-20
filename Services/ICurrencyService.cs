namespace TechMove.Services
{
    public interface ICurrencyService
    {
        Task<decimal> GetUsdToZarRateAsync();
        Task<decimal> ConvertUsdToZarAsync(decimal usdAmount);
    }
}