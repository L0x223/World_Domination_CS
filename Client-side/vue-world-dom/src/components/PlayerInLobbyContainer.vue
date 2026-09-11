<script setup> 
import PlayerInLobbyElement from './PlayerInLobbyElement.vue';
import {LobbyState} from '@/states/lobby'
const props = defineProps({
    players: Array,
    joinCode: String
}
)
</script>

<template>
    <div class="player-in-lobby-container">
    <h1>Players in lobby: {{ props.players.length }}</h1>
    <p>Join code: {{ props.joinCode }}</p>
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