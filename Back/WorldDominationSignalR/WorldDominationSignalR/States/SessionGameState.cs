using WorldDominationSignalR.Entites;

namespace WorldDominationSignalR.States;

public class SessionGameState
{
    public string SessionId { get; set; }
    public string JoinCode { get; set; }
    public const int MaxPlayers = 4;

    public Dictionary<string, Player> PlayersByConnectionId { get; set; } = new();
    public Dictionary<string, string> CountryByPlayerId { get; set; } = new();
    public GamePhase Phase { get; set; } = GamePhase.Lobby;
}
public enum GamePhase { Lobby, CountrySelection, InProgress, Finished }