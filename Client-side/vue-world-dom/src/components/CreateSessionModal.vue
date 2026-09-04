<script setup>
import { defineProps, defineEmits, ref } from 'vue'

const sessionName = ref('')
const props = defineProps({
  show: Boolean
})

const emit = defineEmits(['close', 'createSession'])
</script>

<template>
  <div v-if="props.show" class="modal">
    <div class="modal-content">
      <slot></slot>
        <p>Session name: <input v-model="sessionName" placeholder="enter here" /></p>
      <button @click="emit('close')">Close</button>
      <button @click="emit('createSession', sessionName.value)">Create</button>
    </div>
  </div>
</template>


//TODO use style
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
</style>
