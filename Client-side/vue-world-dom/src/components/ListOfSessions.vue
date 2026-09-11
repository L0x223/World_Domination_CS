<script setup>
const props = defineProps({
  sessions: Array
})

const emit = defineEmits(['select'])
</script>

<template>
  <div class="list-of-sessions">
    <h2>Available Sessions</h2>
    <p v-if="props.sessions.length === 0">No sessions found.</p>
    <ul v-else>
      <li
        v-for="session in props.sessions"
        :key="session.sessionName"
        @click="emit('select', session)"
        class="session-item"
      >
        {{session.sessionName}} - Players: {{ session.playerCount }}/{{ session.maxPlayers }}
      </li>
    </ul>
  </div>
</template>

<style scoped>
.session-item {
  cursor: pointer;
  padding: 4px 0;
}
.session-item:hover {
  text-decoration: underline;
}
</style>