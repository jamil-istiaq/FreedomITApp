using FreedomITAS.API_Serv;
using FreedomITAS.Models;

namespace FreedomITAS.Services
{
    public class ClientDeleteService
    {
        private readonly HuduService _huduService;
        private readonly HaloPSAService _haloPSAService;
        private readonly SyncroService _syncroService;
        private readonly DreamscapeService _dreamscapeService;
        private readonly Pax8Service _pax8Service;
        private readonly ZomentumService _zomentumService;
        private readonly GoHighLevelService _goHighLevelService;

        public ClientDeleteService(HuduService huduService, HaloPSAService haloPSAService, SyncroService syncroService, DreamscapeService dreamscapeService, Pax8Service pax8Service,
            ZomentumService zomentumService, GoHighLevelService goHighLevelService)
        {
            _huduService = huduService;
            _haloPSAService = haloPSAService;
            _syncroService = syncroService;
            _dreamscapeService = dreamscapeService;
            _pax8Service = pax8Service;
            _zomentumService = zomentumService;
            _goHighLevelService = goHighLevelService;
        }

        public async Task<Dictionary<string, string>> DeleteClientAsync(ClientModel client)
        {
            var systems = new List<string>();
            if (!string.IsNullOrWhiteSpace(client.HuduId)) systems.Add("Hudu");
            if (!string.IsNullOrWhiteSpace(client.HaloId)) systems.Add("HaloPSA");
            if (!string.IsNullOrWhiteSpace(client.SyncroId)) systems.Add("Syncro");
            if (!string.IsNullOrWhiteSpace(client.DreamScapeId)) systems.Add("Dreamscape");
            if (!string.IsNullOrWhiteSpace(client.Pax8Id)) systems.Add("Pax8");
            if (!string.IsNullOrWhiteSpace(client.ZomentumId)) systems.Add("Zomentum");
            if (!string.IsNullOrWhiteSpace(client.HighLevelId)) systems.Add("HighLevel");

            return await DeleteClientAsync(client, systems);
        }

        public async Task<Dictionary<string, string>> DeleteClientAsync(ClientModel client, List<string> systems)
        {
            var results = new Dictionary<string, string>();

            foreach (var system in systems)
            {
                switch (system)
                {
                    case "Hudu":
                        if (!string.IsNullOrWhiteSpace(client.HuduId))
                        {
                            await _huduService.DeleteCompanyAsync(client.HuduId);
                            results["Hudu"] = "Deleted successfully.";
                        }
                        else
                        {
                            results["Hudu"] = "No Hudu ID to delete.";
                        }
                        break;
                    case "HaloPSA":
                        if (!string.IsNullOrWhiteSpace(client.HaloId))
                        {
                            await _haloPSAService.DeleteCompanyAsync(client.HaloId);
                            results["HaloPSA"] = "Deleted successfully.";
                        }
                        else
                        {
                            results["HaloPSA"] = "No ID to delete.";
                        }
                        break;
                    case "Syncro":
                        if (!string.IsNullOrWhiteSpace(client.SyncroId))
                        {
                            await _syncroService.DeleteCustomerOrContactAsync(client.SyncroId);
                            results["Syncro"] = "Deleted successfully.";
                        }
                        else
                        {
                            results["Syncro"] = "No ID to delete.";
                        }
                        break;
                    case "Dreamscape":
                        if (!string.IsNullOrWhiteSpace(client.DreamScapeId))
                        {
                            await _dreamscapeService.DeleteCompanyAsync(client.DreamScapeId);
                            results["Dreamscape"] = "Deleted successfully.";
                        }
                        else
                        {
                            results["Dreamscape"] = "No ID to delete.";
                        }
                        break;
                    case "Pax8":
                        if (!string.IsNullOrWhiteSpace(client.Pax8Id))
                        {
                            await _pax8Service.DeleteClientAsync(client.Pax8Id);
                            results["Pax8"] = "Deleted successfully.";
                        }
                        else
                        {
                            results["Pax8"] = "No ID to delete.";
                        }
                        break;
                    case "Zomentum":
                        if (!string.IsNullOrWhiteSpace(client.ZomentumId))
                        {
                            await _zomentumService.DeleteClientAsync(client.ZomentumId);
                            results["Zomentum"] = "Deleted successfully.";
                        }
                        else
                        {
                            results["Zomentum"] = "No ID to delete.";
                        }
                        break;
                    case "HighLevel":
                        if (!string.IsNullOrWhiteSpace(client.HighLevelId))
                        {
                            await _goHighLevelService.DeleteContactAsync(client.HighLevelId);
                            results["HighLevel"] = "Deleted successfully.";
                        }
                        else
                        {
                            results["HighLevel"] = "No ID to delete.";
                        }
                        break;

                }
            }

            return results;
        }
    }
}
