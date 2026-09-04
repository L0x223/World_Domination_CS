<script setup>
/*import ChooseCountryInLobbyModal  from '@/components/ChooseCountryInLobbyModal.vue'
import PlayerInLobbyContainer from '@/components/PlayerInLobbyContainer.vue';
import { ref } from 'vue';
import {requestCountries, requestPlayersInLobby} from '@/server/requests'
const players = ref(requestPlayersInLobby())


const ChooseCountryInLobbyShow = ref(false)

  function handleChooseCountryInLobby(country) {

    if( country !== "")
    ChooseCountryInLobbyShow.value = false

    LobbyState.selectedCountry = country
    ws.send(buildSelectCountryMessage(country, Cookie.get('id')))//TODO Secure send
    }
    function ChooseCountryInLobbyShowHandler(){
        //TODO IF ALREADY HAS COUNTRY THEN FALSE
        ChooseCountryInLobbyShow.value = true
    }
    ChooseCountryInLobbyShowHandler()
*/

</script>
<template>
  <h1></h1>
    <PlayerInLobbyContainer :players = "players"/>
    <ChooseCountryInLobbyModal :show = "ChooseCountryInLobbyShow"
    :countries="requestCountries()"
    @continue="handleChooseCountryInLobby"></ChooseCountryInLobbyModal>
</template>