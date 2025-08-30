using FreedomITAS.API_Serv;
using FreedomITAS.Data;
using FreedomITAS.Models;
using FreedomITAS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace FreedomITAS.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ClientCreateService _clientCreateService;
        private readonly AppDbContext _context;
        private readonly RouteProtector _protector;
        private readonly ClientDeleteService _deleteService;
        private readonly ClientUpdateService _clientUpdateService;        


        [BindProperty]
        public List<string> SelectedSource { get; set; }
        [BindProperty]
        public string ClientId { get; set; }
        [BindProperty]
        public ClientModel EditedClient { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; }

        public IList<ClientModel> Clients { get; set; }
        public Dictionary<string, string> EncryptedIds { get; set; }

        public IndexModel(AppDbContext context, RouteProtector protector, ClientCreateService clientCreateService, ClientUpdateService clientUpdateService)
        {
            _context = context;
            _protector = protector;
            _clientCreateService = clientCreateService;
            _clientUpdateService = clientUpdateService;            
        }


        public async Task OnGetAsync()
        {
            Clients = await _context.Clients.ToListAsync();
            EncryptedIds = Clients.ToDictionary(c => c.ClientId, c => _protector.Protect(c.ClientId));
        }
       
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var clientToUpdate = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == EditedClient.ClientId);
            if (clientToUpdate == null) return NotFound();

            _context.Entry(clientToUpdate).CurrentValues.SetValues(EditedClient);
            await _context.SaveChangesAsync();

            // choose platforms: by presence of IDs (edit pushes only to systems where the client already exists)
            var systemsToUpdate = new List<string>();
            if (!string.IsNullOrWhiteSpace(clientToUpdate.HaloId)) systemsToUpdate.Add("HaloPSA");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.HuduId)) systemsToUpdate.Add("Hudu");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.SyncroId)) systemsToUpdate.Add("Syncro");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.DreamScapeId)) systemsToUpdate.Add("Dreamscape");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.Pax8Id)) systemsToUpdate.Add("Pax8");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.ZomentumId)) systemsToUpdate.Add("Zomentum");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.HighLevelId)) systemsToUpdate.Add("HighLevel");

            // (optional) if you want to respect checkboxes from UI instead, use SelectedSource when provided:
            if (SelectedSource != null && SelectedSource.Any())
                systemsToUpdate = SelectedSource;

            var updateResults = await _clientUpdateService.UpdateClientAsync(clientToUpdate, systemsToUpdate);
            foreach (var kv in updateResults)
                TempData[$"Update_{kv.Key}"] = kv.Value;

            TempData["Message"] = "Client updated successfully!";
            
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            string clientId;

            try
            {
                clientId = _protector.Unprotect(id);
            }
            catch
            {
                clientId = id;
            }

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
            if (client == null)
                return NotFound();

            var results = await _deleteService.DeleteClientAsync(client);

            foreach (var kv in results)
                TempData[$"Delete_{kv.Key}"] = kv.Value;

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Client {client.CompanyName} deleted successfully.";
            return RedirectToPage();
        }

        //API Calls      
        public async Task<IActionResult> OnPostPushClientAsync()
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == ClientId);
            if (client == null)
            {
                ModelState.AddModelError("", "Client not found.");
                return Page();
            }

            if (SelectedSource == null || !SelectedSource.Any())
            {
                ModelState.AddModelError("", "Please select at least one system.");
                return Page();
            }

            try
            {
                var results = await _clientCreateService.CreateClientAsync(client, SelectedSource);

                foreach (var result in results)
                {
                    TempData[$"{result.Key}Message"] = result.Value;
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error pushing client: {ex.Message}";
                return RedirectToPage();
            }
        }

    }
}