using System.Net;
using Microsoft.AspNetCore.SignalR;
using WorldDominationSignalR.Context;
using WorldDominationSignalR.Entites;
using WorldDominationSignalR.Service;

namespace WorldDominationSignalR.Hubs;

public class WdGameHub : Hub
{
    private readonly IGameStateService _gameState;
    private readonly IDataBaseService _db;

    public WdGameHub(IGameStateService gameState, IDataBaseService db)
    {
        _gameState = gameState;
        _db = db;
    } 
    
    public async Task<bool> CheckNickAvailability(string nickname)
    {
        var player = await _db.GetByNickname(nickname);
        return player is null; 
    }

    public async Task<HttpStatusCode> AddPlayerToDatabase(string nickname)
    {
        bool isTaken = await _db.NicknameExistsAsync(nickname);
        if (isTaken)
        {
            return HttpStatusCode.Conflict; // 409
        }

        var player = new Player { Nickname = nickname };
        await _db.AddPlayerAsync(player);

        return HttpStatusCode.Created; // 201
    }
}
