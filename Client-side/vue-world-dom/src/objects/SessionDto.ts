export interface SessionDto {
  sessionName: string  //for now this is basically Session ID, named this way in case of separation
  playerCount: number
  maxPlayers: number
  phase: number  
}