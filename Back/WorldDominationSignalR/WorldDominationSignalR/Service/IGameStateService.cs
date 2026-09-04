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
    IEnumerable<Player> GetPlayersInLobby(string sessionId);
    public bool TryAddPlayer(string sessionId, string playerId, Player player);

    void RemoveGameState(string sessionId);
}