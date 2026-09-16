<script setup>
const props = defineProps({
  hasEndedTurn: Boolean
})

const emit = defineEmits(['endTurn'])

function handleEndTurn() {
  emit('endTurn')
}

</script>

<template>
  <div class="end-turn-container">
    <button
      class="end-turn-btn"
      :disabled="props.hasEndedTurn"
      @click="handleEndTurn"
    >
      {{ props.hasEndedTurn ? 'Waiting for other players...' : 'End Turn' }}
    </button>
  </div>
</template>

<style scoped>
.end-turn-container {
  margin-top: 16px;
  text-align: center;
}

.end-turn-btn {
  padding: 10px 24px;
  font-size: 1em;
  font-weight: bold;
  border-radius: 6px;
  border: 2px solid #000;
  background: #ffffff;
  cursor: pointer;
}

.end-turn-btn:disabled {
  background: #eeeeee;
  color: #888888;
  cursor: not-allowed;
}
</style>