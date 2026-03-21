<template>
  <div>
    <div class="shadow card full-width column items-center justify-center grid-card">
      <div class="sensor-name">{{ name }}</div>
      <div class="sensor-value">{{ valueRounded }} {{ unit }}</div>
    </div>
    <div v-if="!heatmapEnabled" class="color-strip"></div>
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
  heatmapEnabled: boolean;
  backgroundColor: string;
  textColor: string;
  lastValue: number | null;
}>();

const valueRounded = computed(() => {
  return props.lastValue?.toFixed(props.accuracyDecimals) ?? '-';
});
</script>

<style scoped>
.grid-card {
  border-top-left-radius: 0.5rem;
  border-top-right-radius: 0.5rem;
  border-bottom-left-radius: v-bind('heatmapEnabled ? "0.5rem" : "0"');
  border-bottom-right-radius: v-bind('heatmapEnabled ? "0.5rem" : "0"');
  background-color: v-bind(backgroundColor);
}
.sensor-name {
  font-weight: 400;
  color: v-bind(textColor);
  font-size: 1rem;
  text-align: center;
}

.sensor-value {
  margin-top: 0.25rem;
  font-weight: 500;
  font-size: 1.3rem;
  color: v-bind(textColor);
  text-align: center;
}

.color-strip {
  height: 0.2rem;
  width: 100%;
  background-color: v-bind(color);
  border-bottom-left-radius: 0.5rem;
  border-bottom-right-radius: 0.5rem;
}

@media (max-width: 600px) {
  .sensor-name {
    font-size: 0.9rem;
  }

  .sensor-value {
    font-size: 1.05rem;
  }
}
</style>
