import { reactive } from 'vue'

export interface Player {
  id: string
  name: string,
  countryName: string,
}

export const LobbyState = reactive({
  players: [] as Player[],
  selectedCountry: null as string | null,
})