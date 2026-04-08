<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('device.label', 2), to: '/devices' },
      { label: device?.name, to: `/devices/${deviceId}` },
      { label: t('device_template.grid_settings'), to: `/devices/${deviceId}/grid` },
    ]"
  >
    <template v-if="device" #actions>
      <chart-time-range-select
        ref="timeRangeSelectRef"
        class="col-grow col-lg-auto tw-bg-white grid-toolbar-control"
        @update:model-value="onTimeRangeChanged"
      />
      <q-btn-toggle
        v-model="gridMode"
        class="col-grow col-lg-auto grid-toolbar-control grid-mode-toggle"
        unelevated
        toggle-color="primary"
        color="white"
        no-caps
        text-color="primary"
        :options="gridModeOptions"
      />
      <q-btn
        class="col-grow col-lg-auto grid-toolbar-control refresh-btn"
        unelevated
        no-caps
        color="white"
        text-color="grey-5"
        :icon="mdiRefresh"
        @click="refreshGrid"
      >
        <div class="text-grey-10 text-weight-regular q-ml-sm">
          {{ t('global.refresh') }}
        </div>
      </q-btn>
    </template>
    <template v-if="device" #default>
      <div class="column q-gutter-lg">
        <div v-if="!hasGrid" class="q-mt-lg">
          <q-banner class="bg-grey-2 text-grey-8">
            {{ t('device_template.grid_not_configured') }}
          </q-banner>
        </div>
        <div v-else class="grid-wrapper">
          <div v-if="isLoadingGrid" class="grid-loading">
            <q-spinner color="primary" size="2rem" />
          </div>
          <div v-else>
            <div v-if="heatmapEnabled && activeHeatmapScale" class="heatmap-scale">
              <div class="heatmap-scale__header">
                <span class="heatmap-scale__title">{{ t('device_template.heatmap_scale') }}</span>
              </div>
              <div class="heatmap-scale__body">
                <div class="heatmap-scale__bar" :style="{ background: activeHeatmapScale.gradient }"></div>
                <div class="heatmap-scale__ticks">
                  <span
                    v-for="(tick, index) in activeHeatmapScale.ticks"
                    :key="`${index}-${tick.label}`"
                    class="heatmap-scale__tick-label"
                    :class="{
                      'heatmap-scale__tick-label--start': index === 0,
                      'heatmap-scale__tick-label--end': index === activeHeatmapScale.ticks.length - 1,
                    }"
                    :style="{ left: `${tick.position}%` }"
                  >
                    {{ tick.label }}
                  </span>
                </div>
              </div>
            </div>
            <div class="device-grid" :style="gridStyle">
              <div v-for="cell in gridCells" :key="cell.key" class="grid-cell">
                <LatestGridDataPointCard
                  v-if="cellData[cell.key]"
                  class="grid-card"
                  :device-id="deviceId"
                  :sensor-tag="cellData[cell.key].sensorTag"
                  :name="cellData[cell.key].sensorName"
                  :unit="cellData[cell.key].unit"
                  :accuracy-decimals="cellData[cell.key].accuracyDecimals"
                  :color="cellData[cell.key].color"
                  :heatmap-enabled="heatmapEnabled"
                  :background-color="getCardBackgroundColor(getDisplayValue(cellData[cell.key].value))"
                  :text-color="getCardTextColor(getDisplayValue(cellData[cell.key].value))"
                  :last-value="getDisplayValue(cellData[cell.key].value)"
                />
                <div v-else class="empty-grid-cell shadow"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { computed, reactive, ref, onUnmounted } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import ChartTimeRangeSelect from '@/components/datapoints/ChartTimeRangeSelect.vue';
import LatestGridDataPointCard from '@/components/datapoints/LatestGridDataPointCard.vue';
import DeviceService from '@/api/services/DeviceService';
import DataPointService from '@/api/services/DataPointService';
import type { DeviceResponse } from '@/api/services/DeviceService';
import type { GetLatestDataPointsQuery, GetLatestDataPointsResponse } from '@/api/services/DataPointService';
import type { ProblemDetails } from '@/api/types/ProblemDetails';
import { handleError } from '@/utils/error-handler';
import { getGraphColor } from '@/utils/colors';
import type { TimeRange } from '@/models/TimeRange';
import { mdiRefresh } from '@quasar/extras/mdi-v7';
import { useSignalR } from '@/composables/useSignalR';
import type { LastDataPoint } from '@/models/LastDataPoint';
import { subscribeToLastDataPointEvents } from '@/utils/signalrDataPoints';

