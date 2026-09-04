using System.Collections.Concurrent;
using WorldDominationSignalR.Entites;
using WorldDominationSignalR.States;

namespace WorldDominationSignalR.Service;

public class GameStateService : IGameStateService
{
    private readonly ConcurrentDictionary<string, SessionGameState> _sessions = new();
    private readonly ConcurrentDictionary<string, string> _sessionIdByJoinCode = new();
    
    private readonly ICountryDataService _countryDataService;

    public GameStateService(ICountryDataService countryDataService)
    {
        _countryDataService = countryDataService;
    }
    public SessionGameState CreateSessionState(string sessionId, string joinCode)
    {
        var state = new SessionGameState { SessionId = sessionId, JoinCode = joinCode };
        _sessions[sessionId] = state;
        _sessionIdByJoinCode[joinCode] = sessionId;
        return state;
    }

    public SessionGameState? GetGameState(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var state);
        return state;
    }

    public SessionGameState? GetGameStateByName(string sessionName) => GetGameState(sessionName);
    
    public SessionGameState? GetGameStateByJoinCode(string joinCode)
    {
        return _sessionIdByJoinCode.TryGetValue(joinCode, out var sessionId)
            ? GetGameState(sessionId)
            : null;
    }
    

    public bool TryAddPlayer(string sessionId, string connectionId, Player player)
    {
        var state = GetGameState(sessionId);
        if (state is null) return false;

        lock (state)
        {
            if (state.PlayersByConnectionId.Count >= SessionGameState.MaxPlayers) return false;
            if (state.PlayersByConnectionId.ContainsKey(connectionId)) return true;
            state.PlayersByConnectionId[connectionId] = player;
            return true;
        }
    }

    public IEnumerable<Player> GetPlayersInLobby(string sessionId)
    {
        var state = GetGameState(sessionId);
        if (state is null) return Enumerable.Empty<Player>();

        lock (state)
        {
            return state.PlayersByConnectionId.Values.ToList();
        }
    }
    

    public IEnumerable<Country> GetAllCountries()
    {
        return  _countryDataService.GetAll();
    }

    public IEnumerable<Country> GetAvailableCountries(string sessionId)
    {
        var state = GetGameState(sessionId);
        if (state is null) return Enumerable.Empty<Country>();

        var taken = state.CountryByPlayerId.Values.ToHashSet();
        return _countryDataService.GetAll().Where(c => !taken.Contains(c.Id));
    }

    public bool TrySelectCountry(string sessionId, string playerId, string countryId)
    {
        var state = GetGameState(sessionId);
        if (state is null) return false;

        lock (state)
        {
            if (state.CountryByPlayerId.Values.Contains(countryId)) return false;
            state.CountryByPlayerId[playerId] = countryId;
            return true;
        }
    }

    public void RemoveGameState(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var state))
            _sessionIdByJoinCode.TryRemove(state.JoinCode, out _);
    }
}