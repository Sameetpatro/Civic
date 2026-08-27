using Microsoft.AspNetCore.SignalR;

namespace CivicFix.Api.Hubs;

public interface IIncidentDispatchClient
{
    Task ReceiveNewIncident(object incidentSummary);
    Task ReceiveIncidentUpdate(object incidentUpdate);
    Task ReceiveEmergencyAlert(object alertData);
}

public class IncidentDispatchHub : Hub<IIncidentDispatchClient>
{
    public async Task JoinDepartmentChannel(string departmentCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Dept_{departmentCode}");
    }

    public async Task LeaveDepartmentChannel(string departmentCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Dept_{departmentCode}");
    }

    public async Task JoinWardChannel(string wardName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Ward_{wardName}");
    }
}
