using WorldDominationSignalR.DTOs;
using WorldDominationSignalR.Entites;
using WorldDominationSignalR.States;

namespace WorldDominationSignalR.Service;

public interface IGameStateService
{
    SessionGameState CreateSessionState(string sessionName, string joinCode);
    SessionGameState? GetGameState(string sessionId);
    SessionGameState? GetGameStateByName(string sessionName);
    SessionGameState? GetGameStateByJoinCode(string joinCode);
    IEnumerable<Country> GetAvailableCountries(string sessionId);
    IEnumerable<Country> GetAllCountries();
    public bool TryAddPlayer(string sessionId, string playerId, Player player);
    public IEnumerable<PlayerLobbyDto> GetLobbyPlayers(string sessionId);
    public bool TrySelectCountry(string sessionId, string playerId, string countryId);

    void RemoveGameState(string sessionId);
}