const { t } = useI18n();
const route = useRoute();

const { connection, connect } = useSignalR();

const deviceId = route.params.id.toString();
const device = ref<DeviceResponse>();
const isLoadingGrid = ref(false);
const selectedTimeRange = ref<TimeRange | null>(null);
const timeRangeSelectRef = ref<{ updateTimeRange: () => void }>();
type GridMode = 'default' | 'heatmap1' | 'heatmap2';
const gridMode = ref<GridMode>('default');

const cellData = reactive<Record<string, GridCellData>>({});
const sensorCellMap = reactive<Record<string, string>>({});
const fallbackTimestamp = new Date(0).toISOString();
const heatmapIgnoredValue = 9999999;
let activeRequestId = 0;
const usesOneBasedX = ref<boolean | null>(null);
const usesOneBasedY = ref<boolean | null>(null);

interface GridCellData {
  sensorTag: string;
  sensorName: string;
  unit: string;
  accuracyDecimals: number;
  value: number | null;
  color: string;
  timestamp: string;
}

type DeviceSensor = NonNullable<DeviceResponse['deviceTemplate']>['sensors'][number];
type LatestDataPoint = GetLatestDataPointsResponse;
type GridLatestDataPointsQuery = GetLatestDataPointsQuery & { MaskStaleValue?: boolean };

const gridRows = computed(() => device.value?.deviceTemplate?.gridRowSpan ?? 0);
const gridColumns = computed(() => device.value?.deviceTemplate?.gridColumnSpan ?? 0);
const hasGrid = computed(() => gridRows.value > 0 && gridColumns.value > 0);
const heatmapEnabled = computed(() => gridMode.value !== 'default');
const gridModeOptions = computed(() => [
  { label: t('device_template.default_view'), value: 'default' as GridMode },
  { label: t('device_template.heatmap_1_view'), value: 'heatmap1' as GridMode },
  { label: t('device_template.heatmap_2_view'), value: 'heatmap2' as GridMode },
]);
const heatmapPalette = [
  { stop: 0, color: '#5e0b7b' },
  { stop: 0.22, color: '#5e4e9e' },
  { stop: 0.45, color: '#4769a5' },
  { stop: 0.68, color: '#2f9198' },
  { stop: 0.84, color: '#7bcf4d' },
  { stop: 1, color: '#ffe44a' },
];
const centeredHeatmapPalette = [
  { stop: 0, color: '#5e0b7b' },
  { stop: 0.25, color: '#4769a5' },
  { stop: 0.4, color: '#7bcf4d' },
  { stop: 0.5, color: '#ffe44a' },
  { stop: 0.6, color: '#7bcf4d' },
  { stop: 0.75, color: '#4769a5' },
  { stop: 1, color: '#5e0b7b' },
];

const gridStyle = computed(() => {
  if (!hasGrid.value) {
    return {};
  }

  return {
    gridTemplateColumns: `repeat(${gridColumns.value}, minmax(0, 1fr))`,
    gridTemplateRows: `repeat(${gridRows.value}, minmax(120px, 1fr))`,
  };
});

const gridCells = computed(() => {
  if (!hasGrid.value) {
    return [] as { row: number; col: number; key: string }[];
  }

  const cells: { row: number; col: number; key: string }[] = [];
  for (let row = 0; row < gridRows.value; row += 1) {
    for (let col = 0; col < gridColumns.value; col += 1) {
      cells.push({ row, col, key: `${row}-${col}` });
    }
  }
  return cells;
});

const numericCellValues = computed(() => {
  return Object.values(cellData)
    .map((cell) => getDisplayValue(cell.value))
    .filter((value): value is number => typeof value === 'number' && Number.isFinite(value));
});

const heatmapRange = computed(() => {
  if (numericCellValues.value.length === 0) {
    return null;
  }

  return {
    min: Math.min(...numericCellValues.value),
    max: Math.max(...numericCellValues.value),
  };
});

