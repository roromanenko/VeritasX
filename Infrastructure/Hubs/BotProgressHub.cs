using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;

public class BotProgressHub : Hub
{
	public async Task JoinGroup(string botId)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, $"bot-{botId}");
	}

	public async Task LeaveGroup(string botId)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"bot-{botId}");
	}
}
