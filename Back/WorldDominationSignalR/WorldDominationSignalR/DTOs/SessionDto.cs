using WorldDominationSignalR.States;

namespace WorldDominationSignalR.DTOs;

public class SessionDto
{
    public string SessionName { get; set; } //for now this is basically Session ID, named this way in case of separation 
    public int PlayerCount { get; set; }
    public int MaxPlayers { get; set; }
    public GamePhase Phase { get; set; }
}