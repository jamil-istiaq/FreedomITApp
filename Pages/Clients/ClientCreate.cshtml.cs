using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using FreedomITAS.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FreedomITAS.Data;
using Microsoft.EntityFrameworkCore;

namespace FreedomITAS.Pages.Clients
{
    public class ClientCreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public ClientCreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ClientModel Client { get; set; }

        public void OnGet()
        {
        }

        private async Task<string> GenerateCompanyIdAsync()
        {
            var today = DateTime.Now;
            string day = today.Day.ToString("00");
            string month = today.Month.ToString("00");
            string year = today.Year.ToString().Substring(2);

            // Build date prefix: YYMMDD
            string datePrefix = $"{year}{month}{day}";

            // Get the last client with the highest increment (e.g., 250501-FIT-C002)
            var lastClient = await _context.Clients
                .OrderByDescending(c => c.ClientId)
                .FirstOrDefaultAsync();

            int nextIncrement = 1;

            if (lastClient != null)
            {
                var lastId = lastClient.ClientId;
                // Extract the numeric part from last ID
                var parts = lastId.Split("-FIT-C");
                if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
                {
                    nextIncrement = lastNumber + 1;
                }
            }

            string paddedCount = nextIncrement.ToString("D4");
            return $"{datePrefix}-FIT-C{paddedCount}";
        }

        public async Task<IActionResult> OnPostAsync()
        {          

            var clientId = await GenerateCompanyIdAsync(); 

            var newClient = new ClientModel
            {
                ClientId = clientId,
                CompanyName = Client.CompanyName,
                CompanyLegalName = Client.CompanyLegalName,
                CompanyABN = Client.CompanyABN,
                CompanyType = Client.CompanyType,
                CompanyPhone = Client.CompanyPhone,
                Website = Client.Website,
                NumberStreet = Client.NumberStreet,
                City = Client.City,                
                StateName = Client.StateName,
                Postcode = Client.Postcode,
                Country = Client.Country,
                CountryCode = Client.CountryCode,

                ContactFirstName = Client.ContactFirstName,
                ContactMiddleName = Client.ContactMiddleName,
                ContactLastName = Client.ContactLastName,
                ContactEmail = Client.ContactEmail,
                ContactMobile = Client.ContactMobile
            };


            // Save the client data to database
            _context.Clients.Add(newClient);
            await _context.SaveChangesAsync();

            return RedirectToPage("/index");
        }
    }

}
