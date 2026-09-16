export interface CityDto {
  cityId: string
  cityName: string
  isAlive: boolean
  hasShield: boolean | null   // null when hidden from viewer (enemy city)
  economic: number | null     // null when hidden from viewer (enemy city)
}
 
export interface MyCountryDto {
  countryId: string
  countryName: string
  budget: number
  hasNuclearTech: boolean
  nukeCount: number
  isEliminated: boolean
  sanctionedByPlayerIds: string[]   // players currently sanctioning me
  cities: CityDto[]
}
 
export interface OtherCountryDto {
  playerId: string
  playerName: string
  countryId: string
  countryName: string
  isEliminated: boolean
  cities: CityDto[]   // hasShield/economic will be null here
}
 
export interface RoundResolvedDto {
  round: number
  maxRounds: number
  ecologyHistory: number[]
  gameOver: boolean
  winnerPlayerId: string | null
  me: MyCountryDto
  others: OtherCountryDto[]
}
 
export interface CityActionDto {
  cityId: string
  cityName: string
  isAlive: boolean
  hasShield: boolean
  investRequested: boolean
  shieldRequested: boolean
}
 
export interface TargetableCityDto {
  cityId: string
  cityName: string
  canStrike: boolean
  isAlive: boolean
}
 
export interface TargetableCountryDto {
  playerId: string
  countryName: string
  isSanctioned: boolean            // currently active, imposed by me
  sanctionToggleRequested: boolean // staged this round
  cities: TargetableCityDto[]
}
 
export interface ControlPanelDto {
  maxRounds: number
  budget: number
 
  hasNuclearTech: boolean
  nukeTechCost: number
 
  nukeCount: number
  nukesCommittedThisRound: number
  costNuke: number
 
  costEcology: number
  costInvest: number
  costShield: number
 
  hasEndedTurn: boolean
 
  myCities: CityActionDto[]
  foreignCountries: TargetableCountryDto[]
}
 
// ----- Lobby / session DTOs -----
 
export type GamePhase = 'Lobby' | 'CountrySelection' | 'InProgress' | 'Finished'
 
export interface SessionDto {
  sessionName: string
  playerCount: number
  maxPlayers: number
  phase: GamePhase
}
 
export interface PlayerLobbyDto {
  connectionId: string
  nickname: string
  countryId: string | null
  countryName: string | null
  countryLeaderIconId: string | null
  isReady: boolean
}
 
export interface AddPlayerResult {
  success: boolean
  error?: string
  playerId?: string
}
 
export interface CreateSessionResult {
  success: boolean
  error?: string
  joinCode?: string
}
 
export interface JoinSessionResult {
  success: boolean
  error?: string
  sessionId?: string
}

export interface SubmitTurnActionDto {
  investCityIds: string[]
  shieldCityIds: string[]

  nukeTech: boolean
  nukesToBuild: number

  ecologyInvest: boolean

  sanctionTogglePlayerIds: string[]

  nukeStrikes: NukeStrikeDto[]
}

export interface NukeStrikeDto {
  defenderPlayerId: string
  cityId: string
}