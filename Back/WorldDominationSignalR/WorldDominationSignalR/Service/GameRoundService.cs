using WorldDominationSignalR.States;

namespace WorldDominationSignalR.Service;

public class GameRoundService : IGameRoundService
{
    private readonly ICountryDataService _countryDataService;

    public GameRoundService(ICountryDataService countryDataService)
    {
        _countryDataService = countryDataService;
    }
    
    public void StartGame(SessionGameState session)
    {
        lock (session)
        {
            foreach (var (playerId, countryId) in session.CountryByPlayerId)
            {
                var country = session.CountryStateByPlayerId.GetValueOrDefault(playerId)
                              ?? new CountryGameState { PlayerId = playerId, CountryId = countryId };

                if (country.Cities.Count == 0)
                {
                    var countryData = _countryDataService.GetAll().First(c => c.Id == countryId);
                    foreach (var cityData in countryData.Cities)
                    {
                        country.Cities.Add(new CityState
                        {
                            CityId = cityData.Id,                     
                            Economic = cityData.StartingEconomic
                        });
                    }
                }

                session.CountryStateByPlayerId[playerId] = country;
            }

            session.Phase = GamePhase.InProgress;
            session.Round = 1;
            session.Ecology = 0.8;
        }
    }

    private CountryGameState? GetAliveCountry(SessionGameState session, string playerId)
    {
        return session.CountryStateByPlayerId.TryGetValue(playerId, out var c) && c.IsAlive ? c : null;
    }

    public bool RequestInvest(SessionGameState session, string playerId, string cityId)
    {
        lock (session)
        {
            var country = GetAliveCountry(session, playerId);
            var city = country?.Cities.FirstOrDefault(c => c.CityId == cityId && c.IsAlive);
            if (city is null) return false;

            city.InvestRequested = true;
            return true;
        }
    }

    public bool RequestShield(SessionGameState session, string playerId, string cityId)
    {
        lock (session)
        {
            var country = GetAliveCountry(session, playerId);
            var city = country?.Cities.FirstOrDefault(c => c.CityId == cityId && c.IsAlive);
            if (city is null) return false;

            city.ShieldRequested = true;
            return true;
        }
    }

    public bool RequestNukeTech(SessionGameState session, string playerId)
    {
        lock (session)
        {
            var country = GetAliveCountry(session, playerId);
            if (country is null) return false;

            country.NukeTechRequested = true;
            return true;
        }
    }

    public bool RequestBuildNukes(SessionGameState session, string playerId, int count)
    {
        if (count <= 0) return false;

        lock (session)
        {
            var country = GetAliveCountry(session, playerId);
            if (country is null) return false;

            country.NukesToBuildRequested += count;
            return true;
        }
    }

    public bool RequestEcologyInvest(SessionGameState session, string playerId)
    {
        lock (session)
        {
            var country = GetAliveCountry(session, playerId);
            if (country is null) return false;

            country.EcologyInvestRequested = true;
            return true;
        }
    }

    public bool RequestSanctionToggle(SessionGameState session, string byPlayerId, string onPlayerId)
    {
        lock (session)
        {
            var byCountry = GetAliveCountry(session, byPlayerId);
            var onCountry = GetAliveCountry(session, onPlayerId);
            if (byCountry is null || onCountry is null || byPlayerId == onPlayerId) return false;

            // toggle: if already staged, un-stage it (cancels the pending change)
            if (!onCountry.PendingSanctionToggles.Remove(byPlayerId))
                onCountry.PendingSanctionToggles.Add(byPlayerId);

            return true;
        }
    }

    public bool RequestNukeStrike(SessionGameState session, string attackerPlayerId, string defenderPlayerId, string cityId)
    {
        lock (session)
        {
            var attacker = GetAliveCountry(session, attackerPlayerId);
            var defender = GetAliveCountry(session, defenderPlayerId);
            if (attacker is null || defender is null) return false;
            if (attackerPlayerId == defenderPlayerId) return false;

            var city = defender.Cities.FirstOrDefault(c => c.CityId == cityId && c.IsAlive);
            if (city is null) return false;

            // Count nukes already committed to pending strikes this round so you can't overcommit
            int alreadyCommitted = session.PendingNukeStrikes.Count(s => s.AttackerPlayerId == attackerPlayerId);
            if (attacker.NukeCount <= alreadyCommitted) return false;

            session.PendingNukeStrikes.Add(new PendingNukeStrike
            {
                AttackerPlayerId = attackerPlayerId,
                DefenderPlayerId = defenderPlayerId,
                CityId = cityId
            });
            return true;
        }
    }

    public bool EndTurn(SessionGameState session, string playerId)
    {
        lock (session)
        {
            var country = GetAliveCountry(session, playerId);
            if (country is null) return false;

            country.EndedTurn = true;
            return true;
        }
    }

    public bool TryResolveRound(SessionGameState session)
    {
        lock (session)
        {
            if (session.GameOver) return false;

            var aliveCountries = session.CountryStateByPlayerId.Values.Where(c => c.IsAlive).ToList();
            if (aliveCountries.Count == 0 || !aliveCountries.All(c => c.EndedTurn))
                return false;

            ResolveNukeStrikes(session);
            ResolveEconomyAndActions(session, aliveCountries);
            ResolveSanctions(aliveCountries);
            
            session.EcologyHistory.Add(session.Ecology);
            session.PendingNukeStrikes.Clear();
            foreach (var c in aliveCountries) c.EndedTurn = false;

            CheckEliminations(session);
            session.Round++;
            CheckGameEnd(session);

            return true;
        }
    }

