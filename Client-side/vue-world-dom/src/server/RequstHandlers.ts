import { connection, connectionReady } from './connection'
import type { Country } from '../objects/country.ts'
import type { PlayerDto } from '@/objects/player'
import type { SessionDto } from '@/objects/SessionDto'
import type { RoundResolvedDto, ControlPanelDto, SubmitTurnActionDto } from '@/objects/Dtos'

export async function isNickAvailable(nickname: string) {
    const isAvailable = await connection.invoke<boolean>("CheckNickAvailability", nickname)
    //console.log("Available:", isAvailable)
    return isAvailable
}

export async function addPlayerNickToDb(nickname: string): Promise<{ success: boolean; playerId?: string; error?: string }> {
  return await connection.invoke("AddPlayerToDatabase", nickname)
}

export async function checkSessionNameAvailability(sessionName: string): Promise<boolean> {

    const isAvailable = await connection.invoke<boolean>("CheckSessionNameAvailability", sessionName)
    return isAvailable
}

export async function createSession(sessionName: string) {
  return await connection.invoke<{ success: boolean; joinCode?: string; error?: string }>(
    "CreateSession",
    sessionName
  )
}

export async function getAllCountries(): Promise<Country[]> {
    const countries = await connection.invoke<Country[]>("GetAllCountries")
    return countries
}

export async function getPlayersInLobby(sessionId: string): Promise<PlayerDto[]> {

    const players = await connection.invoke<PlayerDto[]>("GetPlayersInLobby", sessionId)
    return players
}

export async function joinSessionByCode(sessionCode: string, nickname: string): Promise<{ success: boolean; sessionId?: string; error?: string }> {
  return await connection.invoke<{ success: boolean; sessionId?: string; error?: string }>(
    "JoinSessionByCode",
    sessionCode,
    nickname
  )
}

export async function rejoinSession(sessionId: string, playerId: string) {
  return await connection.invoke<{ success: boolean; sessionId?: string; error?: string }>(
    "RejoinSession", sessionId, playerId
  )
}

export async function getAvailableCountries(sessionId: string) {
  return await connection.invoke<Country[]>("GetAvailableCountries", sessionId)
}

export async function selectCountry(sessionId: string, playerId: string, countryId: string): Promise<boolean> {
  return await connection.invoke<boolean>("SelectCountry", sessionId, playerId, countryId)
}

export function onPlayersUpdated(callback: (players: PlayerDto[]) => void) {
  connection.on("PlayersUpdated", callback)
}

export function offPlayersUpdated(callback: (players: PlayerDto[]) => void) {
  connection.off("PlayersUpdated", callback)
}

export async function getSessions(filter: string = '', excludeFullSessions: boolean = false): Promise<SessionDto[]> {
  await connectionReady
  return await connection.invoke<SessionDto[]>("GetSessions", filter, excludeFullSessions)
}

export async function joinSessionById(sessionId: string, nickname: string): Promise<{ success: boolean; sessionId?: string; error?: string }> {
  return await connection.invoke("JoinSessionById", sessionId, nickname)
}

export async function getJoinCodeById(sessionId: string): Promise<string | null> {
  return await connection.invoke<string | null>("GetJoinCodeById", sessionId)
}

export async function setReady(sessionId: string, playerId: string, isReady: boolean): Promise<boolean> {
  return await connection.invoke<boolean>("SetReady", sessionId, playerId, isReady)
}

 
export async function requestInvest(sessionId: string, playerId: string, cityId: string): Promise<boolean> {
  return await connection.invoke<boolean>("RequestInvest", sessionId, playerId, cityId)
}
 
export async function requestShield(sessionId: string, playerId: string, cityId: string): Promise<boolean> {
  return await connection.invoke<boolean>("RequestShield", sessionId, playerId, cityId)
}
 
export async function requestNukeTech(sessionId: string, playerId: string): Promise<boolean> {
  return await connection.invoke<boolean>("RequestNukeTech", sessionId, playerId)
}
 
export async function requestBuildNukes(sessionId: string, playerId: string, count: number): Promise<boolean> {
  return await connection.invoke<boolean>("RequestBuildNukes", sessionId, playerId, count)
}
 
export async function requestEcologyInvest(sessionId: string, playerId: string): Promise<boolean> {
  return await connection.invoke<boolean>("RequestEcologyInvest", sessionId, playerId)
}
 
export async function endTurn(sessionId: string, playerId: string): Promise<boolean> {
  return await connection.invoke<boolean>("EndTurn", sessionId, playerId)
}
 
 
export function onGameStarted(callback: () => void) {
  connection.on("GameStarted", callback)
}

export function offGameStarted(callback: () => void) {
  connection.off("GameStarted", callback)
}

export function onRoundResolved(
  callback: () => void
) {
  connection.on("RoundResolved", callback)
}
 
export function offRoundResolved(
  callback: () => void
) {
  connection.off("RoundResolved", callback)
}
 
export function onTurnEnded(callback: (playerId: string) => void) {
  connection.on("TurnEnded", callback)
}
 
export function offTurnEnded(callback: (playerId: string) => void) {
  connection.off("TurnEnded", callback)
}
 
export function onGameOver(callback: (winnerPlayerId: string | null) => void) {
  connection.on("GameOver", callback)
}
 
export function offGameOver(callback: (winnerPlayerId: string | null) => void) {
  connection.off("GameOver", callback)
}

export async function getMyGameState(
  sessionId: string,
  playerId: string
): Promise<{ view: RoundResolvedDto; controls: ControlPanelDto } | null> {
  await connectionReady
  return await connection.invoke("GetMyGameState", sessionId, playerId)
}

export async function submitTurnAction(sessionId: string, playerId: string, pending: SubmitTurnActionDto)
{
  return connection.invoke('SubmitTurnActions', sessionId, playerId, pending)
}