const heatmapAverage = computed(() => {
  if (numericCellValues.value.length === 0) {
    return null;
  }

  const total = numericCellValues.value.reduce((sum, value) => sum + value, 0);
  return total / numericCellValues.value.length;
});

const centeredHeatmapRange = computed(() => {
  if (numericCellValues.value.length === 0 || heatmapAverage.value === null) {
    return null;
  }

  const average = heatmapAverage.value;
  const maxDeviation = Math.max(...numericCellValues.value.map((value) => Math.abs(value - average)), 0);

  return {
    mean: average,
    maxDeviation,
    min: average - maxDeviation,
    max: average + maxDeviation,
  };
});

const heatmapTickLabels = computed(() => {
  if (!heatmapRange.value) {
    return [] as string[];
  }

  const { min, max } = heatmapRange.value;
  const points = 5;

  if (max <= min) {
    return Array.from({ length: points }, () => formatHeatmapTick(min));
  }

  const step = (max - min) / (points - 1);
  return Array.from({ length: points }, (_, index) => formatHeatmapTick(min + index * step));
});

const centeredHeatmapTickLabels = computed(() => {
  if (!centeredHeatmapRange.value) {
    return [] as string[];
  }

  const { min, max } = centeredHeatmapRange.value;
  const points = 5;

  if (max <= min) {
    return Array.from({ length: points }, () => formatHeatmapTick(min));
  }

  const step = (max - min) / (points - 1);
  return Array.from({ length: points }, (_, index) => formatHeatmapTick(min + index * step));
});

const heatmapGradient = computed(() => {
  return `linear-gradient(90deg, ${heatmapPalette
    .map((entry) => `${entry.color} ${Math.round(entry.stop * 100)}%`)
    .join(', ')})`;
});

const centeredHeatmapGradient = computed(() => {
  return `linear-gradient(90deg, ${centeredHeatmapPalette
    .map((entry) => `${entry.color} ${Math.round(entry.stop * 100)}%`)
    .join(', ')})`;
});

const activeHeatmapScale = computed(() => {
  if (gridMode.value === 'heatmap1' && heatmapRange.value) {
    return {
      gradient: heatmapGradient.value,
      ticks: heatmapTickLabels.value.map((label, index, labels) => ({
        label,
        position: labels.length <= 1 ? 0 : (index / (labels.length - 1)) * 100,
      })),
    };
  }

  if (gridMode.value === 'heatmap2' && centeredHeatmapRange.value) {
    return {
      gradient: centeredHeatmapGradient.value,
      ticks: centeredHeatmapTickLabels.value.map((label, index, labels) => ({
        label,
        position: labels.length <= 1 ? 0 : (index / (labels.length - 1)) * 100,
      })),
    };
  }

  return null;
});

async function getDevice() {
  const { data, error } = await DeviceService.getDevice(deviceId);
  if (error) {
    handleError(error, t('device.toasts.loading_failed'));
    return;
  }

  device.value = data;

  if (selectedTimeRange.value) {
    void loadGridData(selectedTimeRange.value);
  }
}

void getDevice();

function onTimeRangeChanged(range: TimeRange) {
  selectedTimeRange.value = range;
  if (device.value?.deviceTemplate?.sensors?.length) {
    void loadGridData(range);
  }
}

