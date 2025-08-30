using Azure;
using FreedomITAS.API_Serv;
using FreedomITAS.Data;
using FreedomITAS.Models;
using System.Text.Json;

namespace FreedomITAS.Services
{
    public class ClientCreateService
    {
        private readonly ZomentumService _zomentumService;
        private readonly HuduService _huduService;
        private readonly HaloPSAService _haloPSAService;
        private readonly SyncroService _syncroService;
        private readonly DreamscapeService _dreamscapeService;
        private readonly Pax8Service _pax8Service;
        private readonly GoHighLevelService _highLevelService;
        private readonly AppDbContext _dbContext;

        public ClientCreateService(
            Pax8Service pax8Service,
            ZomentumService zomentumService,
            HuduService huduService,
            HaloPSAService haloPSAService,
            SyncroService syncroService,
            DreamscapeService dreamscapeService,
            GoHighLevelService highLevelService,
            AppDbContext dbContext)
        {
            _zomentumService = zomentumService;
            _huduService = huduService;
            _haloPSAService = haloPSAService;
            _syncroService = syncroService;
            _dreamscapeService = dreamscapeService;
            _pax8Service = pax8Service;
            _highLevelService = highLevelService;
            _dbContext = dbContext;
        }
       
        private async Task<string> ExtractIdFromResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(json);
                //DreamscapeID
                if (doc.RootElement.TryGetProperty("data", out var dataElement) &&
                    dataElement.TryGetProperty("id", out var dreamscapeId))
                {
                    return dreamscapeId.GetInt32().ToString();
                }

                //SyncroID
                if (doc.RootElement.TryGetProperty("customer", out var customerElement) &&
                    customerElement.TryGetProperty("id", out var customerId))
                {
                    return customerId.GetInt32().ToString();
                }

                // HuduID
                if (doc.RootElement.TryGetProperty("company", out var companyElement) &&
                    companyElement.TryGetProperty("id", out var idElement))
                {
                    return idElement.GetInt32().ToString();
                }

                // For other APIs
                if (doc.RootElement.TryGetProperty("id", out var rootId))
                    return rootId.GetInt32().ToString();

                //GoHighLevelID
                if (doc.RootElement.TryGetProperty("contact", out var contactElement) &&
                    contactElement.TryGetProperty("id", out var contactId))
                {
                    return contactId.GetString();
                }

