<script setup>

const props = defineProps(
    {
        avatar: String,
        nickName: String,
        countryName: String,
        isSelected: Boolean,
        isHost: Boolean,
    }
)

</script>

<template>
  <div class="player-lobby">
    <img class="avatar" :src="props.avatar" alt="avatar" width="48" height="48"/>

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