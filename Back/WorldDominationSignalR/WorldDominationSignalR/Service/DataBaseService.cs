using WorldDominationSignalR.Context;
using WorldDominationSignalR.Entites;

namespace WorldDominationSignalR.Service;

public class DataBaseService : IDataBaseService
{
    private readonly AppDbContext _dbContext;

    public DataBaseService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Player?> GetByNickname(string nickname) => throw new NotImplementedException();
    public Task<bool> NicknameExistsAsync(string nickname) => throw new NotImplementedException();
    public Task AddPlayerAsync(Player player) => throw new NotImplementedException();
}