    private void ResolveNukeStrikes(SessionGameState session)
    {
        foreach (var strike in session.PendingNukeStrikes)
        {
            var attacker = session.CountryStateByPlayerId.GetValueOrDefault(strike.AttackerPlayerId);
            var defender = session.CountryStateByPlayerId.GetValueOrDefault(strike.DefenderPlayerId);
            if (attacker is null || defender is null || attacker.NukeCount <= 0) continue;

            var city = defender.Cities.FirstOrDefault(c => c.CityId == strike.CityId);
            if (city is null || !city.IsAlive) continue;

            if (city.HasShield)
            {
                city.HasShield = false; // shield absorbs the hit
            }
            else
            {
                city.IsAlive = false;
                city.Economic = 0;
            }

            attacker.NukeCount--;
            session.Ecology = Math.Max(0, session.Ecology - 0.1);
        }
    }

    private void ResolveEconomyAndActions(SessionGameState session, List<CountryGameState> aliveCountries)
    {
        foreach (var country in aliveCountries)
        {
            // Nuclear tech toggle
            if (country.NukeTechRequested && country.Budget >= GameConstants.NukeTechCost)
            {
                country.Budget -= GameConstants.NukeTechCost;
                country.HasNuclearTech = !country.HasNuclearTech;
                session.Ecology = Math.Max(0, session.Ecology - 0.1);
            }
            country.NukeTechRequested = false;

            // Build nukes
            if (country.HasNuclearTech && country.NukesToBuildRequested > 0)
            {
                int affordable = Math.Min(country.NukesToBuildRequested, country.Budget / GameConstants.CostNuke);
                if (affordable > 0)
                {
                    country.Budget -= affordable * GameConstants.CostNuke;
                    country.NukeCount += affordable;
                    session.Ecology = Math.Max(0, session.Ecology - 0.05 * affordable);
                }
            }
            country.NukesToBuildRequested = 0;

            // Ecology investment
            if (country.EcologyInvestRequested && country.Budget >= GameConstants.CostEcology)
            {
                country.Budget -= GameConstants.CostEcology;
                session.Ecology = Math.Min(1, session.Ecology + 0.2);
            }
            country.EcologyInvestRequested = false;

            int sanctionCount = country.SanctionedBy.Count;

            // Shields — pay upkeep for all requested shields this round
            var shieldRequests = country.Cities.Where(c => c.IsAlive && c.ShieldRequested).ToList();
            if (shieldRequests.Count > 0 && country.Budget >= GameConstants.CostShield * shieldRequests.Count)
            {
                country.Budget -= GameConstants.CostShield * shieldRequests.Count;
                foreach (var city in shieldRequests) city.HasShield = true;
            }
            foreach (var city in country.Cities) city.ShieldRequested = false;

            // Investment
            var investRequests = country.Cities.Where(c => c.IsAlive && c.InvestRequested).ToList();
            if (investRequests.Count > 0 && country.Budget >= GameConstants.CostInvest * investRequests.Count)
            {
                country.Budget -= GameConstants.CostInvest * investRequests.Count;
                foreach (var city in investRequests)
                {
                    city.Economic = Math.Min(1, city.Economic + 0.2);
                }
            }
            foreach (var city in country.Cities) city.InvestRequested = false;

            // Income, reduced by sanctions
            double sanctionPenalty = 1 - Math.Min(1, 0.15 * sanctionCount);
            foreach (var city in country.Cities.Where(c => c.IsAlive))
            {
                country.Budget += (int)Math.Round(city.GetIncome() * city.Economic * sanctionPenalty);
            }
        }
    }

    private void ResolveSanctions(List<CountryGameState> aliveCountries)
    {
        foreach (var country in aliveCountries)
        {
            foreach (var byPlayerId in country.PendingSanctionToggles)
            {
                if (!country.SanctionedBy.Remove(byPlayerId))
                    country.SanctionedBy.Add(byPlayerId);
            }
            country.PendingSanctionToggles.Clear();
        }
    }

    private void CheckEliminations(SessionGameState session)
    {
        foreach (var country in session.CountryStateByPlayerId.Values.Where(c => c.IsAlive))
        {
            if (country.Cities.All(c => !c.IsAlive))
            {
                country.IsEliminated = true;
            }
        }
    }

    private void CheckGameEnd(SessionGameState session)
    {
        var aliveCountries = session.CountryStateByPlayerId.Values.Where(c => c.IsAlive).ToList();

        if (session.Ecology <= 0 || session.Round > SessionGameState.MaxRounds || aliveCountries.Count <= 1)
        {
            session.GameOver = true;
            session.Phase = GamePhase.Finished;

            if (aliveCountries.Count == 1)
            {
                session.WinnerPlayerId = aliveCountries[0].PlayerId;
            }
            else if (aliveCountries.Count > 1)
            {
                // highest budget wins on round-limit/ecology-collapse ties
                session.WinnerPlayerId = aliveCountries.OrderByDescending(c => c.Budget).First().PlayerId;
            }
        }
    }
}