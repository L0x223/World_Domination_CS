namespace WorldDominationSignalR.Entites;

public class Country
{
    public const int RequiredCityCount = 4;

    public string Id { get; set; }
    public string Name { get; set; }
    public string LeaderIconId { get; set; }
    public List<City> Cities { get; set; } = new();

    public class City
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double StartingEconomic { get; set; } = 0.6;
    }
}