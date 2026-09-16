<script setup>
import ChooseCountryInLobbyModal from '@/components/ChooseCountryInLobbyModal.vue'
import PlayerInLobbyContainer from '@/components/PlayerInLobbyContainer.vue'
import { ref, onMounted, onUnmounted } from 'vue'
import { onGameStarted, offGameStarted, setReady, onPlayersUpdated, getPlayersInLobby, rejoinSession, getAvailableCountries, selectCountry, getJoinCodeById } from '@/server/RequstHandlers'
import { connection } from '@/server/connection'
import { LobbyState } from '@/states/lobby'
import { SessionState } from '@/states/session'
import { Player } from '@/states/player'

const ChooseCountryInLobbyShow = ref(true)
const countries = ref([])
const players = ref([])
const joinCode = ref()
const isReady = ref(false)

function handleGameStarted() {
  window.location.hash = '/game'
}

async function toggleReady() {
  const next = !isReady.value
  const success = await setReady(SessionState.sessionId, Player.id, next)
  if (success) isReady.value = next
}

async function handleChooseCountryInLobby(country) {
  if (country !== "") {
    const success = await selectCountry(SessionState.sessionId, Player.id, country.id)
    if (success) {
      ChooseCountryInLobbyShow.value = false
      LobbyState.selectedCountry = country
    } else {
      countries.value = await getAvailableCountries(SessionState.sessionId)
    }
  }
}

function handlePlayersUpdated(updatedPlayers) {
  players.value = updatedPlayers
  getAvailableCountries(SessionState.sessionId).then(c => countries.value = c)
}

onMounted(async () => {
  if (!SessionState.sessionId || !Player.id) {
    window.location.hash = '/'
    return
  }

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

  if (SessionState.justJoined) {
    SessionState.justJoined = false
  } else {
    const rejoin = await rejoinSession(SessionState.sessionId, Player.id)
    if (!rejoin.success) {
      console.error('Failed to rejoin session:', rejoin.error)
      SessionState.sessionId = null
      window.location.hash = '/'
      return
    }
  }

  onPlayersUpdated(handlePlayersUpdated)
  onGameStarted(handleGameStarted)

  joinCode.value = await getJoinCodeById(SessionState.sessionId)
  countries.value = await getAvailableCountries(SessionState.sessionId)
  players.value = await getPlayersInLobby(SessionState.sessionId)

  const me = players.value.find(p => p.nickname === Player.registeredNick)
  if (me?.countryId) {
    LobbyState.selectedCountry = { id: me.countryId, name: me.countryName, leaderIconId: me.countryLeaderIconId }
    ChooseCountryInLobbyShow.value = false
  } else {
    ChooseCountryInLobbyShow.value = true
  }

  isReady.value = me?.isReady ?? false
})

onUnmounted(() => {
  offGameStarted(handleGameStarted)
})
</script>

<template>
  <h1></h1>
  <PlayerInLobbyContainer :players="players" :joinCode="joinCode" />
  <ChooseCountryInLobbyModal :show="ChooseCountryInLobbyShow"
    :countries="countries"
    @continue="handleChooseCountryInLobby"></ChooseCountryInLobbyModal>
  <button @click="toggleReady" :disabled="!LobbyState.selectedCountry">
    {{ isReady ? 'Not ready' : 'Ready' }}
  </button>
</template>