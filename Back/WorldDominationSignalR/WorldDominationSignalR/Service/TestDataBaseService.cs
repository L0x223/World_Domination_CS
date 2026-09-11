using WorldDominationSignalR.Entites;

namespace WorldDominationSignalR.Service;

public class TestDataBaseService : IDataBaseService
{
    public List<Player> Players { get; }

    public TestDataBaseService()
    {
        Players = new List<Player>()
        {
            new Player() { Id = "1", Nickname = "Lox223" },
            new Player() { Id = "2",Nickname = "BorisJonson" },
            new Player() { Id = "3",Nickname = "Lalka223" },
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
        player.Id = Guid.NewGuid().ToString();
        Players.Add(player);
        return Task.CompletedTask;
    }

    public async Task<Player?> GetById(string id)
    {
        var player = Players.FirstOrDefault(p => p.Id == id);
        return player;
    }
}