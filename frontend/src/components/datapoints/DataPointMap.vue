<template>
  <div>
    <div class="row items-center justify-start q-mb-md q-gutter-x-md q-gutter-y-sm">
      <p class="text-weight-medium text-h6 chart-title">{{ mapStrings.title }}</p>
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
        :icon="refreshIcon"
      >
        <template #default>
          <div class="text-grey-10 text-weight-regular q-ml-sm">
            {{ t('global.refresh') }}
          </div>
        </template>
      </q-btn>
      <q-btn-toggle
        v-model="mapMode"
        class="col-grow col-lg-auto"
        rounded
        dense
        unelevated
        color="primary"
        toggle-color="primary"
        text-color="white"
        :options="mapModeOptions"
      />
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
import type { GetDataPointsQuery } from '@/api/services/DataPointService';
import DataPointService from '@/api/services/DataPointService';
import type { DataPoint } from '@/models/DataPoint';
import type { TimeRange } from '@/models/TimeRange';
import type { PropType } from 'vue';
import { computed, ref, watch, onMounted, onBeforeUnmount } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiRefresh } from '@quasar/extras/mdi-v7';

import L, { type HeatLayer, type HeatLatLngTuple, type LatLngExpression, type LayerGroup } from 'leaflet';
import 'leaflet/dist/leaflet.css';
import 'leaflet.heat';

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
type MapMode = 'markers' | 'heat';
const selectedTimeRange = ref<TimeRange | null>(null);
const mapMode = ref<MapMode>('markers');
const mapStrings = computed(() => ({
  title: t('datapoints.map.title'),
  markers: t('datapoints.map.markers'),
  heatmap: t('datapoints.map.heatmap'),
}));
const mapModeOptions = computed(() => [
  { label: mapStrings.value.markers, value: 'markers' as MapMode },
  { label: mapStrings.value.heatmap, value: 'heat' as MapMode },
]);
const refreshIcon = mdiRefresh;

function getSensorUniqueId(sensor: SensorData) {
  return `${sensor.deviceId}-${sensor.tag}`;
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
const heatLayer = ref<HeatLayer | null>(null);
const heatLayerOptions = {
  radius: 25,
  blur: 15,
  maxZoom: 17,
};

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

function ensureHeatLayer(mapInstance: L.Map) {
  if (!heatLayer.value) {
    heatLayer.value = L.heatLayer([], heatLayerOptions);
  }

  const layer = heatLayer.value;
  if (layer && !mapInstance.hasLayer(layer)) {
    layer.addTo(mapInstance);
  }

  return heatLayer.value;
}

function detachHeatLayer(mapInstance: L.Map) {
  const layer = heatLayer.value;
  if (layer && mapInstance.hasLayer(layer)) {
    mapInstance.removeLayer(layer);
  }
}

function updateMapVisualization() {
  if (!map.value) return;

  const mapInstance = map.value;
  const sensor = selectedSensor.value;
  const pointsWithCoords = sensor
    ? dataPoints.value.filter(
        (dp): dp is DataPoint & { latitude: number; longitude: number } =>
          dp.latitude !== null && dp.latitude !== undefined && dp.longitude !== null && dp.longitude !== undefined,
      )
    : [];

  if (mapMode.value === 'markers') {
    if (!mapInstance.hasLayer(markers)) {
      markers.addTo(mapInstance);
    }
    detachHeatLayer(mapInstance);
    markers.clearLayers();

    if (sensor) {
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
    }
  } else {
    if (mapInstance.hasLayer(markers)) {
      mapInstance.removeLayer(markers);
    }
    const layer = ensureHeatLayer(mapInstance);
    if (layer) {
      const heatPoints: HeatLatLngTuple[] = sensor
        ? pointsWithCoords.map((dp) => {
            const intensity = typeof dp.value === 'number' ? Math.max(0.1, Math.abs(dp.value)) : 0.5;
            return [dp.latitude, dp.longitude, intensity];
          })
        : [];
      layer.setLatLngs(heatPoints);
    }
  }

  if (pointsWithCoords.length > 0) {
    const latlngs: LatLngExpression[] = pointsWithCoords.map((dp) => [dp.latitude, dp.longitude] as [number, number]);
    mapInstance.fitBounds(latlngs, { padding: [30, 30] });
  }
}

onMounted(() => {
  void refreshIcon;
  createMap();
  updateMapVisualization();
  void getDataPoints();
});

watch(dataPoints, () => {
  updateMapVisualization();
});

watch(mapMode, () => {
  void mapModeOptions.value;
  void mapStrings.value;
  updateMapVisualization();
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
    updateMapVisualization();
  }
});
</script>

<style lang="scss"></style>
