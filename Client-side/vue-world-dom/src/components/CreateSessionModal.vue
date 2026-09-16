<script setup>
import { ref } from 'vue'
import { checkSessionNameAvailability, createSession, joinSessionByCode } from '@/server/RequstHandlers';

const sessionName = ref('')
const props = defineProps({
  show: Boolean,
  isTaken: Boolean,
  nickname: String
})

const emit = defineEmits(['close', 'createSession', 'update:isTaken'])


async function onSessionNameInput(e) {
  const name = e.target.value.trim()
  if (name === '') {
    emit('update:isTaken', false)
    return
  }
  const available = await checkSessionNameAvailability(name)
  if (sessionName.value.trim() !== name) return
  emit('update:isTaken', !available)
}

async function onCreateClick() {
  const createResult = await createSession(sessionName.value)
  if (!createResult.success) {
    emit('update:isTaken', createResult.error === 'NameTaken')
    return
  }

  const joinResult = await joinSessionByCode(createResult.joinCode, props.nickname)
  if (joinResult.success) {
    emit('createSession', {
      sessionId: joinResult.sessionId,
      joinCode: createResult.joinCode,
      sessionName: sessionName.value
    })
  } else {
    console.error('Created session but failed to join it:', joinResult.error)
  }
}
</script>

<template>
  <div v-if="props.show" class="modal">
    <div class="modal-content">
      <slot></slot>
        <p>Session name: <input v-model="sessionName" @input="onSessionNameInput" placeholder="enter here" /></p>
        <p v-if="props.isTaken" class="error">This session name is already taken</p>  
      <button @click="emit('close')">Close</button>
      <button @click="onCreateClick" :disabled="props.isTaken">Create</button>
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
  margin-left: 108px;
}

</style>
