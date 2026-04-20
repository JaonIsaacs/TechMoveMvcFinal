using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TechMove.Data;
using TechMove.Models;

namespace TechMove.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(ApplicationDbContext context, ILogger<ClientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients
                .Include(c => c.Contracts) // Include to show contract count
                .ToListAsync();
            return View(clients);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = new Client
            {
                Name = model.Name,
                Region = model.Region
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Client created: {ClientName} ({Region})", client.Name, client.Region);
            TempData["SuccessMessage"] = "Client created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Delete/5
        [Authorize(Roles = "Admin")] // Only Admins can delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admins can delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Contracts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            // Validation: Cannot delete client with contracts
            if (client.Contracts.Any())
            {
                _logger.LogWarning("Attempted to delete client #{Id} with {Count} existing contracts", id, client.Contracts.Count);
                TempData["ErrorMessage"] = $"Cannot delete client '{client.Name}' because it has {client.Contracts.Count} existing contract(s). Delete the contracts first.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Client #{Id} ({Name}) deleted successfully", id, client.Name);
                TempData["SuccessMessage"] = $"Client '{client.Name}' deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client #{Id}", id);
                TempData["ErrorMessage"] = "Error deleting client. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class ClientViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200)]
        [Display(Name = "Client Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region is required")]
        [StringLength(100)]
        [Display(Name = "Region")]
        public string Region { get; set; } = string.Empty;
    }
}
