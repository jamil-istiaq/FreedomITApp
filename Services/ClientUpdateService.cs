using FreedomITAS.API_Serv;
using FreedomITAS.Data;
using FreedomITAS.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FreedomITAS.Services
{
    public class ClientUpdateService
    {
        private readonly HuduService _huduService;
        private readonly HaloPSAService _haloPSAService;
        private readonly SyncroService _syncroService;
        private readonly DreamscapeService _dreamscapeService;
        private readonly Pax8Service _pax8Service;
        private readonly ZomentumService _zomentumService;
        private readonly GoHighLevelService _goHighLevelService;
        private readonly AppDbContext _dbContext;

        public ClientUpdateService(HuduService huduService, HaloPSAService haloPSAService, AppDbContext dbContext, SyncroService syncroService, DreamscapeService dreamscapeService, Pax8Service pax8Service, ZomentumService zomentumService, GoHighLevelService goHighLevelService)
        {
            _huduService = huduService;
            _haloPSAService = haloPSAService;
            _dbContext = dbContext;
            _syncroService = syncroService;
            _dreamscapeService = dreamscapeService;
            _pax8Service = pax8Service;
            _zomentumService = zomentumService;
            _goHighLevelService = goHighLevelService;
        }

        public async Task<Dictionary<string, string>> UpdateClientAsync(ClientModel client, IEnumerable<string> systems)
        {

            var results = new Dictionary<string, string>();

            foreach (var system in systems)
            {
                try
                {
                    switch (system)
                    {
                        case "Hudu":
                            if (string.IsNullOrWhiteSpace(client.HuduId)) { results["Hudu"] = "No HuduId."; break; }
                            await UpdateHuduAsync(client);
                            results["Hudu"] = "Updated.";
                            break;

                        case "HaloPSA":
                            if (string.IsNullOrWhiteSpace(client.HaloId)) { results["HaloPSA"] = "No HaloId."; break; }
                            await UpdateHaloAsync(client);
                            results["HaloPSA"] = "Updated.";
                            break;

                        case "Syncro":
                            if (string.IsNullOrWhiteSpace(client.SyncroId)) { results["Syncro"] = "No SyncroId."; break; }
                            await UpdateSyncroAsync(client);
                            results["Syncro"] = "Updated.";
                            break;

                        case "Dreamscape":
                            if (string.IsNullOrWhiteSpace(client.DreamScapeId)) { results["Dreamscape"] = "No DreamScapeId."; break; }
                            await UpdateDreamscapeAsync(client);
                            results["Dreamscape"] = "Updated.";
                            break;

                        case "Pax8":
                            if (string.IsNullOrWhiteSpace(client.Pax8Id)) { results["Pax8"] = "No Pax8Id."; break; }
                            await UpdatePax8Async(client);
                            results["Pax8"] = "Updated.";
                            break;

                        case "Zomentum":
                            if (string.IsNullOrWhiteSpace(client.ZomentumId)) { results["Zomentum"] = "No ZomentumId."; break; }
                            await UpdateZomentumAsync(client);
                            results["Zomentum"] = "Updated.";
                            break;

                        case "HighLevel":
                            if (string.IsNullOrWhiteSpace(client.HighLevelId)) { results["HighLevel"] = "No HighLevelId."; break; }
                            await UpdateHighLevelAsync(client);
                            results["HighLevel"] = "Updated.";
                            break;

                        default:
                            results[system] = "Unsupported.";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    results[system] = $"Error: {ex.Message}";
                }
            }

            return results;
        }

        private async Task UpdateHuduAsync(ClientModel c)
        {
            var payload = new
            {
                name = c.CompanyName,
                compnay_type = c.CompanyType,
                address_line_1 = c.NumberStreet,
                city = c.City,
                state = c.StateName,
                zip = c.Postcode,
                country_name = c.Country,
                phone_number = c.CompanyPhone,
                website = c.Website
            };

            var resp = await _huduService.UpdateCompanyAsync(c.HuduId!, payload);
            await EnsureSuccess(resp, "Hudu");
        }

        private async Task UpdateHaloAsync(ClientModel c)
        {
            if (string.IsNullOrWhiteSpace(c.HaloId))
                throw new Exception("HaloId is missing.");

            var payload = new[] {
            new {                
                name = c.CompanyName,
                override_org_address = new {
                    line1 = c.NumberStreet,
                    line2 = c.City,
                    line3 = c.StateName,
                    line4 = c.Country,
                    postcode = c.Postcode
                },
                override_org_phone = c.CompanyPhone,
                override_org_website = c.Website,
                newclient_contactname = $"{c.ContactFirstName} {c.ContactMiddleName} {c.ContactLastName}".Trim(),
                newclient_contactemail = c.ContactEmail
            }
        };
            var resp = await _haloPSAService.UpdateClientAsync(c.HaloId,payload);
            await EnsureSuccess(resp, "HaloPSA");
        }

        private async Task UpdateSyncroAsync(ClientModel c)
        {
            var payload = new
            {
                business_name = c.CompanyLegalName,
                firstname = c.ContactFirstName,
                lastname = $"{c.ContactMiddleName} {c.ContactLastName}".Trim(),
                email = c.ContactEmail,
                phone = c.CompanyPhone,
                mobile = c.ContactMobile,
                address = c.NumberStreet,
                city = c.City,
                state = c.StateName,
                zip = c.Postcode
            };

            var resp = await _syncroService.UpdateCustomerAsync(c.SyncroId!, payload); 
            await EnsureSuccess(resp, "Syncro");
        }

        private async Task UpdateDreamscapeAsync(ClientModel c)
        {
            
            var payload = new
            {
                first_name = c.ContactFirstName,
                last_name = $"{c.ContactMiddleName} {c.ContactLastName}".Trim(),
                address = c.NumberStreet,
                city = c.City,
                country = string.IsNullOrWhiteSpace(c.CountryCode) ? c.Country : c.CountryCode, // AU preferred
                country_code = c.CountryCode,
                state = c.StateName,
                post_code = c.Postcode,
                phone = c.CompanyPhone,
                mobile = c.ContactMobile,
                email = c.ContactEmail,
                business_name = c.CompanyLegalName,
                business_number_type = "ABN",
                business_number = c.CompanyABN
            };

            var resp = await _dreamscapeService.UpdateCompanyAsync(c.DreamScapeId!, payload); // implement UpdateCompanyAsync in DreamscapeService -> PUT /customers/{id}
            await EnsureSuccess(resp, "Dreamscape");
        }

        private async Task UpdatePax8Async(ClientModel c)
        {
            // Pax8 update endpoint/fields are more limited; adjust to your API.
            var payload = new
            {
                name = c.CompanyName,
                address = new
                {
                    street = c.NumberStreet,
                    city = c.City,
                    stateOrProvince = c.StateName,
                    postalCode = c.Postcode,
                    country = c.Country // or code, depending on API
                },
                phone = c.CompanyPhone,
                website = c.Website
            };

            var resp = await _pax8Service.UpdateClientAsync(c.Pax8Id!, payload); // implement in Pax8Service if supported
            await EnsureSuccess(resp, "Pax8");
        }

        private async Task UpdateZomentumAsync(ClientModel c)
        {
            var payload = new
            {
                name = c.CompanyName,
                phone = c.CompanyPhone,
                billing_address = new
                {
                    address_line_1 = c.NumberStreet,
                    address_line_2 = "",
                    city = c.City,
                    state = c.StateName,
                    pincode = c.Postcode,
                    country = c.Country
                },
                compnay_types = c.CompanyType,
                website = c.Website
            };

            var resp = await _zomentumService.UpdateClientAsync(c.ZomentumId!, payload); // implement in ZomentumService -> PUT /client/companies/{id}
            await EnsureSuccess(resp, "Zomentum");
        }

        private async Task UpdateHighLevelAsync(ClientModel c)
        {
            var payload = new
            {
                firstName = c.ContactFirstName,
                lastName = $"{c.ContactMiddleName} {c.ContactLastName}".Trim(),
                companyName = c.CompanyName,
                email = c.ContactEmail,
                phone = c.CompanyPhone,
                address1 = c.NumberStreet,
                city = c.City,
                state = c.StateName,
                postalCode = c.Postcode,
                website = c.Website
            };

            var resp = await _goHighLevelService.UpdateContactAsync(c.HighLevelId!, payload); // implement in GoHighLevelService -> PATCH /contacts/{id} with Version header
            await EnsureSuccess(resp, "HighLevel");
        }

        private static async Task EnsureSuccess(HttpResponseMessage resp, string system)
        {
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                throw new Exception($"{system} update failed: {resp.StatusCode} - {body}");
            }
        }
    }
}
