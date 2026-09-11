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
    <h1>Players in lobby: {{ props.players.length }}</h1>
    <div>
        <PlayerInLobbyElement
            v-for="p in props.players"
            :key="p.connectionId"
            :nickName="p.nickname"
            :countryName="p.countryName"
            :countryLeaderIconId="p.countryLeaderIconId"
            :isSelected="!!p.countryId"
            />
    </div>
    </div>
</template>