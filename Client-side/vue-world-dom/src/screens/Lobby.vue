<script setup>
import ChooseCountryInLobbyModal from '@/components/ChooseCountryInLobbyModal.vue'
import PlayerInLobbyContainer from '@/components/PlayerInLobbyContainer.vue'
import { ref, onMounted } from 'vue'
import { getAllCountries, getPlayersInLobby } from '@/server/RequstHandlers'
import { connection } from '@/server/connection'
import { LobbyState } from '@/states/lobby'
import {SessionState} from '@/states/session'

const ChooseCountryInLobbyShow = ref(false)
const countries = ref([])
const players = ref([])

  function handleChooseCountryInLobby(country) {
  if (country !== "") {
    ChooseCountryInLobbyShow.value = false
    LobbyState.selectedCountry = country
    // send country.id to the server here, e.g.:
    // await selectCountry(SessionState.sessionId, country.id)
  }
}
  function ChooseCountryInLobbyShowHandler(){
      //TODO IF ALREADY HAS COUNTRY THEN FALSE
      ChooseCountryInLobbyShow.value = true
  }
    ChooseCountryInLobbyShowHandler()

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

  countries.value = await getAllCountries()
  players.value = await getPlayersInLobby(SessionState.sessionId)
  ChooseCountryInLobbyShowHandler()
})


</script>
<template>
  <h1></h1>
    <PlayerInLobbyContainer :players = "players"/>
    <ChooseCountryInLobbyModal :show="ChooseCountryInLobbyShow"
      :countries="countries"
      @continue="handleChooseCountryInLobby"></ChooseCountryInLobbyModal>
</template>