async function loadGridData(range: TimeRange) {
  if (!device.value?.deviceTemplate?.sensors?.length) {
    Object.keys(cellData).forEach((key) => delete cellData[key]);
    return;
  }

  if (!hasGrid.value) {
    Object.keys(cellData).forEach((key) => delete cellData[key]);
    return;
  }

  const rows = gridRows.value;
  const cols = gridColumns.value;
  const requestId = ++activeRequestId;
  isLoadingGrid.value = true;

  try {
    const from = new Date(range.from);
    const to = new Date(range.to);

    const query: GridLatestDataPointsQuery = {
      From: from.toISOString(),
      To: to.toISOString(),
      MaskStaleValue: false,
    };

    const errors: ProblemDetails[] = [];

    const results = await Promise.all(
      device.value.deviceTemplate.sensors.map(async (sensor, index) => {
        const { data, error } = await DataPointService.getLatestDataPoints(deviceId, sensor.tag, query);
        if (error) {
          errors.push(error);
          return null;
        }

        const latest = data;
        if (!latest) {
          return null;
        }

        return {
          sensor,
          latest,
          color: getGraphColor(index),
        } as SensorGridEntry;
      }),
    );

    if (requestId !== activeRequestId) {
      return;
    }

    if (errors.length > 0 && errors[0].status != 404) {
      handleError(errors[0], t('device.toasts.loading_failed'));
    }

    const entries = results.filter((entry): entry is SensorGridEntry => entry !== null);

    const xValues = entries
      .map((entry) => entry.latest.gridX)
      .filter((value): value is number => value !== null && value !== undefined);
    const yValues = entries
      .map((entry) => entry.latest.gridY)
      .filter((value): value is number => value !== null && value !== undefined);

    const nextUsesOneBasedX = xValues.length > 0 && xValues.every((value) => value >= 1) && !xValues.includes(0);
    const nextUsesOneBasedY = yValues.length > 0 && yValues.every((value) => value >= 1) && !yValues.includes(0);

    usesOneBasedX.value = nextUsesOneBasedX;
    usesOneBasedY.value = nextUsesOneBasedY;

    const nextCells: Record<string, GridCellData> = {};

    entries.forEach((entry) => {
      const { sensor, latest, color } = entry;
      if (latest.gridX === undefined || latest.gridX === null || latest.gridY === undefined || latest.gridY === null) {
        return;
      }

      const columnIndex = resolveGridIndex(latest.gridX, cols, 'x');
      const rowIndex = resolveGridIndex(latest.gridY, rows, 'y');

      if (columnIndex === null || rowIndex === null) {
        return;
      }

      const key = `${rowIndex}-${columnIndex}`;
      const current = nextCells[key];
      const currentTimestamp = current ? new Date(current.timestamp).getTime() : Number.NEGATIVE_INFINITY;
      const latestTimestamp = latest.ts ? new Date(latest.ts).getTime() : Number.NEGATIVE_INFINITY;

      if (!current || latestTimestamp >= currentTimestamp) {
        nextCells[key] = {
          sensorTag: sensor.tag,
          sensorName: sensor.name,
          unit: sensor.unit ?? '',
          accuracyDecimals: sensor.accuracyDecimals ?? 2,
          value: latest.value ?? null,
          color,
          timestamp: latest.ts ?? fallbackTimestamp,
        };
      }
    });

    Object.keys(cellData).forEach((key) => delete cellData[key]);
    Object.keys(sensorCellMap).forEach((key) => delete sensorCellMap[key]);
    Object.entries(nextCells).forEach(([key, value]) => {
      cellData[key] = value;
      sensorCellMap[value.sensorTag] = key;
    });
  } finally {
    if (requestId === activeRequestId) {
      isLoadingGrid.value = false;
    }
  }
}

function refreshGrid() {
  if (selectedTimeRange.value) {
    timeRangeSelectRef.value?.updateTimeRange();
  }
}

function resolveGridIndex(rawIndex: number | null | undefined, size: number, axis: 'x' | 'y'): number | null {
  if (rawIndex === null || rawIndex === undefined) {
    return null;
  }

  const useOneBased = axis === 'x' ? usesOneBasedX : usesOneBasedY;
  let resolvedIndex: number | null = null;

  if (useOneBased.value === true) {
    resolvedIndex = rawIndex - 1;
  } else if (useOneBased.value === false) {
    resolvedIndex = rawIndex;
  } else {
    if (rawIndex >= 0 && rawIndex < size) {
      useOneBased.value = false;
      resolvedIndex = rawIndex;
    } else if (rawIndex - 1 >= 0 && rawIndex - 1 < size) {
      useOneBased.value = true;
      resolvedIndex = rawIndex - 1;
    } else {
      resolvedIndex = rawIndex;
    }
  }

  if (resolvedIndex < 0 || resolvedIndex >= size) {
    return null;
  }

  return resolvedIndex;
}

async function subscribeToDeviceUpdates() {
  await connect();
  await connection.send('SubscribeToDevice', deviceId);
}
void subscribeToDeviceUpdates();

