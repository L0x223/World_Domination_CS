namespace WorldDominationSignalR.DTOs;

public class CityDto
{
    public string CityId { get; set; }
    public string CityName { get; set; }
    public bool IsAlive { get; set; }
    public bool? HasShield { get; set; }
    public double? Economic { get; set; } 
}