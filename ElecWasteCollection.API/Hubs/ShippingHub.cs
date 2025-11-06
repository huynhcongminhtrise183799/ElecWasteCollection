using Microsoft.AspNetCore.SignalR;

namespace ElecWasteCollection.API.Hubs
{
	public class ShippingHub : Hub
	{
		public async Task JoinShipperGroup(string shipperId)
		{
			string groupName = $"Shipper_{shipperId}";
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
		}

	}
}
