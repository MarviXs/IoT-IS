<template>
  <div class="full-width groups-container row">
    <!-- Grouped Sensors -->
    <div v-for="(sensors, groupName) in groupedSensors" :key="groupName" class="group-column">
      <div class="group-item">
        <h5 class="group-name">{{ groupName }}</h5>
      </div>
      <div class="data-card" v-for="sensor in sensors" :key="sensor.tag">
        <LatestDataPointCard
          :device-id="sensor.deviceId"
          :sensor-tag="sensor.tag"
          :name="sensor.name"
          :unit="sensor.unit ?? ''"
          :accuracy-decimals="sensor.accuracyDecimals ?? 2"
          :color="getGraphColorFromSensor(sensor)"
          :last-value="sensor.lastValue ?? null"
        />
      </div>
    </div>
  </div>
  <!-- Ungrouped Sensors -->
  <div class="col-12 col-md-4 col-lg-3 col-xl-2" v-for="sensor in ungroupedSensors" :key="'ungrouped-' + sensor.tag">
    <LatestDataPointCard
      :device-id="sensor.deviceId"
      :sensor-tag="sensor.tag"
      :name="sensor.name"
      :unit="sensor.unit ?? ''"
      :accuracy-decimals="sensor.accuracyDecimals ?? 2"
      :color="getGraphColorFromSensor(sensor)"
      :last-value="sensor.lastValue ?? null"
    />
  </div>
</template>

<script setup lang="ts">
import { getGraphColor } from '@/utils/colors';
import { computed } from 'vue';
import LatestDataPointCard from '../datapoints/LatestDataPointCard.vue';
import type { SensorData } from '@/models/SensorData';

const sensors = defineModel<SensorData[]>('sensors');

// Group sensors by their group field
const groupedSensors = computed(() => {
  const groups: Record<string, SensorData[]> = {};
  (sensors.value ?? []).forEach((sensor) => {
    if (sensor.group) {
      if (!groups[sensor.group]) {
        groups[sensor.group] = [];
      }
      groups[sensor.group].push(sensor);
    }
  });
  return groups;
});

// Get sensors without a group
const ungroupedSensors = computed(() => {
  return (sensors.value ?? []).filter((sensor) => !sensor.group);
});

const getGraphColorFromSensor = (sensor: SensorData): string => {
  if (sensor.group) {
    const groupNames = Object.keys(groupedSensors.value);
    const index = groupNames.indexOf(sensor.group);
    return getGraphColor(index);
  }

  return getGraphColor(sensors.value.indexOf(sensor));
};
</script>

<style lang="scss" scoped>
.groups-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 0.25rem;
  width: 100%;
  box-sizing: border-box;
}

.group-column {
  background: #f5f5f5;
  padding: 1rem;
  border-radius: 0.5rem;
  text-align: center;
}

.group-name {
  font-size: 1.2rem;
  font-weight: 500;
}

.data-card {
  margin: 1rem 0;
}

@media (min-width: 600px) {
  .groups-container {
    gap: 2rem;
  }
}
</style>
