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
  rejoinSession, getMyGameState, submitTurnAction
} from '@/server/RequstHandlers'
import MyCountryPanel from '@/components/MyCountryPanel.vue'
import EndTurnButton from '@/components/EndTurnButton.vue'
import OpponentActionsPanel from '@/components/OpponentActionsPanel.vue'
 
const view = ref(null)      // RoundResolvedDto
const controls = ref(null)  // ControlPanelDto
const waitingOnPlayerIds = reactive(new Set())
const gameOverWinner = ref(null)
 

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
 
function handleGameOver(gameOver) {
  gameOverWinner.value = gameOver
}

async function handleEndTurn() {
  if (controls.value.hasEndedTurn) {
    return
  }

  const action = buildTurnActionDto()
  
  console.log('Submitting turn actions:', action)

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

function handleToggleSanction(playerId) {
  if (pending.sanctionTogglePlayerIds.has(playerId)) {
    pending.sanctionTogglePlayerIds.delete(playerId)
  } else {
    pending.sanctionTogglePlayerIds.add(playerId)
  }
}

function handleToggleStrike({ defenderPlayerId, cityId }) {
  const idx = pending.nukeStrikes.findIndex(
    s => s.defenderPlayerId === defenderPlayerId && s.cityId === cityId
  )
  if (idx >= 0) {
    pending.nukeStrikes.splice(idx, 1)
  } else {
    pending.nukeStrikes.push({ defenderPlayerId, cityId })
  }
}

onMounted(async () => {

    if (connection.state !== 'Connected') {
    await new Promise(resolve => {
      const check = setInterval(() => {
        if (connection.state === 'Connected') {
          clearInterval(check)
          resolve()
        }
      }, 50)
    })
  }
  const result = await rejoinSession(
      SessionState.sessionId,
      Player.id
    )

    if (!result.success) {
      console.error('Failed to rejoin session:', result.error)
      window.location.hash = '/'
      return
    }

  if (!SessionState.sessionId || !Player.id) {
    window.location.hash = '/'
    return
  }

  onGameStarted(handleGameStarted)
  onRoundResolved(handleRoundResolved)
  onTurnEnded(handleTurnEnded)
  onGameOver(handleGameOver)

  const initial = await getMyGameState(SessionState.sessionId, Player.id)
  if (initial) {
    view.value = initial.view
    controls.value = initial.controls
  }
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
  
    <div v-else-if="gameOverWinner !== null || view.gameOver" class="game-over">
      <h1>Game Over</h1>
      <template v-if="gameOverWinner">
      <h2>Winner: {{ gameOverWinner.winnerNickname }}({{ gameOverWinner.winnerNickname }})</h2>
      <p>Wealth: {{ gameOverWinner.winnerWealth }}</p>
  </template>

  <p v-else>
    No winner.
  </p>
  </div>
 
  <div v-else class="game-screen">
    <h1>Round {{ view.round }} / {{ view.maxRounds }}</h1>
    <div class="game-layout">
      <div class="main-column">
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
  <OpponentActionsPanel 
  :controls="controls"
  :opponents="controls.foreignCountries" 
  :pending-sanctions="pending.sanctionTogglePlayerIds" 
  :pending-strikes="pending.nukeStrikes" 
  @toggle-sanction="handleToggleSanction" 
  @toggle-strike="handleToggleStrike" />
 </div>

  </div>
</template>
 
<style scoped>
.loading,
.game-over {
  text-align: center;
  margin-top: 40px;
  font-size: 1.2em;
}
.game-layout { 
  display: grid;
   grid-template-columns: minmax(0, 1fr) 320px; 
   gap: 16px; 
   align-items: start; } 
.main-column { min-width: 0; }
</style>
 
