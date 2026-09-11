namespace WorldDominationSignalR.DTOs;

public class PlayerLobbyDto
{
    public string ConnectionId { get; set; }
    public string Nickname { get; set; }
    public string? CountryId { get; set; }
    public string? CountryName { get; set; }
    public string? CountryLeaderIconId { get; set; }
}