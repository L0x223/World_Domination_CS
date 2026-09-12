using WorldDominationSignalR.Entites;

namespace WorldDominationSignalR.States;

public class SessionGameState
{
    public string SessionId { get; set; }
    public string JoinCode { get; set; }
    public const int MaxPlayers = 4;
    public const int MinPlayers = 4;
    
    public Dictionary<string, Player> PlayersById { get; set; } = new();       // playerId -> Player
    public Dictionary<string, string> ConnectionIdByPlayerId { get; set; } = new(); // playerId -> current connectionId
    public Dictionary<string, string> CountryByPlayerId { get; set; } = new(); // playerId -> countryId
    public Dictionary<string, CancellationTokenSource> DisconnectTimers { get; set; } = new();
    public HashSet<string> ReadyPlayerIds { get; set; } = new();
    public GamePhase Phase { get; set; } = GamePhase.Lobby;
}
public enum GamePhase { Lobby, CountrySelection, InProgress, Finished }