using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;
using TechMove.Patterns.Observer;

namespace TechMove.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly ContractObservable _contractObservable;
        private readonly IEnumerable<IContractObserver> _observers;
        private readonly ILogger<ContractsController> _logger;
        private readonly IWebHostEnvironment _environment;
        
    
        public ContractsController(
            ApplicationDbContext context,
            IFileStorageService fileStorage,
            ContractObservable contractObservable,
            IEnumerable<IContractObserver> observers,
            ILogger<ContractsController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _fileStorage = fileStorage;
            _contractObservable = contractObservable;
            _observers = observers;
            _logger = logger;
            _environment = environment;

            // Attach all observers
            foreach (var observer in _observers)
            {
                _contractObservable.Attach(observer);
            }
        }

        // GET: Contracts
        public async Task<IActionResult> Index()
        {
            var contracts = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            return View(contracts);
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create
        public async Task<IActionResult> Create()
        {
            await LoadClientsAsync();
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContractCreateViewModel model, IFormFile? signedAgreement)
        {
            _logger.LogInformation("=== Create POST Started ===");
            _logger.LogInformation("ClientId: {ClientId}", model.ClientId);
            _logger.LogInformation("StartDate: {StartDate}", model.StartDate);
            _logger.LogInformation("EndDate: {EndDate}", model.EndDate);
            _logger.LogInformation("Status: {Status}", model.Status);
            _logger.LogInformation("File: {FileName}", signedAgreement?.FileName ?? "No file");

            // Manual validation for dates
            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
            }

            // Validate file if uploaded
            if (signedAgreement != null && !_fileStorage.IsValidPdfFile(signedAgreement))
            {
                ModelState.AddModelError("SignedAgreement", "Only PDF files up to 10MB are allowed.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("=== ModelState is INVALID ===");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state != null && state.Errors.Count > 0)
                    {
                        foreach (var error in state.Errors)
                        {
                            _logger.LogWarning("Key: {Key}, Error: {Error}", key, error.ErrorMessage);
                        }
                    }
                }
                await LoadClientsAsync();
                return View(model);
            }

            try
            {
                var contract = new Contract
                {
                    ClientId = model.ClientId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Status = model.Status
                };

                // Save file if uploaded
                if (signedAgreement != null)
                {
                    contract.SignedAgreementPath = await _fileStorage.SaveFileAsync(signedAgreement, "contracts");
                    _logger.LogInformation("File saved: {FilePath}", contract.SignedAgreementPath);
                }

                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();
                _logger.LogInformation("=== Contract created successfully with ID: {Id} ===", contract.Id);

                TempData["SuccessMessage"] = "Contract created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract");
                TempData["ErrorMessage"] = $"Error creating contract: {ex.Message}";
                await LoadClientsAsync();
                return View(model);
            }
        }

        // GET: Contracts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit called with null id");
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client) // Include client for validation
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                _logger.LogWarning("Contract with id {Id} not found", id);
                return NotFound();
            }

            var model = new ContractEditViewModel
            {
                Id = contract.Id,
                ClientId = contract.ClientId,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status,
                SignedAgreementPath = contract.SignedAgreementPath
            };

            _logger.LogInformation("Loading edit view for contract #{Id}", id);
            await LoadClientsAsync();
            
            return View(model);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContractEditViewModel model, IFormFile? signedAgreement)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // Manual validation for dates
            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
            }

            // Validate file if uploaded
            if (signedAgreement != null && !_fileStorage.IsValidPdfFile(signedAgreement))
            {
                ModelState.AddModelError("SignedAgreement", "Only PDF files up to 10MB are allowed.");
            }

            if (!ModelState.IsValid)
            {
                await LoadClientsAsync();
                return View(model);
            }

            try
            {
                // Get original status before updating
                var originalContract = await _context.Contracts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == model.Id);
                
                if (originalContract == null)
                {
                    return NotFound();
                }

                var oldStatus = originalContract.Status.ToString();

                // Get the contract to update
                var contract = await _context.Contracts.FindAsync(id);
                if (contract == null)
                {
                    return NotFound();
                }

                // Update properties
                contract.ClientId = model.ClientId;
                contract.StartDate = model.StartDate;
                contract.EndDate = model.EndDate;
                contract.Status = model.Status;

                // Handle file upload
                if (signedAgreement != null)
                {
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                    {
                        await _fileStorage.DeleteFileAsync(contract.SignedAgreementPath);
                    }
                    
                    // Save new file
                    contract.SignedAgreementPath = await _fileStorage.SaveFileAsync(signedAgreement, "contracts");
                    _logger.LogInformation("New file uploaded for contract #{Id}: {FilePath}", contract.Id, contract.SignedAgreementPath);
                }

                _context.Update(contract);
                await _context.SaveChangesAsync();

                // OBSERVER PATTERN: Notify if status changed
                var newStatus = contract.Status.ToString();
                if (oldStatus != newStatus)
                {
                    _logger.LogInformation("Contract #{Id} status changed from {OldStatus} to {NewStatus}",
                        contract.Id, oldStatus, newStatus);

                    _contractObservable.NotifyStatusChange(contract.Id, oldStatus, newStatus);
                }

                TempData["SuccessMessage"] = $"Contract #{contract.Id} updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ContractExistsAsync(model.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract #{Id}", id);
                TempData["ErrorMessage"] = $"Error updating contract: {ex.Message}";
                await LoadClientsAsync();
                return View(model);
            }
        }

        // GET: Contracts/Download/5
        [Authorize]
        public async Task<IActionResult> Download(int? id)
        {   
            if (id == null)
            {
                _logger.LogWarning("Download called with null id");
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                _logger.LogWarning("Contract with id {Id} not found", id);
                return NotFound();
            }

            if (string.IsNullOrEmpty(contract.SignedAgreementPath))
            {
                _logger.LogWarning("Contract #{Id} has no signed agreement", id);
                TempData["ErrorMessage"] = "No file uploaded for this contract.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                _logger.LogInformation("Attempting to download file for contract #{Id}: {Path}", id, contract.SignedAgreementPath);
                
                // Build the full path: uploads/contracts/filename.pdf
                var filePath = Path.Combine("uploads", contract.SignedAgreementPath);
                
                var fileBytes = await _fileStorage.GetFileAsync(filePath);
                
                _logger.LogInformation("Successfully retrieved file for contract #{Id}, size: {Size} bytes", id, fileBytes.Length);
                
                return File(fileBytes, "application/pdf", $"Contract_{contract.Id}_Agreement.pdf");
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "File not found for contract #{Id}: {Path}", id, contract.SignedAgreementPath);
                TempData["ErrorMessage"] = "File not found on server. The file may have been deleted.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file for contract #{Id}", id);
                TempData["ErrorMessage"] = $"Error downloading file: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // TEMPORARY: Debug file paths
        [Authorize]
        public async Task<IActionResult> DebugFilePath(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            
            if (contract == null)
            {
                return Content("Contract not found");
            }

            var info = new System.Text.StringBuilder();
            info.AppendLine($"Contract ID: {contract.Id}");
            info.AppendLine($"SignedAgreementPath in DB: {contract.SignedAgreementPath ?? "NULL"}");
            info.AppendLine($"Current Directory: {Directory.GetCurrentDirectory()}");
            
            if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
            {
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(), 
                    "wwwroot", 
                    "uploads", 
                    contract.SignedAgreementPath
                );
                
                info.AppendLine($"Full Path: {fullPath}");
                info.AppendLine($"File Exists: {System.IO.File.Exists(fullPath)}");
                
                // Check uploads directory
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
                info.AppendLine($"\nUploads Directory: {uploadsDir}");
                info.AppendLine($"Directory Exists: {Directory.Exists(uploadsDir)}");
                
                if (Directory.Exists(uploadsDir))
                {
                    var files = Directory.GetFiles(uploadsDir);
                    info.AppendLine($"\nFiles in contracts folder ({files.Length}):");
                    foreach (var file in files)
                    {
                        info.AppendLine($"  - {Path.GetFileName(file)}");
                    }
                }
            }
            
            return Content(info.ToString(), "text/plain");
        }

        // GET: Contracts/Delete/5
        [Authorize(Roles = "Admin,Manager")] // Admins and Managers can delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")] // Admins and Managers can delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.ServiceRequests)
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            // Validation: Cannot delete contract with service requests
            if (contract.ServiceRequests.Any())
            {
                _logger.LogWarning("Attempted to delete contract #{Id} with {Count} existing service requests", id, contract.ServiceRequests.Count);
                TempData["ErrorMessage"] = $"Cannot delete Contract #{id} because it has {contract.ServiceRequests.Count} existing service request(s). Delete the service requests first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Delete associated file if exists
                if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                {
                    await _fileStorage.DeleteFileAsync(contract.SignedAgreementPath);
                    _logger.LogInformation("Deleted file: {FilePath}", contract.SignedAgreementPath);
                }

                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Contract #{Id} for client '{ClientName}' deleted successfully", id, contract.Client.Name);
                TempData["SuccessMessage"] = $"Contract #{id} deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract #{Id}", id);
                TempData["ErrorMessage"] = "Error deleting contract. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ContractExistsAsync(int id)
        {
            return await _context.Contracts.AnyAsync(e => e.Id == id);
        }

        private async Task LoadClientsAsync()
        {
            var clients = await _context.Clients.OrderBy(c => c.Name).ToListAsync();
            ViewBag.ClientsList = new SelectList(clients, nameof(Client.Id), nameof(Client.Name));
        }
    }

    public class ContractCreateViewModel
    {
        [Required(ErrorMessage = "Please select a client")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid client")]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(6);

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public ContractStatus Status { get; set; }
    }

    public class ContractEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a client")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid client")]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public ContractStatus Status { get; set; }

        public string? SignedAgreementPath { get; set; }
    }
}
