import { reactive, watch } from 'vue'

const STORAGE_KEY = 'wd-player-state'

function loadInitial() {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : { id: null, nick: '' }
  } catch {
    return { id: null, nick: '' }
  }
}

export const Player = reactive(loadInitial())

watch(Player, (value) => {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(value))
}, { deep: true })