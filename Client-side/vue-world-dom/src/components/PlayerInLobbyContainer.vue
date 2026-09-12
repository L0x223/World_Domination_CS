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
    <div class="payer-in-llobby-container">
        <h1>Players in lobby: {{ props.players.length }}</h1>
        <p>Join code: {{ props.joinCode }}</p>
        <div class="player-list">
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

<style scoped>
.player-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
</style>