using TechMove.Data;
using TechMove.Models;
using Microsoft.EntityFrameworkCore;

namespace TechMove.Patterns.Factory
{
    public class ServiceRequestFactoryProvider
    {
        private readonly ApplicationDbContext _context;
        private readonly StandardServiceRequestFactory _standardFactory;
        private readonly PremiumServiceRequestFactory _premiumFactory;

        public ServiceRequestFactoryProvider(
            ApplicationDbContext context,
            StandardServiceRequestFactory standardFactory,
            PremiumServiceRequestFactory premiumFactory)
        {
            _context = context;
            _standardFactory = standardFactory;
            _premiumFactory = premiumFactory;
        }

        public async Task<IServiceRequestFactory> GetFactoryAsync(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == contractId);

            // Premium regions get premium factory
            if (contract?.Client.Region == "Europe" || contract?.Client.Region == "Premium")
            {
                return _premiumFactory;
            }

            return _standardFactory;
        }
    }
}
