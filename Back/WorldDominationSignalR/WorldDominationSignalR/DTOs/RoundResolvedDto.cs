namespace WorldDominationSignalR.DTOs;

public class RoundResolvedDto
{
    public int Round { get; set; }
    public int MaxRounds { get; set; }
    public List<double> EcologyHistory { get; set; }
    public bool GameOver { get; set; }
    public string? WinnerPlayerId { get; set; }

    public MyCountryDto Me { get; set; }
    public List<OtherCountryDto> Others { get; set; } = new();
}