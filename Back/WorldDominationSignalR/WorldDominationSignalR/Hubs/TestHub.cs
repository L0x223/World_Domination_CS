using Microsoft.AspNetCore.SignalR;

namespace WorldDominationSignalR.Hubs;

public class TestHub : Hub
{
    public async Task SendStockData()
    {
        await Clients.All.SendAsync("ReceiveStockData", new { Symbol = 1, Price = 2, Time = 3 });
    }
}