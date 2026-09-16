using Microsoft.AspNetCore.SignalR;
using WorldDominationSignalR.DTOs;
using WorldDominationSignalR.Entites;
using WorldDominationSignalR.Results;
using WorldDominationSignalR.Service;
using WorldDominationSignalR.States;

namespace WorldDominationSignalR.Hubs;

public class WdGameHub : Hub
{
    private static readonly char[] CodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ".ToCharArray();
    private static readonly Random Rng = new();

    private readonly IGameStateService _gameState;
    private readonly IDataBaseService _db;
    private readonly IGameRoundService _gameRound;
    private readonly ICountryDataService _countryDataService; // gameState implementation uses countryDataService too, segregate this in future

    public WdGameHub(IGameStateService gameState, IDataBaseService db, IGameRoundService gameRoundService, ICountryDataService countryDataService)
    {
        _gameState = gameState;
        _db = db;
        _gameRound = gameRoundService;
        _countryDataService = countryDataService;
    }

    public async Task<bool> CheckNickAvailability(string nickname)
    {
        var player = await _db.GetByNickname(nickname);
        return player is null;
    }

    public async Task<AddPlayerResult> AddPlayerToDatabase(string nickname)
    {
        bool isTaken = await _db.NicknameExistsAsync(nickname);
        if (isTaken)
            return new AddPlayerResult { Success = false, Error = "NicknameTaken" };

        var player = new Player { Nickname = nickname };
        await _db.AddPlayerAsync(player);

        return new AddPlayerResult { Success = true, PlayerId = player.Id };
    }

    public bool CheckSessionNameAvailability(string sessionName)
    {
        return _gameState.GetGameStateByName(sessionName) is null;
    }

    public CreateSessionResult CreateSession(string sessionName)
    {
        if (_gameState.GetGameStateByName(sessionName) is not null)
        {
            return new CreateSessionResult { Success = false, Error = "NameTaken" };
        }

        var joinCode = GenerateJoinCode();
        _gameState.CreateSessionState(sessionName, joinCode);

        return new CreateSessionResult { Success = true, JoinCode = joinCode };
    }

    public IEnumerable<Country> GetAllCountries()
    {
        return _countryDataService.GetAll();
    }

    public IEnumerable<PlayerLobbyDto> GetPlayersInLobby(string sessionId)
    {
        return _gameState.GetLobbyPlayers(sessionId);
    }

    private static string GenerateJoinCode(int length = 4)
    {
        return new string(Enumerable.Range(0, length)
            .Select(_ => CodeAlphabet[Rng.Next(CodeAlphabet.Length)])
            .ToArray());
    }

    public async Task<JoinSessionResult> RejoinSession(string sessionId, string playerId)
    {
        var state = _gameState.GetGameState(sessionId);
        if (state is null)
            return new JoinSessionResult { Success = false, Error = "SessionNotFound" };

        var player = await _db.GetById(playerId);
        if (player is null)
            return new JoinSessionResult { Success = false, Error = "PlayerNotFound" };

        if (!_gameState.TryAddPlayer(state.SessionId, Context.ConnectionId, player))
            return new JoinSessionResult { Success = false, Error = "SessionFull" };

        await Groups.AddToGroupAsync(Context.ConnectionId, state.SessionId);
        return new JoinSessionResult { Success = true, SessionId = state.SessionId };
    }

    public async Task<bool> SelectCountry(string sessionId, string playerId, string countryId)
    {
        var success = _gameState.TrySelectCountry(sessionId, playerId, countryId);
        if (success)
            await BroadcastLobbyPlayers(sessionId);

        return success;
    }

    public IEnumerable<Country> GetAvailableCountries(string sessionId)
    {
        return _gameState.GetAvailableCountries(sessionId);
    }

    private async Task BroadcastLobbyPlayers(string sessionId)
    {
        var players = _gameState.GetLobbyPlayers(sessionId);
        await Clients.Group(sessionId).SendAsync("PlayersUpdated", players);
    }

