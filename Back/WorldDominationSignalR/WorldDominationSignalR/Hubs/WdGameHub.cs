using System.Net;
using Microsoft.AspNetCore.SignalR;
using WorldDominationSignalR.Entites;
using WorldDominationSignalR.Results;
using WorldDominationSignalR.Service;
using WorldDominationSignalR.States;

namespace WorldDominationSignalR.Hubs;

public class WdGameHub : Hub
{
    private static readonly char[] CodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ".ToCharArray();
    private static readonly Random Rng = new();

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
        if (isTaken) return HttpStatusCode.Conflict;

        var player = new Player { Nickname = nickname };
        await _db.AddPlayerAsync(player);

        return HttpStatusCode.Created;
    }

    public bool CheckSessionNameAvailability(string sessionName)
    {
        return _gameState.GetGameStateByName(sessionName) is null;
    }

    public CreateSessionResult CreateSession(string sessionName)
    {
        if (_gameState.GetGameStateByName(sessionName) is not null)
        {
            return new CreateSessionResult { Success = false, Error = "NameTaken" };
        }

        var joinCode = GenerateJoinCode();
        _gameState.CreateSessionState(sessionName, joinCode);

        return new CreateSessionResult { Success = true, JoinCode = joinCode };
    }

    public async Task<IEnumerable<Country>> GetAllCountries()
    {
        var countries = _gameState.GetAllCountries();
        return countries;
    }

    public async Task<IEnumerable<Player>> GetPlayersInLobby(string sessionId)
    {
        var players = _gameState.GetPlayersInLobby(sessionId);
        return players; 
    }
    
    public async Task<JoinSessionResult> JoinSessionByCode(string code, string nickname)
    {
        var state = _gameState.GetGameStateByJoinCode(code.ToUpperInvariant());
        if (state is null)
            return new JoinSessionResult { Success = false, Error = "SessionNotFound" };

        if (state.Phase != GamePhase.Lobby)
            return new JoinSessionResult { Success = false, Error = "SessionAlreadyStarted" };

        var player = await _db.GetByNickname(nickname);
        if (player is null)
            return new JoinSessionResult { Success = false, Error = "PlayerNotFound" };

        if (!_gameState.TryAddPlayer(state.SessionId, Context.ConnectionId, player))
            return new JoinSessionResult { Success = false, Error = "SessionFull" };

        await Groups.AddToGroupAsync(Context.ConnectionId, state.SessionId);
        await Clients.Group(state.SessionId).SendAsync("PlayerJoined", nickname);

        return new JoinSessionResult { Success = true, SessionId = state.SessionId };
    }
    private static string GenerateJoinCode(int length = 4)
    {
        return new string(Enumerable.Range(0, length)
            .Select(_ => CodeAlphabet[Rng.Next(CodeAlphabet.Length)])
            .ToArray());
    }
}