                throw new Exception($"ID not found in API response. Raw JSON: {json}");
            }
            catch (JsonException jex)
            {
                throw new Exception($"Invalid JSON returned in response: {jex.Message}\n\nRaw response: {json}");
            }
        }


        public async Task<Dictionary<string, string>> CreateClientAsync(ClientModel client, List<string> systems)
        {
            var results = new Dictionary<string, string>();

            foreach (var system in systems)
            {
                switch (system)
                {
                    case "Zomentum":
                        var zomentumPayload = new
                        {
                            name = client.CompanyName,
                            phone = client.CompanyPhone,
                            billing_address = new
                            {
                                address_line_1 = client.NumberStreet,
                                address_line_2 = "",
                                city = client.City,
                                state = client.StateName,
                                pincode = client.Postcode,
                                country = client.Country,
                                phone = client.CompanyPhone
                            },
                            compnay_types = client.CompanyType,
                            website = client.Website,

                        };                                          
                        var Zomentumresponse = await _zomentumService.CreateClientAsync(zomentumPayload);
                        var ZomentumId = await ExtractIdFromResponse(Zomentumresponse);
                        client.ZomentumId = ZomentumId;
                        await _dbContext.SaveChangesAsync();                        
                        break;

                    case "HaloPSA":
                        
                            var haloPayload = new[] {
                    new
                    {
                        name = client.CompanyName,
                        toplevel_name = "FIT_App",
                        from_address_override=$"{client.NumberStreet}{client.City}{client.StateName}{client.Country}{client.Postcode}".Trim(),
                        override_org_address = new
                        {
                            line1 = client.NumberStreet,
                            line2 = client.City,
                            line3 = client.StateName,
                            line4 = client.Country,
                            postcode = client.Postcode
                        },
                        override_org_phone= client.CompanyPhone,
                        override_org_website= client.Website,
                        override_org_email=client.ContactEmail,
                        newclient_contactname= $"{client.ContactFirstName} {client.ContactMiddleName} {client.ContactLastName}".Trim(),
                        newclient_contactemail =client.ContactEmail
                    }
                    };
                            var Haloresponse = await _haloPSAService.CreateClientAsync(haloPayload);                                                       
                            var content = await Haloresponse.Content.ReadAsStringAsync();                            
                            var HaloId = await ExtractIdFromResponse(Haloresponse);
                            client.HaloId = HaloId;
                            await _dbContext.SaveChangesAsync();                            
                            break;
                        

                    case "Hudu":
                        var huduPayload = new
                        {
                            name = client.CompanyName,
                            compnay_type = client.CompanyType,
                            address_line_1 = client.NumberStreet,
                            city = client.City,
                            state = client.StateName,
                            zip = client.Postcode,
                            country_name = client.Country,
                            phone_number = client.CompanyPhone,
                            website = client.Website
                        };
                        var huduResponse = await _huduService.CreateCompanyAsync(huduPayload);
                        var HuduId = await ExtractIdFromResponse(huduResponse);
                        var json = await huduResponse.Content.ReadAsStringAsync();
                        client.HuduId = HuduId;
                        await _dbContext.SaveChangesAsync();                        
                        break;

                    case "Syncro":
                        var syncroPayload = new
                        {
                            business_name = client.CompanyLegalName,
                            firstname = client.ContactFirstName,
                            lastname = $"{client.ContactMiddleName} {client.ContactLastName}".Trim(),
                            email = client.ContactEmail,
                            phone = client.CompanyPhone,
                            mobile = client.ContactMobile,
                            address = client.NumberStreet,
                            city = client.City,
                            state = client.StateName,
                            zip = client.Postcode,

                        };
                        var syncroResponse = await _syncroService.CreateCompanyAsync(syncroPayload);
                        var SyncroId = await ExtractIdFromResponse(syncroResponse);
                        client.SyncroId = SyncroId;
                        await _dbContext.SaveChangesAsync();
                        //results["Syncro"] = SyncroId;
                        break;

                    case "Dreamscape":
                        string companyName = client.CompanyName ?? string.Empty;
                        string cleaned = new string(companyName.Where(char.IsLetterOrDigit).ToArray()).ToLower();
                        string shortCode = "xx";
                        if (cleaned.Length >= 2)
                        {
                            char firstLetter = cleaned.FirstOrDefault(char.IsLetter);
                            char firstDigit = cleaned.FirstOrDefault(char.IsDigit);
                            if (firstLetter == default) firstLetter = 'x';
                            if (firstDigit == default) firstDigit = '0';
                            shortCode = $"{firstLetter}{firstDigit}";
                        }
                        string username = $"admin_{shortCode}";

                        string rawType = (client.CompanyType ?? "").ToLower();
                        string accountType;
                        switch (rawType)
                        {
                            case "personal":
                            case "business":
                                accountType = rawType;
                                break;
                            default:                                
                                accountType = "business";
                                break;
                        }
                        var dreamPayload = new
                        {
                            username = username,
                            first_name = client.ContactFirstName,
                            last_name = $"{client.ContactMiddleName} {client.ContactLastName}".Trim(),
                            address = client.NumberStreet,
                            city = client.City,
                            country = client.Country,
                            country_code = client.CountryCode,
                            state = client.StateName,
                            post_code = client.Postcode,
                            phone = client.CompanyPhone,
                            mobile = client.ContactMobile,
                            email = client.ContactEmail,
                            account_type = accountType,
                            business_name = client.CompanyLegalName,
                            business_number_type = "ABN",
                            business_number = client.CompanyABN,

                        };
                        var dreamResponse = await _dreamscapeService.CreateCompanyAsync(dreamPayload);
                        if (!dreamResponse.IsSuccessStatusCode)
                        {
                            var error = await dreamResponse.Content.ReadAsStringAsync();
                            Console.WriteLine($"Dreamscape Error: {dreamResponse.StatusCode} - {error}");
                            throw new Exception($"Failed to create Dreamscape customer: {error}");
                        }
                        var DreamScapeId = await ExtractIdFromResponse(dreamResponse);
                        client.DreamScapeId = DreamScapeId;
                        await _dbContext.SaveChangesAsync();                        
                        break;

                    case "Pax8":
                        string postcode = string.IsNullOrWhiteSpace(client.Postcode) ? "0000" : client.Postcode.Trim();
                        var pax8Payload =
                        new
                        {
                            name = client.CompanyName,
                            company_type = client.CompanyType,
                            address = new
                            {
                                street = client.NumberStreet,
                                city = client.City,
                                stateOrProvince = client.StateName,
                                postalCode = postcode,
                                country = client.Country

                            },
                            phone = client.CompanyPhone,
                            website = client.Website,
                            billOnBehalfOfEnabled = true,
                            selfServiceAllowed = true,
                            orderApprovalRequired = true
                        };
                        var pax8Response = await _pax8Service.CreateClientAsync(pax8Payload);
                        var pax8CompanyId = await ExtractIdFromResponse(pax8Response);
                        client.Pax8Id = pax8CompanyId;
                        await _dbContext.SaveChangesAsync();

                        var adminContactPayload = new
                        {
                            firstName = client.ContactFirstName,
                            lastName = $"{client.ContactMiddleName} {client.ContactLastName}".Trim(),
                            email = client.ContactEmail,
                            phone = client.CompanyPhone,
                            types = new[] { new { type = "Admin", primary = true } }
                        };

                        results["Pax8"] = await pax8Response.Content.ReadAsStringAsync();
                        var resultJson = await pax8Response.Content.ReadAsStringAsync();
                        Console.WriteLine("Pax8 API Create Response:\n" + resultJson);

                        if (!pax8Response.IsSuccessStatusCode)
                        {
                            var error = await pax8Response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Pax8 Error: {pax8Response.StatusCode} - {error}");
                            throw new Exception($"Failed to create Pax8 customer: {error}");
                        }

                        break;

                    case "HighLevel":
                        var GHLPayload =
                        new
                        {
                            firstName = client.ContactFirstName,
                            lastName = $"{client.ContactMiddleName} {client.ContactLastName}".Trim(),
                            companyName = client.CompanyName,
                            email = client.ContactEmail,
                            locationId = "aLgWjwsNm8ALxmFjkeu6",
                            phone = client.CompanyPhone,
                            address1 = client.NumberStreet,
                            city = client.City,
                            state = client.StateName,
                            postalCode = client.Postcode,
                            website = client.Website,
                            customFields = new[] {
                        new {
                            id="aLgWjwsNm8ALxmFjkeu6",
                            }                          
                        }
                        };
                        var GHLResponse = await _highLevelService.CreateContactAsync(GHLPayload);
                        var HighLevelId = await ExtractIdFromResponse(GHLResponse); ;
                        client.HighLevelId = HighLevelId;
                        await _dbContext.SaveChangesAsync();                        
                        break;


                    default:
                        results[system] = "Unsupported system";
                        break;
                }
            }

            return results;
        }
    }
}
