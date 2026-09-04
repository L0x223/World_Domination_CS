import { connection } from './connection'

export async function isNickAvailable(nickname: string) {
    const isAvailable = await connection.invoke<boolean>("CheckNickAvailability", nickname)
    //console.log("Available:", isAvailable)
    return isAvailable
}

export async function addPlayerNickToDb(nickname: string): Promise<boolean> {
    const statusCode = await connection.invoke<number>("AddPlayerToDatabase", nickname)
    return statusCode === 201
}