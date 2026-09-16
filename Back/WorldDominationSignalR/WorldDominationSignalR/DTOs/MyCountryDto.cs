namespace WorldDominationSignalR.DTOs;

public class MyCountryDto
{
    public string CountryId { get; set; }
    public string CountryName { get; set; }
    public int Budget { get; set; }
    public bool HasNuclearTech { get; set; }
    public int NukeCount { get; set; }
    public bool IsEliminated { get; set; }
    public List<string> SanctionedByPlayerIds { get; set; } = new();  
    public List<CityDto> Cities { get; set; } = new();
}