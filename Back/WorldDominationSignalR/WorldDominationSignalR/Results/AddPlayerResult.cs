namespace WorldDominationSignalR.Results;

public class AddPlayerResult
{
    public bool Success { get; set; }
    public string? PlayerId { get; set; }
    public string? Error { get; set; }
}