using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;
using TechMove.Patterns.Observer;

namespace TechMove.Pages.Contracts
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly ContractObservable _contractObservable;
        private readonly IEnumerable<IContractObserver> _observers;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            ApplicationDbContext context, 
            IFileStorageService fileStorage, 
            ContractObservable contractObservable,
            IEnumerable<IContractObserver> observers,
            ILogger<EditModel> logger)
        {
            _context = context;
            _fileStorage = fileStorage;
            _contractObservable = contractObservable;
            _observers = observers;
            _logger = logger;

            // Attach all observers
            foreach (var observer in _observers)
            {
                _contractObservable.Attach(observer);
            }
        }

        [BindProperty]
        public Contract Contract { get; set; } = null!;

        [BindProperty]
        public IFormFile? SignedAgreement { get; set; }

        public SelectList? ClientsList { get; set; }

        private string? _originalStatus;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            Contract = contract;
            _originalStatus = contract.Status.ToString();
            await LoadClientsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Contract.EndDate <= Contract.StartDate)
            {
                ModelState.AddModelError("Contract.EndDate", "End date must be after start date.");
            }

            if (SignedAgreement != null && !_fileStorage.IsValidPdfFile(SignedAgreement))
            {
                ModelState.AddModelError("SignedAgreement", "Only PDF files up to 10MB are allowed.");
            }

            if (!ModelState.IsValid)
            {
                await LoadClientsAsync();
                return Page();
            }

            try
            {
                // Get original status before updating
                var originalContract = await _context.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Contract.Id);
                var oldStatus = originalContract?.Status.ToString() ?? "Unknown";

                // Handle file upload
                if (SignedAgreement != null)
                {
                    if (!string.IsNullOrEmpty(Contract.SignedAgreementPath))
                    {
                        await _fileStorage.DeleteFileAsync(Contract.SignedAgreementPath);
                    }
                    Contract.SignedAgreementPath = await _fileStorage.SaveFileAsync(SignedAgreement, "contracts");
                }

                _context.Attach(Contract).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // OBSERVER PATTERN: Notify if status changed
                var newStatus = Contract.Status.ToString();
                if (oldStatus != newStatus)
                {
                    _logger.LogInformation("Contract #{Id} status changed from {OldStatus} to {NewStatus}", 
                        Contract.Id, oldStatus, newStatus);
                    
                    _contractObservable.NotifyStatusChange(Contract.Id, oldStatus, newStatus);
                }

                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ContractExistsAsync(Contract.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task<bool> ContractExistsAsync(int id)
        {
            return await _context.Contracts.AnyAsync(e => e.Id == id);
        }

        private async Task LoadClientsAsync()
        {
            var clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
            ClientsList = new SelectList(clients, nameof(Client.Id), nameof(Client.Name));
        }
    }
}