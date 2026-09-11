import { connection } from './connection'
import type { Country } from '../objects/country.ts'
import type { PlayerInterface } from '@/objects/player'

export async function isNickAvailable(nickname: string) {
    const isAvailable = await connection.invoke<boolean>("CheckNickAvailability", nickname)
    //console.log("Available:", isAvailable)
    return isAvailable
}

export async function addPlayerNickToDb(nickname: string): Promise<{ success: boolean; playerId?: string; reason?: string }> {
  return await connection.invoke("AddPlayerToDatabase", nickname)
}

export async function checkSessionNameAvailability(sessionName: string): Promise<boolean> {

    const isAvailable = await connection.invoke<boolean>("CheckSessionNameAvailability", sessionName)
    return isAvailable
}

export async function createSession(sessionName: string) {
  return await connection.invoke<{ success: boolean; joinCode?: string; reason?: string }>(
    "CreateSession",
    sessionName
  )
}

export async function getAllCountries(): Promise<Country[]> {
    const countries = await connection.invoke<Country[]>("GetAllCountries")
    return countries
}

export async function getPlayersInLobby(sessionId: string): Promise<PlayerInterface[]> {

    const players = await connection.invoke<PlayerInterface[]>("GetPlayersInLobby", sessionId)
    return players
}

export async function joinSessionByCode(sessionCode: string, nickname: string): Promise<{ success: boolean; sessionId?: string; reason?: string }> {
  return await connection.invoke<{ success: boolean; sessionId?: string; reason?: string }>(
    "JoinSessionByCode",
    sessionCode,
    nickname
  )
}

export async function rejoinSession(sessionId: string, playerId: string) {
  return await connection.invoke<{ success: boolean; sessionId?: string; reason?: string }>(
    "RejoinSession", sessionId, playerId
  )
}

export async function getAvailableCountries(sessionId: string) {
  return await connection.invoke<Country[]>("GetAvailableCountries", sessionId)
}

export async function selectCountry(sessionId: string, playerId: string, countryId: string): Promise<boolean> {
  return await connection.invoke<boolean>("SelectCountry", sessionId, playerId, countryId)
}

export function onPlayersUpdated(callback: (players: PlayerInterface[]) => void) {
  connection.on("PlayersUpdated", callback)
}

export function offPlayersUpdated(callback: (players: PlayerInterface[]) => void) {
  connection.off("PlayersUpdated", callback)
}