using WorldDominationSignalR.Entites;

namespace WorldDominationSignalR.Service;

public class TestDataBaseService : IDataBaseService
{
    public List<Player> Players { get; }

    public TestDataBaseService()
    {
        Players = new List<Player>()
        {
            new Player() { Nickname = "Lox223" },
            new Player() { Nickname = "BorisJonson" },
            new Player() { Nickname = "Lalka223" },
        };
    }

    public Task<Player?> GetByNickname(string nickname)
    {
        var player = Players.FirstOrDefault(p => p.Nickname.ToUpper().Equals(nickname.ToUpper()));
        return Task.FromResult(player);
    }

    public Task<bool> NicknameExistsAsync(string nickname)
    {
        bool exists = Players.Any(p => p.Nickname == nickname);
        return Task.FromResult(exists);
    }

    public Task AddPlayerAsync(Player player)
    {
        Players.Add(player);
        return Task.CompletedTask;
    }
}