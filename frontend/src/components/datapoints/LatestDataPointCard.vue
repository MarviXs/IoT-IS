<template>
  <div>
    <div class="shadow card bg-white rounded full-width q-pa-md column items-center justify-center">
      <div class="sensor-name">{{ name }}</div>
      <div class="sensor-value">{{ valueRounded }} {{ unit }}</div>
    </div>
    <div class="color-strip"></div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
const props = defineProps<{
  deviceId: string;
  sensorTag: string;
  name: string;
  unit: string;
  accuracyDecimals: number;
  color: string;
  lastValue: number | null;
}>();

const valueRounded = computed(() => {
  return props.lastValue?.toFixed(props.accuracyDecimals) ?? '-';
});
</script>

<style scoped>
.sensor-name {
  font-weight: 400;
  color: var(--q-grey-color);
  font-size: 1rem;
}

.sensor-value {
  margin-top: 0.25rem;
  font-weight: 500;
  font-size: 1.3rem;
}

.color-strip {
  height: 0.2rem;
  width: 100%;
  background-color: v-bind(color);
}
</style>
