using WorldDominationSignalR.Entites;
using WorldDominationSignalR.Service;

namespace WorldDominationSignalR.States;

public class SessionGameState
{
    //Global Data
    public string SessionId { get; set; }
    public string JoinCode { get; set; }
    public const int MaxPlayers = 4;
    public const int MinPlayers = 4;

    public Dictionary<string, Player> PlayersById { get; set; } = new(); // playerId -> Player
    public Dictionary<string, string> ConnectionIdByPlayerId { get; set; } = new(); // playerId -> current connectionId
    public Dictionary<string, string> CountryByPlayerId { get; set; } = new(); // playerId -> countryId
    public Dictionary<string, CancellationTokenSource> DisconnectTimers { get; set; } = new();
    public HashSet<string> ReadyPlayerIds { get; set; } = new();
    public GamePhase Phase { get; set; } = GamePhase.Lobby;


    //Game Data
    public double Ecology { get; set; } = 0.8;
    public int Round { get; set; } = 1;
    public List<double> EcologyHistory { get; set; } = new();
    public const int MaxRounds = 6;

    public Dictionary<string, CountryGameState> CountryStateByPlayerId { get; set; } = new();
    public List<PendingNukeStrike> PendingNukeStrikes { get; set; } = new();
    public bool GameOver { get; set; } = false;
    public string? WinnerPlayerId { get; set; }
}

public enum GamePhase { Lobby, InProgress, Finished }