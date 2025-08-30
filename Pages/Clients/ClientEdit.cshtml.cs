using FreedomITAS.Data;
using FreedomITAS.Models;
using FreedomITAS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FreedomITAS.Pages.Clients
{
    public class ClientEditModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly RouteProtector _protector;
        private readonly ClientUpdateService _clientUpdateService;

        public ClientEditModel(AppDbContext context, RouteProtector protector, ClientUpdateService clientUpdateService)
        {
            _context = context;
            _protector = protector;
            _clientUpdateService = clientUpdateService;
        }

        [BindProperty]
        public ClientModel Client { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            string decryptedId;
            try
            {
                decryptedId = _protector.Unprotect(id);
            }
            catch
            {
                return BadRequest("Invalid or tampered link.");
            }

            Client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == decryptedId);

            if (Client == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var clientToUpdate = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == Client.ClientId);
            if (clientToUpdate == null)
                return NotFound();

            // Update fields
            clientToUpdate.CompanyName = Client.CompanyName;
            clientToUpdate.CompanyLegalName = Client.CompanyLegalName;
            clientToUpdate.CompanyType = Client.CompanyType;
            clientToUpdate.Website = Client.Website;
            clientToUpdate.NumberStreet = Client.NumberStreet;
            clientToUpdate.City = Client.City;
            clientToUpdate.StateName = Client.StateName;
            clientToUpdate.Country = Client.Country;
            clientToUpdate.CountryCode = Client.CountryCode;
            clientToUpdate.Postcode = Client.Postcode;
            clientToUpdate.CompanyPhone = Client.CompanyPhone;
            clientToUpdate.CompanyABN = Client.CompanyABN;
            clientToUpdate.ContactFirstName = Client.ContactFirstName;
            clientToUpdate.ContactMiddleName = Client.ContactMiddleName;
            clientToUpdate.ContactLastName = Client.ContactLastName;
            clientToUpdate.ContactEmail = Client.ContactEmail;
            clientToUpdate.ContactMobile = Client.ContactMobile;
            _context.Entry(clientToUpdate).CurrentValues.SetValues(Client);
            await _context.SaveChangesAsync();

            // Decide which systems to update based on which IDs exist
            var systemsToUpdate = new List<string>();
            if (!string.IsNullOrWhiteSpace(clientToUpdate.HaloId)) systemsToUpdate.Add("HaloPSA");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.HuduId)) systemsToUpdate.Add("Hudu");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.SyncroId)) systemsToUpdate.Add("Syncro");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.DreamScapeId)) systemsToUpdate.Add("Dreamscape");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.Pax8Id)) systemsToUpdate.Add("Pax8");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.ZomentumId)) systemsToUpdate.Add("Zomentum");
            if (!string.IsNullOrWhiteSpace(clientToUpdate.HighLevelId)) systemsToUpdate.Add("HighLevel");

            // Push updates
            var results = await _clientUpdateService.UpdateClientAsync(clientToUpdate, systemsToUpdate);

            // Surface results to UI
            foreach (var kv in results) TempData[$"{kv.Key}Update"] = kv.Value;

            return RedirectToPage("/Index");
        }

    }
}
