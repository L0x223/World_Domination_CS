namespace WorldDominationSignalR.States;

public class CityState
{
    public string CityId { get; set; }
    public bool IsAlive { get; set; } = true;
    public double Economic { get; set; } = 0.6;
    public bool HasShield { get; set; } = false;
    
    public bool InvestRequested { get; set; } = false;
    public bool ShieldRequested { get; set; } = false;

    public double GetWealth(double ecology) => Math.Round((3 * Economic + 2 * ecology) / 5, 2);
    public int GetIncome() => (int)Math.Round(250 * Economic);
}