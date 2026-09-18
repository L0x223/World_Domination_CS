<script setup>
import { computed } from 'vue'

const nukesSelected = computed(() => {
  return props.pendingStrikes.length
})

const nukesAvailable = computed(() => {
  return Math.max(
    0,
    props.controls.nukeCount - nukesSelected.value
  )
})

const props = defineProps({
  controls: {
    type: Object,
    required: true
  },

  opponents: {
    type: Array,
    default: () => []
  },

  pendingSanctions: {
    type: Set,
    default: () => new Set()
  },

  pendingStrikes: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits([
  'toggleSanction',
  'toggleStrike'
])

function isSanctionChecked(playerId) {
  const opponent = props.opponents.find(
    opp => opp.playerId === playerId
  )

  const currentlySanctioned =
    opponent?.isSanctioned ?? false

  const pendingToggle =
    props.pendingSanctions.has(playerId)

  return currentlySanctioned !== pendingToggle
}

function isStrikeChecked(playerId, cityId) {
  return props.pendingStrikes.some(
    strike =>
      strike.defenderPlayerId === playerId &&
      strike.cityId === cityId
  )
}

function onSanctionChange(playerId) {
  emit('toggleSanction', playerId)
}

function onStrikeChange(playerId, cityId) {
  emit('toggleStrike', {
    defenderPlayerId: playerId,
    cityId
  })
}

function isStrikeDisabled(city, playerId) {
  const checked = isStrikeChecked(playerId, city.cityId)

  if (checked) {
    return false
  }

  if (!city.canStrike) {
    return true
  }

  return nukesAvailable.value <= 0
}
</script>

<template>
  <div class="opponent-panel">
    <h2>Target Opponents</h2>

    <div
      v-if="props.opponents.length === 0"
      class="empty"
    >
      No opponents available.
    </div>
    <div class="strike-title">
          Nuclear strikes
          ({{ nukesAvailable }} / {{ props.controls.nukeCount }} nukes available)
        </div>
    <div
      v-for="opp in props.opponents"
      :key="opp.playerId"
      class="opponent-block"
    >
      <div class="opponent-header">
        <img
          v-if="opp.leaderIconId"
          :src="`@/assets/img/${opp.leaderIconId}.png`"
          :alt="opp.playerName"
          class="leader-icon"
        />

        <div>
          <div class="leader-nick">
            {{ opp.playerName }}
          </div>

          <div class="country-name">
            {{ opp.countryName }}
          </div>
        </div>
      </div>

      <div class="action-column">
        <label class="row">
          <input
            type="checkbox"
            :checked="isSanctionChecked(opp.playerId)"
            @change="onSanctionChange(opp.playerId)"
          />

          <span>Sanction</span>
        </label>

        <div class="strike-title">
          Nuclear strikes
        </div>

        <label
          v-for="city in opp.cities"
          :key="city.cityId"
          class="row"
          :class="{
            disabled: isStrikeDisabled(city, opp.playerId)
          }"
        >
          <input
            type="checkbox"
            :disabled="isStrikeDisabled(city, opp.playerId)"
            :checked="
              isStrikeChecked(
                opp.playerId,
                city.cityId
              )
            "
            @change="
              onStrikeChange(
                opp.playerId,
                city.cityId
              )
            "
          />

          <span>
            {{ city.cityName }}
            <span v-if="!city.isAlive">
              (destroyed)
            </span>
          </span>
        </label>

      </div>
    </div>
  </div>
</template>

<style scoped>
.opponent-panel {
  border: 2px solid #000;
  border-radius: 6px;
  padding: 16px;
  background: #fff;

  height: fit-content;
  max-height: 80vh;
  overflow-y: auto;
}

.opponent-panel h2 {
  margin-top: 0;
  margin-bottom: 16px;
}

.opponent-block {
  padding-bottom: 14px;
  margin-bottom: 14px;
  border-bottom: 1px solid #ccc;
}

.opponent-block:last-child {
  border-bottom: none;
  margin-bottom: 0;
  padding-bottom: 0;
}

.opponent-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}

.leader-icon {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  object-fit: cover;
}

.leader-nick {
  font-weight: bold;
}

.country-name {
  font-size: 0.9em;
  color: #666;
}

.action-column {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.strike-title {
  margin-top: 6px;
  font-size: 0.9em;
  font-weight: bold;
}

.row {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 0.9em;
}

.row.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.empty {
  color: #666;
  font-size: 0.9em;
}
</style>