    public IEnumerable<SessionDto> GetSessions(string filter = "", bool excludeFullSessions = false)
    {
        return _gameState.GetSessions(filter, excludeFullSessions);
    }

    private async Task<JoinSessionResult> JoinSessionInternal(SessionGameState state, string nickname)
    {
        if (state.Phase != GamePhase.Lobby)
            return new JoinSessionResult { Success = false, Error = "SessionAlreadyStarted" };

        var player = await _db.GetByNickname(nickname);
        if (player is null)
            return new JoinSessionResult { Success = false, Error = "PlayerNotFound" };

        if (!_gameState.TryAddPlayer(state.SessionId, Context.ConnectionId, player))
            return new JoinSessionResult { Success = false, Error = "SessionFull" };

        await Groups.AddToGroupAsync(Context.ConnectionId, state.SessionId);
        await BroadcastLobbyPlayers(state.SessionId);

        return new JoinSessionResult { Success = true, SessionId = state.SessionId };
    }

    public async Task<JoinSessionResult> JoinSessionByCode(string code, string nickname)
    {
        var state = _gameState.GetGameStateByJoinCode(code.ToUpperInvariant());
        return state is null
            ? new JoinSessionResult { Success = false, Error = "SessionNotFound" }
            : await JoinSessionInternal(state, nickname);
    }

    public async Task<JoinSessionResult> JoinSessionById(string sessionId, string nickname)
    {
        var state = _gameState.GetGameState(sessionId);
        return state is null
            ? new JoinSessionResult { Success = false, Error = "SessionNotFound" }
            : await JoinSessionInternal(state, nickname);
    }

