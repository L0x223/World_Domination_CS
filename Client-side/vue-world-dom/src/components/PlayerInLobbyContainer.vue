<script setup> 
import PlayerInLobbyElement from './PlayerInLobbyElement.vue';
import {LobbyState} from '@/states/lobby'
const props = defineProps({
    players: Array,
}
)
</script>

<template>
    <div class="player-in-lobby-container">
    <h1>Players in lobby: {{ LobbyState.players.length }}</h1>
    <div>
        <PlayerInLobbyElement v-for ="p in props.players" 
        :key="p"
        :avatar="p.avatar"
        :nickName="p.nickName"
        :countryName="p.countryName"
        :isSelected="p.countryName === LobbyState.selectedCountry"
         />
    </div>
    </div>
</template>