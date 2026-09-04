export interface City {
  id: string
  name: string
}

export interface Country {
  id: string
  name: string
  leaderIconId: string
  cities: City[]
}