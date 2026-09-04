
import { ref, computed } from 'vue'

import StartScreen from '../screens/StartScreen.vue'
import Lobby from '../screens/Lobby.vue'
import Game from '../screens/Game.vue'
import NotFound from '../screens/NotFound.vue'

export const routes: Record<string, any> = {
  '/': StartScreen,
  '/lobby': Lobby,
  '/game': Game
}

const currentPath = ref(window.location.hash)

window.addEventListener('hashchange', () => {
  currentPath.value = window.location.hash
})

export const currentView = computed(() => {
  return routes[currentPath.value.slice(1) || '/'] || NotFound
})