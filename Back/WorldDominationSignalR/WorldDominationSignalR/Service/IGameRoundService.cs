using WorldDominationSignalR.States;

namespace WorldDominationSignalR.Service;

public interface IGameRoundService
{
    void StartGame(SessionGameState session);

    bool RequestInvest(SessionGameState session, string playerId, string cityId);
    bool RequestShield(SessionGameState session, string playerId, string cityId);
    bool RequestNukeTech(SessionGameState session, string playerId);
    bool RequestBuildNukes(SessionGameState session, string playerId, int count);
    bool RequestEcologyInvest(SessionGameState session, string playerId);
    bool RequestSanctionToggle(SessionGameState session, string byPlayerId, string onPlayerId);
    bool RequestNukeStrike(SessionGameState session, string attackerPlayerId, string defenderPlayerId, string cityId);

    bool EndTurn(SessionGameState session, string playerId);
    
    bool TryResolveRound(SessionGameState session);
}