function handleLastDataPointUpdate(dataPoint: LastDataPoint) {
  if (dataPoint.deviceId !== deviceId) {
    return;
  }

  if (!device.value?.deviceTemplate?.sensors?.length) {
    return;
  }

  const rows = gridRows.value;
  const cols = gridColumns.value;

  if (rows <= 0 || cols <= 0) {
    return;
  }

  const sensor = device.value.deviceTemplate.sensors.find((item) => item.tag === dataPoint.tag);

  if (!sensor) {
    return;
  }

  const columnIndex = resolveGridIndex(dataPoint.gridX ?? null, cols, 'x');
  const rowIndex = resolveGridIndex(dataPoint.gridY ?? null, rows, 'y');

  if (columnIndex === null || rowIndex === null) {
    return;
  }

  const key = `${rowIndex}-${columnIndex}`;
  const timestamp = dataPoint.ts ?? fallbackTimestamp;
  const sensorIndex = device.value.deviceTemplate.sensors.findIndex((item) => item.tag === sensor.tag);
  const color = getGraphColor(sensorIndex >= 0 ? sensorIndex : 0);
  const existingKey = sensorCellMap[sensor.tag];

  if (existingKey && existingKey !== key) {
    const existingCell = cellData[existingKey];
    if (existingCell && existingCell.sensorTag === sensor.tag) {
      delete cellData[existingKey];
    }
  }

  const currentCell = cellData[key];
  const currentTimestamp = currentCell ? new Date(currentCell.timestamp).getTime() : Number.NEGATIVE_INFINITY;
  const newTimestamp = timestamp ? new Date(timestamp).getTime() : Number.NEGATIVE_INFINITY;

  if (!currentCell || newTimestamp >= currentTimestamp) {
    cellData[key] = {
      sensorTag: sensor.tag,
      sensorName: sensor.name,
      unit: sensor.unit ?? '',
      accuracyDecimals: sensor.accuracyDecimals ?? 2,
      value: dataPoint.value ?? null,
      color,
      timestamp,
    };
    sensorCellMap[sensor.tag] = key;
  }
}

function subscribeToLastDataPointUpdates() {
  return subscribeToLastDataPointEvents(connection, handleLastDataPointUpdate);
}
const unsubscribeFromLastDataPointUpdates = subscribeToLastDataPointUpdates();

onUnmounted(() => {
  void connection.send('UnsubscribeFromDevice', deviceId);
  unsubscribeFromLastDataPointUpdates();
});

interface SensorGridEntry {
  sensor: DeviceSensor;
  latest: LatestDataPoint;
  color: string;
}

function formatHeatmapTick(value: number) {
  if (!Number.isFinite(value)) {
    return '-';
  }

  return value.toFixed(2).replace(/\.?0+$/, '');
}

function getDisplayValue(value: number | null) {
  return value === heatmapIgnoredValue ? null : value;
}

function getCardBackgroundColor(value: number | null) {
  if (!heatmapEnabled.value) {
    return '#ffffff';
  }

  return getHeatmapColor(value);
}

function getCardTextColor(value: number | null) {
  if (!heatmapEnabled.value) {
    return '#000000';
  }

  const background = getHeatmapColor(value);
  if (background === '#ffffff') {
    return '#000000';
  }

  return getContrastTextColor(background);
}

function getHeatmapColor(value: number | null) {
  if (value === null || !Number.isFinite(value)) {
    return '#ffffff';
  }

  if (gridMode.value === 'heatmap2') {
    return getCenteredHeatmapColor(value);
  }

  if (!heatmapRange.value) {
    return '#ffffff';
  }

  const { min, max } = heatmapRange.value;

  if (max <= min) {
    return interpolatePaletteColor(heatmapPalette, 0.72);
  }

  const normalized = clamp((value - min) / (max - min), 0, 1);
  return interpolatePaletteColor(heatmapPalette, normalized);
}

function getCenteredHeatmapColor(value: number) {
  if (!centeredHeatmapRange.value) {
    return '#ffffff';
  }

  const { mean, maxDeviation } = centeredHeatmapRange.value;

  if (maxDeviation <= 0) {
    return interpolatePaletteColor(centeredHeatmapPalette, 0.5);
  }

  const normalized = clamp(0.5 + (value - mean) / (maxDeviation * 2), 0, 1);
  return interpolatePaletteColor(centeredHeatmapPalette, normalized);
}

