using WorldDominationSignalR.DTOs;
using WorldDominationSignalR.Entites;
using WorldDominationSignalR.States;

namespace WorldDominationSignalR.Service;

public interface IGameStateService
{
    SessionGameState CreateSessionState(string sessionName, string joinCode, bool isPrivate = false);//isPrivate for future use
    SessionGameState? GetGameState(string sessionId);
    SessionGameState? GetGameStateByName(string sessionName);
    SessionGameState? GetGameStateByJoinCode(string joinCode);
    IEnumerable<Country> GetAvailableCountries(string sessionId);
    IEnumerable<Country> GetAllCountries();
    public bool TryAddPlayer(string sessionId, string playerId, Player player);
    public IEnumerable<PlayerLobbyDto> GetLobbyPlayers(string sessionId);
    public bool TrySelectCountry(string sessionId, string playerId, string countryId);
    public IEnumerable<SessionDto>  GetSessions(string filter = "", bool excludeFullSessions = false);
    public string? GetJoinCodeById(string sessionId);
    void RemoveGameState(string sessionId);

    public string? GetSessionIdByConnectionId(string connectionId);
    public string? GetPlayerIdByConnectionId(string sessionId, string connectionId);
    public void MarkPlayerDisconnected(string sessionId, string playerId, Action onGracePeriodExpired);
    public void CancelDisconnectTimer(string sessionId, string playerId);
    public void RemovePlayer(string sessionId, string playerId);
    public bool TrySetReady(string sessionId, string playerId, bool isReady);
    public bool AreAllPlayersReady(string sessionId);
}