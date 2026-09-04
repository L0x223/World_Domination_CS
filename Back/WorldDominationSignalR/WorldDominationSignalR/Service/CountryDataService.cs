using System.Text.Json;

namespace WorldDominationSignalR.Service;
public class Country
{
    private const int MaxCities = 4;
    
    public string Id { get; set; }  
    public string Name { get; set; } 
    public string LeaderIconId { get; set; }
    public List<City> Cities { get; set; } = new List<City>();
    public class City
    {
        public string Id { get; set; }
        public string Name { get; set; }
        //there will be stats
    }
}

public interface ICountryDataService
{
    IReadOnlyList<Country> GetAll();
}

public class CountryDataService : ICountryDataService
{
    private readonly List<Country> _countries;

    public CountryDataService()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "countries.json");
        var json = File.ReadAllText(path);
        _countries = JsonSerializer.Deserialize<List<Country>>(json, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public IReadOnlyList<Country> GetAll() => _countries;
}