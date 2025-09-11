using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

public class JobProgressHub : Hub
{
	public async Task JoinGroup(string jobId)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, $"job-{jobId}");
	}

	public async Task LeaveGroup(string jobId)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"job-{jobId}");
	}
}
