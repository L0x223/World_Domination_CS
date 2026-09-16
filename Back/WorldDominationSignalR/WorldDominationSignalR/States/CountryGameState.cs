namespace WorldDominationSignalR.States;

public class CountryGameState
{
    public string CountryId { get; set; }
    public string PlayerId { get; set; }
    public int Budget { get; set; } = 500;
    public bool HasNuclearTech { get; set; } = false;
    public int NukeCount { get; set; } = 0;
    public bool IsEliminated { get; set; } = false;
    public bool EndedTurn { get; set; } = false;

    public HashSet<string> SanctionedBy { get; set; } = new();         
    public HashSet<string> PendingSanctionToggles { get; set; } = new();

    public bool NukeTechRequested { get; set; } = false;
    public int NukesToBuildRequested { get; set; } = 0;
    public bool EcologyInvestRequested { get; set; } = false;

    public List<CityState> Cities { get; set; } = new();

    public bool IsAlive => !IsEliminated;
}