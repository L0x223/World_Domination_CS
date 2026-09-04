namespace WorldDominationSignalR.Results;

public class CreateSessionResult
{
    public bool Success { get; set; }
    public string JoinCode { get; set; }
    public string? Error { get; set; }
}