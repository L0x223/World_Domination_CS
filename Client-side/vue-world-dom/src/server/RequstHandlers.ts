import { connection } from './connection'
import type { Country } from '../objects/country'
import type { Player } from '@/objects/player'

export async function isNickAvailable(nickname: string) {
    const isAvailable = await connection.invoke<boolean>("CheckNickAvailability", nickname)
    //console.log("Available:", isAvailable)
    return isAvailable
}

export async function addPlayerNickToDb(nickname: string): Promise<boolean> {
    const statusCode = await connection.invoke<number>("AddPlayerToDatabase", nickname)
    return statusCode === 201
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

export async function getPlayersInLobby(sessionId: string): Promise<Player[]> {

    const players = await connection.invoke<Player[]>("GetPlayersInLobby", sessionId)
    return players
}

export async function joinSessionByCode(sessionCode: string, nickname: string): Promise<{ success: boolean; sessionId?: string; reason?: string }> {
  return await connection.invoke<{ success: boolean; sessionId?: string; reason?: string }>(
    "JoinSessionByCode",
    sessionCode,
    nickname
  )
}