<script setup>
import { computed } from 'vue'

const props = defineProps(
    {
      nickName: String,
      countryName: String,
      countryLeaderIconId: String,
      isSelected: Boolean,
    }
)

const icons = import.meta.glob('@/assets/img/*.png', { eager: true, import: 'default' })

const iconSrc = computed(() => {
  const filename = props.countryLeaderIconId ?? 'NoneIcon.png'
  const match = Object.entries(icons).find(([path]) => path.endsWith(filename))
  return match ? match[1] : icons['/src/assets/img/NoneIcon.png']
})

console.log('countryLeaderIconId prop:', props.countryLeaderIconId)
console.log('icons object:', icons)

</script>

<template>
  <div class="player-lobby">
    <img class="avatar" :src="iconSrc" alt="avatar" width="48" height="48"/>

    <div class="info">
      <p class="nick">{{ props.nickName }}</p>
      <p class="country" :class="{ selectedCountry: props.isSelected }">{{ props.countryName }}</p>
    </div>
  </div>
</template>

<style scoped>
.selectedCountry {
  color: black;
  font-weight: bold;
}
</style>