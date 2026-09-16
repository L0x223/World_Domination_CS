namespace WorldDominationSignalR.DTOs;

public class OtherCountryDto
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string CountryId { get; set; }
    public string CountryName { get; set; }
    public bool IsEliminated { get; set; }
    public List<CityDto> Cities { get; set; } = new();
}