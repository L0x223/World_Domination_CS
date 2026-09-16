namespace WorldDominationSignalR.DTOs;

public class TurnActionsDto
{
    public List<string> InvestCityIds { get; set; } = new();
    public List<string> ShieldCityIds { get; set; } = new();
    public bool NukeTech { get; set; }
    public int NukesToBuild { get; set; }
    public bool EcologyInvest { get; set; }
    public List<string> SanctionTogglePlayerIds { get; set; } = new();
    
    public List<NukeStrikeDto> NukeStrikes { get; set; } = new();
}

public class NukeStrikeDto
{
    public string DefenderPlayerId { get; set; } = string.Empty;
    public string CityId { get; set; } = string.Empty;
}