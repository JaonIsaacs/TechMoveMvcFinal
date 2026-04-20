using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;
using TechMove.Patterns.Factory;
using TechMove.Patterns.Strategy;
using Microsoft.Extensions.Logging;

namespace TechMove.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly ServiceRequestFactoryProvider _factoryProvider;
        private readonly PricingContext _pricingContext;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            ApplicationDbContext context,
            ICurrencyService currencyService,
            ServiceRequestFactoryProvider factoryProvider,
            PricingContext pricingContext,
            ILogger<ServiceRequestsController> logger)
        {
            _context = context;
            _currencyService = currencyService;
            _factoryProvider = factoryProvider;
            _pricingContext = pricingContext;
            _logger = logger;
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            var serviceRequests = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                    .ThenInclude(c => c.Client)
                .OrderByDescending(sr => sr.CreatedDate)
                .ToListAsync();

            return View(serviceRequests);
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create(int? contractId)
        {
            await LoadContractsAsync();

            var exchangeRate = await _currencyService.GetUsdToZarRateAsync();
            ViewBag.CurrentExchangeRate = exchangeRate;
            ViewBag.ApiUnavailable = exchangeRate == 18.50m;

            var model = new ServiceRequestCreateViewModel();
            if (contractId.HasValue)
            {
                model.ContractId = contractId.Value;
            }

            return View(model);
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestCreateViewModel model)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == model.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("ContractId", "Please select a valid contract.");
            }
            else
            {
                // WORKFLOW VALIDATION - Check contract status
                if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
                {
                    ModelState.AddModelError("ContractId",
                        $"Cannot create service request for a contract with status '{contract.Status}'. " +
                        $"Only Active or Draft contracts can have service requests.");
                    _logger.LogWarning("Attempt to create service request for {Status} contract #{ContractId}",
                        contract.Status, contract.Id);
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadContractsAsync();
                var exchangeRate = await _currencyService.GetUsdToZarRateAsync();
                ViewBag.CurrentExchangeRate = exchangeRate;
                ViewBag.ApiUnavailable = exchangeRate == 18.50m;
                return View(model);
            }

            try
            {
                var exchangeRate = await _currencyService.GetUsdToZarRateAsync();
                var baseCostZar = await _currencyService.ConvertUsdToZarAsync(model.CostUsd);

                // STRATEGY PATTERN: Apply pricing strategy based on region
                var finalCostZar = _pricingContext.CalculatePrice(baseCostZar, contract!.Client.Region);
                var pricingStrategy = _pricingContext.GetCurrentStrategy();

                _logger.LogInformation("Applied {Strategy} for {Region}. Base: R{Base}, Final: R{Final}",
                    pricingStrategy, contract.Client.Region, baseCostZar, finalCostZar);

                // FACTORY PATTERN: Use appropriate factory based on contract
                var factory = await _factoryProvider.GetFactoryAsync(model.ContractId);
                var serviceRequest = factory.CreateServiceRequest(
                    model.ContractId,
                    model.Description,
                    model.CostUsd,
                    finalCostZar,
                    exchangeRate
                );

                _context.ServiceRequests.Add(serviceRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Service request created with ID: {Id} using {Factory}. " +
                    "USD: ${Usd}, Base ZAR: R{BaseZar}, Final ZAR: R{FinalZar}, Strategy: {Strategy}",
                    serviceRequest.Id, factory.GetType().Name,
                    serviceRequest.CostUsd, baseCostZar, serviceRequest.CostZar, pricingStrategy);

                TempData["SuccessMessage"] = $"Service request created successfully using {pricingStrategy}! " +
                    $"USD ${model.CostUsd:N2} → Base ZAR R{baseCostZar:N2} → Final ZAR R{finalCostZar:N2}";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request");
                TempData["ErrorMessage"] = $"Error creating service request: {ex.Message}";
                await LoadContractsAsync();
                var exchangeRate = await _currencyService.GetUsdToZarRateAsync();
                ViewBag.CurrentExchangeRate = exchangeRate;
                ViewBag.ApiUnavailable = exchangeRate == 18.50m;
                return View(model);
            }
        }

        // POST: ServiceRequests/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Service Request #{id} deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadContractsAsync()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active || c.Status == ContractStatus.Draft)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            ViewBag.ContractsList = new SelectList(
                contracts.Select(c => new
                {
                    Id = c.Id,
                    DisplayText = $"Contract #{c.Id} - {c.Client.Name} ({c.Status}) - {c.Client.Region}"
                }),
                "Id",
                "DisplayText"
            );
        }
    }

    public class ServiceRequestCreateViewModel
    {
        [Required(ErrorMessage = "Please select a contract")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid contract")]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cost is required")]
        [Range(0.01, 1000000, ErrorMessage = "Cost must be between $0.01 and $1,000,000")]
        [Display(Name = "Cost (USD)")]
        public decimal CostUsd { get; set; }
    }
}
