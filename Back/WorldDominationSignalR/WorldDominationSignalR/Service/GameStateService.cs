using System.Collections.Concurrent;
using WorldDominationSignalR.DTOs;
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
    public SessionGameState CreateSessionState(string sessionId, string joinCode, bool isPrivate = false)//isPrivate for future use
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
    

    public bool TryAddPlayer(string sessionId, string playerId, Player player)
    {
        var state = GetGameState(sessionId);
        if (state is null) return false;

        lock (state)
        {
            if (state.PlayersById.ContainsKey(player.Id))
            {
                // Same player reconnecting just update their connection
                state.ConnectionIdByPlayerId[player.Id] = playerId;
                CancelDisconnectTimerLocked(state, player.Id); 
                return true;
            }

            if (state.PlayersById.Count >= SessionGameState.MaxPlayers) return false;

            state.PlayersById[player.Id] = player;
            state.ConnectionIdByPlayerId[player.Id] = playerId;
            return true;
        }
    }
    public void CancelDisconnectTimer(string sessionId, string playerId)
    {
        var state = GetGameState(sessionId);
        if (state is null) return;

        lock (state)
        {
            CancelDisconnectTimerLocked(state, playerId);
        }
    }

    // Assumes caller already holds lock (state)
    private void CancelDisconnectTimerLocked(SessionGameState state, string playerId)
    {
        if (state.DisconnectTimers.TryGetValue(playerId, out var cts))
        {
            cts.Cancel();
            state.DisconnectTimers.Remove(playerId);
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

    public IEnumerable<SessionDto> GetSessions(string filter = "", bool excludeFullSessions = false)
    {
        return _sessions.Values
            .Where(s => s.Phase == GamePhase.Lobby)
            .Where(s => string.IsNullOrEmpty(filter) ||
                        s.SessionId.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Where(s => !excludeFullSessions || s.PlayersById.Count < SessionGameState.MaxPlayers)
            .Select(s => new SessionDto
            {
                SessionName = s.SessionId,
                PlayerCount = s.PlayersById.Count,
                MaxPlayers = SessionGameState.MaxPlayers,
                Phase = s.Phase
            })
            .ToList();
    }

    public string? GetJoinCodeById(string sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var state) ? state.JoinCode : null;
    }

    public void RemoveGameState(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var state))
            _sessionIdByJoinCode.TryRemove(state.JoinCode, out _);
    }
    
    public IEnumerable<PlayerLobbyDto> GetLobbyPlayers(string sessionId)
    {
        var state = GetGameState(sessionId);
        if (state is null) return Enumerable.Empty<PlayerLobbyDto>();

        var allCountries = _countryDataService.GetAll();

        lock (state)
        {
            return state.PlayersById.Select(kvp =>
            {
                var playerId = kvp.Key;
                var player = kvp.Value;
                state.CountryByPlayerId.TryGetValue(playerId, out var countryId);
                var country = countryId is null ? null : allCountries.FirstOrDefault(c => c.Id == countryId);
                state.ConnectionIdByPlayerId.TryGetValue(playerId, out var connectionId);

                return new PlayerLobbyDto
                {
                    ConnectionId = connectionId,
                    Nickname = player.Nickname,
                    CountryId = countryId,
                    CountryName = country?.Name,
                    CountryLeaderIconId =  country?.LeaderIconId,
                    IsReady = state.ReadyPlayerIds.Contains(playerId),
                };
            }).ToList();
        }
        
    
    }
    //disconect
    public string? GetSessionIdByConnectionId(string connectionId)
    {
        foreach (var state in _sessions.Values)
        {
            lock (state)
            {
                if (state.ConnectionIdByPlayerId.Values.Contains(connectionId))
                    return state.SessionId;
            }
        }
        return null;
    }

    public string? GetPlayerIdByConnectionId(string sessionId, string connectionId)
    {
        var state = GetGameState(sessionId);
        if (state is null) return null;

        lock (state)
        {
            return state.ConnectionIdByPlayerId
                .FirstOrDefault(kvp => kvp.Value == connectionId).Key;
        }
    }

    public void MarkPlayerDisconnected(string sessionId, string playerId, Action onGracePeriodExpired)
    {
        var state = GetGameState(sessionId);
        if (state is null) return;

        lock (state)
        {
            var cts = new CancellationTokenSource();
            state.DisconnectTimers[playerId] = cts;

            _ = Task.Delay(TimeSpan.FromSeconds(120), cts.Token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    RemovePlayer(sessionId, playerId);
                    onGracePeriodExpired();
                }
            }, TaskScheduler.Default);
        }
    }

    public void RemovePlayer(string sessionId, string playerId)
    {
        var state = GetGameState(sessionId);
        if (state is null) return;

        lock (state)
        {
            state.PlayersById.Remove(playerId);
            state.ConnectionIdByPlayerId.Remove(playerId);
            state.CountryByPlayerId.Remove(playerId);
            state.DisconnectTimers.Remove(playerId);
        }
    }
    //
    
    public bool TrySetReady(string sessionId, string playerId, bool isReady)
    {
        var state = GetGameState(sessionId);
        if (state is null) return false;

        lock (state)
        {
            if (!state.PlayersById.ContainsKey(playerId)) return false;

            if (isReady) state.ReadyPlayerIds.Add(playerId);
            else state.ReadyPlayerIds.Remove(playerId);

            return true;
        }
    }

    public bool AreAllPlayersReady(string sessionId)
    {
        var state = GetGameState(sessionId);
        if (state is null) return false;

        lock (state)
        {
            return state.PlayersById.Count >= SessionGameState.MinPlayers
                   && state.PlayersById.Keys.All(id => state.ReadyPlayerIds.Contains(id));
        }
    }
    
}