using WorldDominationSignalR.Entites;

namespace WorldDominationSignalR.Service;

public interface IDataBaseService
{
    Task<Player?> GetByNickname(string nickname);
    Task<bool> NicknameExistsAsync(string nickname);
    Task AddPlayerAsync(Player player);
    Task<Player?> GetById(string id);
}