function interpolatePaletteColor(palette: { stop: number; color: string }[], position: number) {
  const normalized = clamp(position, 0, 1);

  for (let index = 1; index < palette.length; index += 1) {
    const previous = palette[index - 1];
    const current = palette[index];

    if (normalized <= current.stop) {
      const range = current.stop - previous.stop || 1;
      const localRatio = (normalized - previous.stop) / range;
      return mixHexColors(previous.color, current.color, localRatio);
    }
  }

  return palette[palette.length - 1].color;
}

function mixHexColors(start: string, end: string, ratio: number) {
  const normalizedRatio = clamp(ratio, 0, 1);
  const startRgb = hexToRgb(start);
  const endRgb = hexToRgb(end);

  const mixed = startRgb.map((channel, index) => {
    return Math.round(channel + (endRgb[index] - channel) * normalizedRatio);
  });

  return rgbToHex(mixed[0], mixed[1], mixed[2]);
}

function hexToRgb(hex: string) {
  const normalized = hex.replace('#', '');
  const chunkSize = normalized.length === 3 ? 1 : 2;
  const values = normalized.match(new RegExp(`.{1,${chunkSize}}`, 'g')) ?? [];

  return values.map((value) => {
    const channel = chunkSize === 1 ? `${value}${value}` : value;
    return Number.parseInt(channel, 16);
  });
}

function rgbToHex(red: number, green: number, blue: number) {
  return `#${[red, green, blue]
    .map((channel) => clamp(Math.round(channel), 0, 255).toString(16).padStart(2, '0'))
    .join('')}`;
}

function getContrastTextColor(background: string) {
  const [red, green, blue] = hexToRgb(background);
  const luminance = (red * 0.299 + green * 0.587 + blue * 0.114) / 255;

  return luminance > 0.62 ? '#1f2933' : '#ffffff';
}

function clamp(value: number, min: number, max: number) {
  return Math.min(Math.max(value, min), max);
}
</script>

<style lang="scss" scoped>
.grid-controls {
  flex-wrap: wrap;
}

.grid-toolbar-control {
  align-self: stretch;
}

.grid-mode-toggle {
  border: 1px solid #027be3;
}

.grid-toolbar-control :deep(.q-field__control) {
  background: #ffffff;
}

.grid-wrapper {
  position: relative;
}

.heatmap-scale {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
  padding: 0.9rem 1rem 0.95rem;
  margin-bottom: 0.75rem;
  background: linear-gradient(180deg, #ffffff, #fbfbfd);
  border: 1px solid #e3e6ee;
  border-radius: 12px;
  box-shadow: 0 10px 24px rgba(31, 41, 51, 0.06);
}

.heatmap-scale__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.heatmap-scale__body {
  display: flex;
  flex-direction: column;
  gap: 0.45rem;
}

.heatmap-scale__title {
  font-size: 0.78rem;
  font-weight: 600;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: #556070;
}

.heatmap-scale__bar {
  height: 0.9rem;
  width: 100%;
  border-radius: 999px;
  overflow: hidden;
  box-shadow: inset 0 0 0 1px rgba(72, 81, 99, 0.14);
}

.heatmap-scale__ticks {
  position: relative;
  height: 1.2rem;
}

.heatmap-scale__tick-label {
  position: absolute;
  top: 0;
  font-size: 0.78rem;
  color: #5f6b7a;
  transform: translateX(-50%);
  white-space: nowrap;
}

.heatmap-scale__tick-label--start {
  transform: translateX(0);
}

.heatmap-scale__tick-label--end {
  transform: translateX(-100%);
}

.grid-loading {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 240px;
}

.device-grid {
  display: grid;
  gap: 0.75rem;
  width: 100%;
}

@media (max-width: 600px) {
  .device-grid {
    gap: 0.5rem;
  }
}

.grid-cell {
  display: flex;
}

.grid-card,
.empty-grid-cell {
  flex: 1 1 auto;
}

.grid-card :deep(.card) {
  height: 100%;
}

.empty-grid-cell {
  background-color: #ffffff;
  border-radius: 8px;
  border: 1px dashed #e0e0e0;
}

.refresh-btn {
  border: 1px solid rgb(187, 187, 187);
}

@media (max-width: 600px) {
  .heatmap-scale {
    padding: 0.8rem 0.85rem 0.9rem;
  }

  .heatmap-scale__tick-label {
    font-size: 0.72rem;
  }
}
</style>