    public string? GetJoinCodeById(string sessionId)
    {
        return _gameState.GetJoinCodeById(sessionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var sessionId = _gameState.GetSessionIdByConnectionId(Context.ConnectionId);
        if (sessionId is not null)
        {
            var playerId = _gameState.GetPlayerIdByConnectionId(sessionId, Context.ConnectionId);
            if (playerId is not null)
            {
                _gameState.MarkPlayerDisconnected(sessionId, playerId, () =>
                {
                    _ = Clients.Group(sessionId).SendAsync("PlayersUpdated", _gameState.GetLobbyPlayers(sessionId));
                });
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<bool> SetReady(string sessionId, string playerId, bool isReady)
    {
        var success = _gameState.TrySetReady(sessionId, playerId, isReady);
        if (success)
        {
            await Clients.Group(sessionId).SendAsync("PlayersUpdated", _gameState.GetLobbyPlayers(sessionId));

            if (_gameState.AreAllPlayersReady(sessionId))
            {
                var session = _gameState.GetGameState(sessionId);
                if (session is not null)
                {
                    _gameRound.StartGame(session);
                    await Clients.Group(sessionId).SendAsync("GameStarted");
                }
            }
        }
        return success;
    }
    
    // ----- GAME RELATED LOGIC -----

    public bool RequestInvest(string sessionId, string playerId, string cityId)
    {
        var session = _gameState.GetGameState(sessionId);
        return session is not null && _gameRound.RequestInvest(session, playerId, cityId);
    }

    public bool RequestShield(string sessionId, string playerId, string cityId)
    {
        var session = _gameState.GetGameState(sessionId);
        return session is not null && _gameRound.RequestShield(session, playerId, cityId);
    }

    public bool RequestNukeTech(string sessionId, string playerId)
    {
        var session = _gameState.GetGameState(sessionId);
        return session is not null && _gameRound.RequestNukeTech(session, playerId);
    }

    public bool RequestBuildNukes(string sessionId, string playerId, int count)
    {
        var session = _gameState.GetGameState(sessionId);
        return session is not null && _gameRound.RequestBuildNukes(session, playerId, count);
    }

    public bool RequestEcologyInvest(string sessionId, string playerId)
    {
        var session = _gameState.GetGameState(sessionId);
        return session is not null && _gameRound.RequestEcologyInvest(session, playerId);
    }

    public bool RequestSanctionToggle(string sessionId, string byPlayerId, string onPlayerId)
    {
        var session = _gameState.GetGameState(sessionId);
        return session is not null && _gameRound.RequestSanctionToggle(session, byPlayerId, onPlayerId);
    }

    public bool RequestNukeStrike(string sessionId, string attackerPlayerId, string defenderPlayerId, string cityId)
    {
        var session = _gameState.GetGameState(sessionId);
        return session is not null && _gameRound.RequestNukeStrike(session, attackerPlayerId, defenderPlayerId, cityId);
    }

    

    private RoundResolvedDto BuildRoundResolvedDto(SessionGameState session, string viewerPlayerId)
    {
        var allCountries = _countryDataService.GetAll();
        var myState = session.CountryStateByPlayerId[viewerPlayerId];
        var myCountryData = allCountries.First(c => c.Id == myState.CountryId);

        CityDto ToCityDto(States.CityState city, Country.City? cityData, bool reveal)
        {
            return new CityDto
            {
                CityId = city.CityId,
                CityName = cityData?.Name ?? city.CityId,
                IsAlive = city.IsAlive,
                HasShield = reveal ? city.HasShield : null,
                Economic = reveal ? city.Economic : null
            };
        }

        var myDto = new MyCountryDto
        {
            CountryId = myState.CountryId,
            CountryName = myCountryData.Name,
            Budget = myState.Budget,
            HasNuclearTech = myState.HasNuclearTech,
            NukeCount = myState.NukeCount,
            IsEliminated = myState.IsEliminated,
            SanctionedByPlayerIds = myState.SanctionedBy.ToList(),
            Cities = myState.Cities.Select(c =>
            {
                var cityData = myCountryData.Cities.FirstOrDefault(cd => cd.Id == c.CityId);
                return ToCityDto(c, cityData, true);
            }).ToList()
        };

        var others = session.CountryStateByPlayerId.Values
            .Where(c => c.PlayerId != viewerPlayerId)
            .Select(otherState =>
            {
                var otherCountryData = allCountries.First(c => c.Id == otherState.CountryId);
                session.PlayersById.TryGetValue(otherState.PlayerId, out var otherPlayer);

                return new OtherCountryDto
                {
                    PlayerId = otherState.PlayerId,
                    PlayerName = otherPlayer?.Nickname ?? "Unknown",
                    CountryId = otherState.CountryId,
                    CountryName = otherCountryData.Name,
                    IsEliminated = otherState.IsEliminated,
                    Cities = otherState.Cities.Select(c =>
                    {
                        var cityData = otherCountryData.Cities.FirstOrDefault(cd => cd.Id == c.CityId);
                        return ToCityDto(c, cityData, false);
                    }).ToList()
                };
            }).ToList();

        return new RoundResolvedDto
        {
            Round = session.Round,
            MaxRounds = SessionGameState.MaxRounds,
            EcologyHistory = session.EcologyHistory,
            GameOver = session.GameOver,
            WinnerPlayerId = session.WinnerPlayerId,
            Me = myDto,
            Others = others
        };
    }

    private ControlPanelDto BuildControlPanelDto(SessionGameState session, string viewerPlayerId)
    {
        var allCountries = _countryDataService.GetAll();
        var me = session.CountryStateByPlayerId[viewerPlayerId];
        var myCountryData = allCountries.First(c => c.Id == me.CountryId);

        int committedNukes = session.PendingNukeStrikes.Count(s => s.AttackerPlayerId == viewerPlayerId);
        int nukesAvailable = me.NukeCount - committedNukes;

        var myCities = me.Cities.Select(c =>
        {
            var cityData = myCountryData.Cities.FirstOrDefault(cd => cd.Id == c.CityId);
            return new CityActionDto
            {
                CityId = c.CityId,
                CityName = cityData?.Name ?? c.CityId,
                IsAlive = c.IsAlive,
                HasShield = c.HasShield,
                InvestRequested = c.InvestRequested,
                ShieldRequested = c.ShieldRequested
            };
        }).ToList();

        var foreignCountries = session.CountryStateByPlayerId.Values
            .Where(c => c.PlayerId != viewerPlayerId && c.IsAlive)
            .Select(other =>
            {
                var otherCountryData = allCountries.First(c => c.Id == other.CountryId);

                return new TargetableCountryDto
                {
                    PlayerId = other.PlayerId,
                    CountryName = otherCountryData.Name,
                    IsSanctioned = other.SanctionedBy.Contains(viewerPlayerId),
                    SanctionToggleRequested = other.PendingSanctionToggles.Contains(viewerPlayerId),
                    Cities = other.Cities.Select(c =>
                    {
                        var cityData = otherCountryData.Cities.FirstOrDefault(cd => cd.Id == c.CityId);
                        return new TargetableCityDto
                        {
                            CityId = c.CityId,
                            CityName = cityData?.Name ?? c.CityId,
                            IsAlive = c.IsAlive,
                            CanStrike = c.IsAlive && nukesAvailable > 0
                        };
                    }).ToList()
                };
            }).ToList();

        return new ControlPanelDto
        {
            MaxRounds =  SessionGameState.MaxRounds,
            Budget = me.Budget,
            HasNuclearTech = me.HasNuclearTech,
            NukeTechCost = GameConstants.NukeTechCost,
            NukeCount = me.NukeCount,
            NukesCommittedThisRound = committedNukes,
            CostNuke = GameConstants.CostNuke,
            CostEcology = GameConstants.CostEcology,
            CostInvest = GameConstants.CostInvest,
            CostShield = GameConstants.CostShield,
            HasEndedTurn = me.EndedTurn,
            MyCities = myCities,
            ForeignCountries = foreignCountries
        };
    }
    public async Task<bool> EndTurn(string sessionId, string playerId)
    {
        var session = _gameState.GetGameState(sessionId);
        if (session is null) return false;

        if (!_gameRound.EndTurn(session, playerId)) return false;

        if (_gameRound.TryResolveRound(session))
        {
            await Clients.Group(sessionId)
                .SendAsync("RoundResolved");

            if (session.GameOver)
            {
                await Clients.Group(sessionId)
                    .SendAsync("GameOver", session.WinnerPlayerId);
            }
        }
        else
        {
            await Clients.Group(sessionId)
                .SendAsync("TurnEnded", playerId);
        }

        return true;
    }
    public async Task<bool> SubmitTurnActions(string sessionId, string playerId, TurnActionsDto actions)
    {
        var session = _gameState.GetGameState(sessionId);
        if (session is null) return false;

        foreach (var cityId in actions.InvestCityIds) _gameRound.RequestInvest(session, playerId, cityId);
        foreach (var cityId in actions.ShieldCityIds) _gameRound.RequestShield(session, playerId, cityId);
        if (actions.NukeTech) _gameRound.RequestNukeTech(session, playerId);
        if (actions.NukesToBuild > 0) _gameRound.RequestBuildNukes(session, playerId, actions.NukesToBuild);
        if (actions.EcologyInvest) _gameRound.RequestEcologyInvest(session, playerId);
        foreach (var onId in actions.SanctionTogglePlayerIds) _gameRound.RequestSanctionToggle(session, playerId, onId);
        foreach (var strikeDto in actions.NukeStrikes) _gameRound.RequestNukeStrike(session, playerId, strikeDto.DefenderPlayerId, strikeDto.CityId);

        return await EndTurn(sessionId, playerId);
    }
    
    public GameStateBundle? GetMyGameState(string sessionId, string playerId)
    {
        var session = _gameState.GetGameState(sessionId);
        if (session is null) return null;

        return new GameStateBundle
        {
            View = BuildRoundResolvedDto(session, playerId),
            Controls = BuildControlPanelDto(session, playerId)
        };
    }
}