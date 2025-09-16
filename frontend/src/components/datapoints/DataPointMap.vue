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
      <q-btn-toggle
        v-model="mapMode"
        class="col-grow col-lg-auto custom-toggle"
        unelevated
        toggle-color="primary"
        color="white"
        no-caps
        text-color="primary"
        :options="mapModeOptions"
      />
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
import type { SensorData } from '@/models/SensorData';

import L, {
  type HeatLayer,
  type HeatLatLngTuple,
  type HeatMapOptions,
  type LatLngTuple,
  type Layer,
  type LayerGroup,
} from 'leaflet';

type HeatLayerInstance = HeatLayer & Layer;
type LeafletMap = ReturnType<typeof L.map>;
import 'leaflet/dist/leaflet.css';
import 'leaflet.heat';

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
const map = ref<LeafletMap | null>(null);
const markers: LayerGroup = L.layerGroup();
const heatLayer = ref<HeatLayerInstance | null>(null);
const heatLayerOptions: HeatMapOptions = {
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
  mapInstance.addLayer(markers as unknown as Layer);
}

function ensureHeatLayer(mapInstance: LeafletMap) {
  if (!heatLayer.value) {
    heatLayer.value = L.heatLayer([], heatLayerOptions) as HeatLayerInstance;
  }

  const layer = heatLayer.value;
  if (layer && !mapInstance.hasLayer(layer as unknown as Layer)) {
    layer.addTo(mapInstance);
  }

  return heatLayer.value;
}

function detachHeatLayer(mapInstance: LeafletMap) {
  const layer = heatLayer.value;
  if (layer && mapInstance.hasLayer(layer as unknown as Layer)) {
    mapInstance.removeLayer(layer as unknown as Layer);
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
    if (!mapInstance.hasLayer(markers as unknown as Layer)) {
      mapInstance.addLayer(markers as unknown as Layer);
    }
    detachHeatLayer(mapInstance as unknown as LeafletMap);
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
    if (mapInstance.hasLayer(markers as unknown as Layer)) {
      mapInstance.removeLayer(markers as unknown as Layer);
    }
    const layer = ensureHeatLayer(mapInstance as unknown as LeafletMap);
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
    const latlngs: LatLngTuple[] = pointsWithCoords.map((dp) => [dp.latitude, dp.longitude] as [number, number]);
    const bounds = L.latLngBounds(latlngs);
    mapInstance.fitBounds(bounds, { padding: [30, 30] });
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

<style lang="scss">
.custom-toggle {
  border: 1px solid #027be3;
}
</style>
