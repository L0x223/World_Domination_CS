<script setup>
import { defineProps, defineEmits, ref } from 'vue'

const selected = ref('')
const props = defineProps({
  show: Boolean,
  countries: Array,
})

const emit = defineEmits(['continue'])
</script>


<template>
  <div v-if="props.show" class="modal">
    <div class="modal-content">
      <slot></slot>
        <p>choose your country:</p>
        <select v-model="selected">
            <option disabled value ="">Select...</option>
            <option 
            v-for="country in props.countries" 
            :key="country.id" 
            :value="country">
            {{ country.name }}
            </option>
        </select>
      <button @click="emit('continue', selected)">continue</button>
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
</style>
