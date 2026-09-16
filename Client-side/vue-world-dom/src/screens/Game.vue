<script setup>
import { ref, onMounted, onUnmounted, reactive } from 'vue'
import { connection } from '@/server/connection'
import { SessionState } from '@/states/session'
import { Player } from '@/states/player'
import {
  onGameStarted, offGameStarted,
  onRoundResolved, offRoundResolved,
  onTurnEnded, offTurnEnded,
  onGameOver, offGameOver,
  endTurn, getMyGameState, submitTurnAction
} from '@/server/RequstHandlers'
import MyCountryPanel from '@/components/MyCountryPanel.vue'
import EndTurnButton from '@/components/EndTurnButton.vue'
 
const view = ref(null)      // RoundResolvedDto
const controls = ref(null)  // ControlPanelDto
const waitingOnPlayerIds = reactive(new Set())
const gameOverWinnerId = ref(null)
 

const pending = reactive({
  investCityIds: new Set(),
  shieldCityIds: new Set(),
  nukeTech: false,
  nukesToBuild: 0,
  ecologyInvest: false,
  sanctionTogglePlayerIds: new Set(),
  nukeStrikes: []
})

function buildTurnActionDto() {
  return {
    investCityIds: [...pending.investCityIds],
    shieldCityIds: [...pending.shieldCityIds],
    nukeTech: pending.nukeTech,
    nukesToBuild: pending.nukesToBuild,
    ecologyInvest: pending.ecologyInvest,
    sanctionTogglePlayerIds: [...pending.sanctionTogglePlayerIds],
    nukeStrikes: [...pending.nukeStrikes]
  }
}

async function handleGameStarted() {
  const state = await getMyGameState(
    SessionState.sessionId,
    Player.id
  )

  if (state) {
    view.value = state.view
    controls.value = state.controls
  }
}
 
async function handleRoundResolved() {
  const state = await getMyGameState(
    SessionState.sessionId,
    Player.id
  )

  if (state) {
    view.value = state.view
    controls.value = state.controls
  }

  waitingOnPlayerIds.clear()

  pending.investCityIds.clear()
  pending.shieldCityIds.clear()
  pending.sanctionTogglePlayerIds.clear()
  pending.nukeTech = false
  pending.nukesToBuild = 0
  pending.ecologyInvest = false
  pending.nukeStrikes = []
}
 
function handleTurnEnded(playerId) {
  waitingOnPlayerIds.add(playerId)
}
 
function handleGameOver(winnerPlayerId) {
  gameOverWinnerId.value = winnerPlayerId
}
 
async function handleEndTurn() {
  if (controls.value.hasEndedTurn) {
    return
  }

  const action = buildTurnActionDto()

  try {
    await submitTurnAction(
      SessionState.sessionId,
      Player.id,
      action
    )

    controls.value.hasEndedTurn = true
  } catch (error) {
    console.error('SubmitTurnActions failed:', error)
  }
}

onMounted(async () => {
  if (!SessionState.sessionId || !Player.id) {
    window.location.hash = '/'
    return
  }

  const initial = await getMyGameState(SessionState.sessionId, Player.id)
  if (initial) {
    view.value = initial.view
    controls.value = initial.controls
  }
  onGameStarted(handleGameStarted)
  onRoundResolved(handleRoundResolved)
  onTurnEnded(handleTurnEnded)
  onGameOver(handleGameOver)
})
 
onUnmounted(() => {
  offGameStarted(handleGameStarted)
  offRoundResolved(handleRoundResolved)
  offTurnEnded(handleTurnEnded)
  offGameOver(handleGameOver)
})
</script>
 
<template>
  <div v-if="!view" class="loading">
    Waiting for game to start...
  </div>
 
  <div v-else-if="gameOverWinnerId !== null || view.gameOver" class="game-over">
    <h1>Game Over</h1>
    <p v-if="gameOverWinnerId">Winner: {{ gameOverWinnerId }}</p>
    <p v-else>No winner.</p>
  </div>
 
  <div v-else class="game-screen">
    <h1>Round {{ view.round }} / {{ view.maxRounds }}</h1>
 
    <MyCountryPanel 
    :country="view.me" 
    :controls="controls" 
    :ecology-history="view.ecologyHistory" 
    :pending="pending"/>
 
    <EndTurnButton
      :has-ended-turn="controls.hasEndedTurn"
      @end-turn="handleEndTurn"
    />
  </div>
</template>
 
<style scoped>
.loading,
.game-over {
  text-align: center;
  margin-top: 40px;
  font-size: 1.2em;
}
</style>
 
