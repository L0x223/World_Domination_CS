namespace WorldDominationSignalR.DTOs;

public class GameOverDto
{
    public string WinnerPlayerId { get; set; } = string.Empty;
    public string WinnerNickname { get; set; } = string.Empty;
    public string WinnerCountry { get; set; } = string.Empty;
    public double WinnerWealth { get; set; }
}