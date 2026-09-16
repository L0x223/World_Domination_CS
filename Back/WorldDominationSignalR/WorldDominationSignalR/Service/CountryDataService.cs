using System.Text.Json;
using WorldDominationSignalR.Entites;

namespace WorldDominationSignalR.Service;

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

        foreach (var country in _countries)
        {
            if (country.Cities.Count != Country.RequiredCityCount)
                throw new InvalidOperationException(
                    $"Country '{country.Id}' must have exactly {Country.RequiredCityCount} cities, has {country.Cities.Count}.");
        }
    }

    public IReadOnlyList<Country> GetAll() => _countries;
}