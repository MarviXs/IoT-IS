<template>
  <div>
    <div class="row items-center justify-start q-mb-md q-gutter-x-md q-gutter-y-sm">
      <p class="text-weight-medium text-h6 chart-title">Map</p>
      <q-space></q-space>
      <!-- <chart-time-range-select
        class="col-grow col-lg-auto"
        ref="timeRangeSelectRef"
        @update:model-value="updateTimeRange"
      ></chart-time-range-select> -->
      <q-btn
        padding="0.5rem 1rem"
        outline
        no-caps
        color="grey-7"
        text-color="grey-5"
        class="options-btn col-grow col-lg-auto"
        :icon="mdiRefresh"
      >
        <template #default>
          <div class="text-grey-10 text-weight-regular q-ml-sm">
            {{ t('global.refresh') }}
          </div>
        </template>
      </q-btn>
      <q-btn
        padding="0.5rem 1rem"
        outline
        no-caps
        color="grey-7"
        text-color="grey-5"
        class="options-btn col-grow col-lg-auto"
      >
        <template #default>
          <div class="text-grey-10 text-weight-regular">
            {{ t('global.options') }}
          </div>
        </template>
      </q-btn>
    </div>
    <div id="map" style="height: 550px; width: 100%"></div>
  </div>
</template>

<script setup lang="ts">
import DataPointService, { GetDataPointsQuery } from '@/api/services/DataPointService';
import { DataPoint } from '@/models/DataPoint';
import { TimeRange } from '@/models/TimeRange';
import { computed, PropType, ref, watch, onMounted, onBeforeUnmount } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiRefresh } from '@quasar/extras/mdi-v7';

import L, { LayerGroup } from 'leaflet';
import 'leaflet/dist/leaflet.css';

export type SensorData = {
  id: string;
  deviceId: string;
  tag: string;
  name: string;
  unit?: string | null;
  accuracyDecimals?: number | null;
  lastValue?: number | null;
  group?: string;
};

const props = defineProps({
  sensors: {
    type: Array as PropType<SensorData[]>,
    required: true,
  },
});

const selectedSensorId = defineModel('selectedSensorId', {
  type: String as PropType<string | null>,
  default: null,
});

const { t } = useI18n();
const selectedTimeRange = ref<TimeRange | null>(null);

function getSensorUniqueId(sensor: SensorData) {
  return `${sensor.deviceId}-${sensor.tag}`;
}

async function updateTimeRange(timeRange: TimeRange) {
  selectedTimeRange.value = timeRange;
  await getDataPoints();
}

const selectedSensor = computed(() => {
  return props.sensors.find((sensor) => getSensorUniqueId(sensor) === selectedSensorId.value) ?? null;
});

const dataPoints = ref<DataPoint[]>([]);
async function getDataPoints() {
  const from = new Date(selectedTimeRange.value?.from ?? 0);
  const to = new Date(selectedTimeRange.value?.to ?? Date.now());

  const sensor = selectedSensor.value;
  if (!sensor) return;

  const query: GetDataPointsQuery = {
    From: from.toISOString(),
    To: to.toISOString(),
  };

  const { data, error } = await DataPointService.getDataPoints(sensor.deviceId, sensor.tag, query);

  if (error) {
    console.error(error);
    return;
  }

  dataPoints.value = data ?? [];
}

// --- Leaflet Map Integration ---
const map = ref<L.Map | null>(null);
const markers: LayerGroup = L.layerGroup();

function createMap() {
  if (map.value) return;
  const mapInstance = L.map('map').setView([51.505, -0.09], 13);
  map.value = mapInstance;

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; OpenStreetMap contributors',
    maxZoom: 19,
  }).addTo(mapInstance);
  markers.addTo(mapInstance);
}

function updateMarkers() {
  markers.clearLayers();
  if (!map.value) return;
  const sensor = selectedSensor.value;
  if (!sensor) return;

  const pointsWithCoords = dataPoints.value.filter(
    (dp): dp is DataPoint & { latitude: number; longitude: number } =>
      dp.latitude !== null && dp.latitude !== undefined && dp.longitude !== null && dp.longitude !== undefined,
  );

  pointsWithCoords.forEach((dp) => {
    const marker = L.circleMarker([dp.latitude, dp.longitude], {
      radius: 8,
      color: '#1976d2',
      fillColor: '#2196f3',
      fillOpacity: 0.7,
      weight: 2,
    }).bindPopup(`<b>${sensor.name}</b><br/>${dp.value ?? ''}${sensor.unit ? ` ${sensor.unit}` : ''}`);
    markers.addLayer(marker);
  });
  // Fit bounds if there are points
  if (pointsWithCoords.length > 0) {
    const latlngs = pointsWithCoords.map((dp) => [dp.latitude, dp.longitude] as [number, number]);
    map.value.fitBounds(latlngs, { padding: [30, 30] });
  }
}

onMounted(() => {
  createMap();
  updateMarkers();
  void getDataPoints();
});

watch(dataPoints, () => {
  updateMarkers();
});

onBeforeUnmount(() => {
  if (map.value) {
    map.value.remove();
    map.value = null;
  }
});

watch(selectedSensorId, (newValue) => {
  if (newValue) {
    void getDataPoints();
  } else {
    dataPoints.value = [];
  }
});
</script>

<style lang="scss"></style>
