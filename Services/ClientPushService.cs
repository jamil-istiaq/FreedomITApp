using FreedomITAS;
using FreedomITAS.API_Serv;
using FreedomITAS.Data;
using FreedomITAS.Models;
using FreedomITAS.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class ClientPushService
{
    private readonly ClientCreateService _createService;
    private readonly ClientUpdateService _updateService;
    private readonly ClientDeleteService _deleteService;

    public ClientPushService(
         ClientCreateService createService,
         ClientUpdateService updateService,
         ClientDeleteService deleteService)
    {
        _createService = createService;
        _updateService = updateService;
        _deleteService = deleteService;
    }
    public Task<Dictionary<string, string>> CreateAsync(ClientModel client, List<string> systems) =>
         _createService.CreateClientAsync(client, systems);

    public Task<Dictionary<string, string>> UpdateAsync(ClientModel client, List<string> systems) =>
        _updateService.UpdateClientAsync(client, systems);

    public Task<Dictionary<string, string>> DeleteAsync(ClientModel client, List<string> systems) =>
        _deleteService.DeleteClientAsync(client, systems);
}
