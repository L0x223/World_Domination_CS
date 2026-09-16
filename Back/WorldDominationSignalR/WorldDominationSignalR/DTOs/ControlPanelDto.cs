namespace WorldDominationSignalR.DTOs;

public class ControlPanelDto
{
    public int MaxRounds { get; set; }
    
    public int Budget { get; set; }

    public bool HasNuclearTech { get; set; }
    public int NukeTechCost { get; set; }

    public int NukeCount { get; set; }
    public int NukesCommittedThisRound { get; set; }
    public int CostNuke { get; set; }
    
    public int CostEcology { get; set; }

    public int CostInvest { get; set; }
    public int CostShield { get; set; }

    public bool HasEndedTurn { get; set; }

    public List<CityActionDto> MyCities { get; set; } = new();
    public List<TargetableCountryDto> ForeignCountries { get; set; } = new();
}

public class CityActionDto
{
    public string CityId { get; set; }
    public string CityName { get; set; }
    public bool IsAlive { get; set; }
    public bool HasShield { get; set; }
    public bool InvestRequested { get; set; }
    public bool ShieldRequested { get; set; }
}

public class TargetableCountryDto
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string CountryName { get; set; }
    public bool IsSanctioned { get; set; }
    public bool SanctionToggleRequested { get; set; }
    public List<TargetableCityDto> Cities { get; set; } = new();
}

public class TargetableCityDto
{
    public string CityId { get; set; }
    public string CityName { get; set; }
    public bool CanStrike { get; set; }
    public bool IsAlive { get; set; }
}