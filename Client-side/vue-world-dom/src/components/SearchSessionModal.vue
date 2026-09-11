<script setup>
import { ref, onMounted, watch } from 'vue'
import ListOfSessions from '@/components/ListOfSessions.vue'
import { getSessions, joinSessionByCode, joinSessionById } from '@/server/RequstHandlers'
import { Player } from '@/states/player'
import { SessionState } from '@/states/session'

const props = defineProps({
  show: Boolean,
})
const emit = defineEmits(['close', 'joinSession'])

const sessionCode = ref('')
const sessions = ref([])
const errorMessage = ref('')
const excludeFull = ref(false)
const nameFilter = ref('')

async function refreshSessions() {
  sessions.value = await getSessions(nameFilter.value, excludeFull.value)
}

async function handleSelectSession(session) {
  const result = await joinSessionById(session.sessionName, Player.nick)
  if (result.success) {
    SessionState.sessionId = result.sessionId
    SessionState.justJoined = true
    emit('joinSession', result.sessionId)
  } else {
    errorMessage.value = result.reason ?? 'Failed to join session'
  }
}

async function handleJoin() {
  errorMessage.value = ''
  const code = sessionCode.value.trim()
  if (code === '') return

  const result = await joinSessionByCode(code, Player.nick)
  if (result.success) {
    SessionState.sessionId = result.sessionId
    SessionState.joinCode = code
    SessionState.justJoined = true
    emit('joinSession', result.sessionId)
  } else {
    errorMessage.value = result.reason ?? 'Failed to join session'
  }
}

watch([nameFilter, excludeFull], refreshSessions)

onMounted(refreshSessions)
</script>

<template>
  <div v-if="props.show" class="modal">
    <div class="modal-content">
      <h2>Search Session</h2>
      <p>Enter the session code to join:</p>
      <input v-model="sessionCode" type="text" placeholder="Session Code" @keyup.enter="handleJoin" />
      <button @click="handleJoin">Join Session</button>
      <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

      <div class="filters">
        <input v-model="nameFilter" type="text" placeholder="Filter by name" />
        <label>
          <input v-model="excludeFull" type="checkbox" />
          Hide full sessions
        </label>
      </div>

      <ListOfSessions :sessions="sessions" @select="handleSelectSession" />
      <button @click="emit('close')">Close</button>
    </div>
  </div>
</template>

<style scoped>
.modal {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  justify-content: center;
  align-items: center;
}
.modal-content {
  background: white;
  padding: 20px;
  border-radius: 8px;
}
.error {
  color: red;
  font-weight: bold;
  font-size: 0.8em;
}
.filters {
  margin: 12px 0;
  display: flex;
  gap: 8px;
  align-items: center;
}
</style>