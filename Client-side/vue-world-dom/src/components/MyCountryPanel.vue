<script setup>
import { ref, onMounted, reactive, computed } from 'vue'

const props = defineProps({
  country: Object,   // MyCountryDto (view.me)
  controls: Object,  // ControlPanelDto
  ecologyHistory: Array, // EcologyHistoryDto
  pending: Object // PendingActionsDto i dont have this but it's sound right
})

const localBudget = computed(() => {
  let budget = props.controls.budget 

  budget -= props.pending.investCityIds.size * props.controls.costInvest
  budget -= props.pending.shieldCityIds.size * props.controls.costShield
  if (props.pending.ecologyInvest) budget -= props.controls.costEcology
  if (props.pending.nukeTech) budget -= props.controls.nukeTechCost
  budget -= props.pending.nukesToBuild * props.controls.costNuke

  return budget
})

const nukeBuildCount = ref(1)
const buildNukes = ref(false)

function toggleInvest(cityId) {
  if (props.pending.investCityIds.has(cityId)) {
    props.pending.investCityIds.delete(cityId)
  } else {
    props.pending.investCityIds.add(cityId)
  }
}

function toggleShield(cityId) {
  if (props.pending.shieldCityIds.has(cityId)) {
    props.pending.shieldCityIds.delete(cityId)
  } else {
    props.pending.shieldCityIds.add(cityId)
  }
}

function toggleNukeTech() {
  props.pending.nukeTech = !props.pending.nukeTech
}

function toggleEcologyInvest() {
  props.pending.ecologyInvest = !props.pending.ecologyInvest
}

function toggleBuildNukes() {
  buildNukes.value = !buildNukes.value

  if (buildNukes.value) {
    props.pending.nukesToBuild = nukeBuildCount.value
  } else {
    props.pending.nukesToBuild = 0
  }
}

</script>

<template>
  <div class="country-panel">
    <h2>{{ props.country.countryName }}</h2>

    <div class="stats-row">
      <span>Budget: {{ localBudget }}</span>
      <span>Nuke Tech: {{ props.country.hasNuclearTech ? 'Yes' : 'No' }}</span>
      <span>Nukes: {{ props.country.nukeCount }}</span>
      <span>Ecology: {{ Math.round((props.ecologyHistory?.at(-1) ?? 0) * 100) }}%</span>
    </div>

    <div class="cities-grid">
      <div
        v-for="city in props.country.cities"
        :key="city.cityId"
        class="city-card"
        :class="{ dead: !city.isAlive }"
      >
        <h3>{{ city.cityName }}</h3>
        <p v-if="!city.isAlive" class="destroyed">Destroyed</p>
        <template v-else>
          <p>Economic: {{ Math.round((city.economic ?? 0) * 100) }}%</p>
          <p>Shield: {{ city.hasShield ? 'Up' : 'None' }}</p>

          <label class="action-row">
            <input
              type="checkbox"
              :checked="props.pending.investCityIds.has(city.cityId)"
              :disabled="!props.pending.investCityIds.has(city.cityId) && 
              localBudget < props.controls.costInvest "
              @change="toggleInvest(city.cityId)"
            />
            Invest ({{ props.controls.costInvest }})
          </label>

          <label class="action-row">
            <input
              type="checkbox"
              :checked="city.hasShield"
              :disabled="( !props.pending.shieldCityIds.has(city.cityId) && 
              localBudget < props.controls.costShield  )
              || city.hasShield"
              @change="toggleShield(city.cityId)"
            />
            Build Shield ({{ props.controls.costShield }})
          </label>
        </template>
      </div>
    </div>

    <div class="global-actions">
      <label class="action-row">
        <input
          type="checkbox"
          :checked="props.country.hasNuclearTech"
          :disabled="props.country.hasNuclearTech ||
           (localBudget < props.controls.nukeTechCost &&
            !props.pending.nukeTech)"
          @change="toggleNukeTech"
        />
        Research Nuclear Tech ({{ props.controls.nukeTechCost }})
      </label>

    </div>
    <div>
        <label class="action-row">
          <input
            type="checkbox"
            :checked="props.pending.ecologyInvest"
            :disabled="localBudget < props.controls.costEcology
            && !props.pending.ecologyInvest"
            @change="toggleEcologyInvest"
          />
          Invest in Ecology ({{ props.controls.costEcology }})
        </label>
      </div>
      
      <div v-if="props.country.hasNuclearTech" class="build-nukes-row">
        <label class="action-row">Build nukes:
        <input type="number" v-model.number="nukeBuildCount" min="1" :disabled="buildNukes"/>
        <input type="checkbox"
              :disabled="localBudget < nukeBuildCount * props.controls.costNuke
              && props.pending.nukesToBuild == 0"
              @change="toggleBuildNukes"/>
          Build ({{ props.controls.costNuke }} each)
          </label>
          </div>
      </div>
</template>

<style scoped>
.country-panel {
  border: 2px solid #000;
  padding: 16px;
  border-radius: 6px;
  margin-bottom: 16px;
}

.stats-row {
  display: flex;
  gap: 16px;
  margin-bottom: 16px;
  font-weight: bold;
}

.cities-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 16px;
}

.city-card {
  border: 1px solid #999;
  border-radius: 4px;
  padding: 10px;
  width: 180px;
}

.city-card.dead {
  background: #f5c6c6;
}

.destroyed {
  color: #900;
  font-weight: bold;
}

.action-row {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 6px;
  font-size: 0.9em;
}

.global-actions {
  display: flex;
  flex-direction: column;
  gap: 10px;
  border-top: 1px solid #ccc;
  padding-top: 12px;
}

.build-nukes-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.build-nukes-row input {
  width: 60px;
}

.ecology-btn {
  align-self: flex-start;
  padding: 6px 14px;
  cursor: pointer;
}
</style>