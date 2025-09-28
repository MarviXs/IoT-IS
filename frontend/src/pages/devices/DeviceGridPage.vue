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
        class="col-grow col-lg-auto tw-bg-white"
        @update:model-value="onTimeRangeChanged"
      />
      <q-btn
        class="col-grow col-lg-auto tw-bg-white"
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
        <div class="row items-center q-gutter-sm grid-controls"></div>
        <div v-if="!hasGrid" class="q-mt-lg">
          <q-banner class="bg-grey-2 text-grey-8">
            {{ t('device_template.grid_not_configured') }}
          </q-banner>
        </div>
        <div v-else class="grid-wrapper">
          <div v-if="isLoadingGrid" class="grid-loading">
            <q-spinner color="primary" size="2rem" />
          </div>
          <div v-else class="device-grid" :style="gridStyle">
            <div v-for="cell in gridCells" :key="cell.key" class="grid-cell">
              <LatestDataPointCard
                v-if="cellData[cell.key]"
                class="grid-card"
                :device-id="deviceId"
                :sensor-tag="cellData[cell.key].sensorTag"
                :name="cellData[cell.key].sensorName"
                :unit="cellData[cell.key].unit"
                :accuracy-decimals="cellData[cell.key].accuracyDecimals"
                :color="cellData[cell.key].color"
                :last-value="cellData[cell.key].value"
              />
              <div v-else class="empty-grid-cell"></div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import ChartTimeRangeSelect from '@/components/datapoints/ChartTimeRangeSelect.vue';
import LatestDataPointCard from '@/components/datapoints/LatestDataPointCard.vue';
import DeviceService from '@/api/services/DeviceService';
import DataPointService from '@/api/services/DataPointService';
import type { DeviceResponse } from '@/api/services/DeviceService';
import type { GetDataPointsQuery, GetDataPointsResponse } from '@/api/services/DataPointService';
import type { ProblemDetails } from '@/api/types/ProblemDetails';
import { handleError } from '@/utils/error-handler';
import { getGraphColor } from '@/utils/colors';
import type { TimeRange } from '@/models/TimeRange';
import { mdiRefresh } from '@quasar/extras/mdi-v7';

const { t } = useI18n();
const route = useRoute();

const deviceId = route.params.id.toString();
const device = ref<DeviceResponse>();
const isLoadingGrid = ref(false);
const selectedTimeRange = ref<TimeRange | null>(null);
const timeRangeSelectRef = ref<{ updateTimeRange: () => void }>();

const cellData = reactive<Record<string, GridCellData>>({});
let activeRequestId = 0;

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
type DataPointItem = GetDataPointsResponse extends Array<infer U> ? U : never;

const gridRows = computed(() => device.value?.deviceTemplate?.gridRowSpan ?? 0);
const gridColumns = computed(() => device.value?.deviceTemplate?.gridColumnSpan ?? 0);
const hasGrid = computed(() => gridRows.value > 0 && gridColumns.value > 0);

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

    const query: GetDataPointsQuery = {
      From: from.toISOString(),
      To: to.toISOString(),
    };

    const errors: ProblemDetails[] = [];

    const results = await Promise.all(
      device.value.deviceTemplate.sensors.map(async (sensor, index) => {
        const { data, error } = await DataPointService.getDataPoints(deviceId, sensor.tag, query);
        if (error) {
          errors.push(error);
          return null;
        }

        const latest = data?.[0];
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

    if (errors.length > 0) {
      handleError(errors[0], t('device.toasts.loading_failed'));
    }

    const entries = results.filter((entry): entry is SensorGridEntry => entry !== null);

    const xValues = entries
      .map((entry) => entry.latest.gridX)
      .filter((value): value is number => value !== null && value !== undefined);
    const yValues = entries
      .map((entry) => entry.latest.gridY)
      .filter((value): value is number => value !== null && value !== undefined);

    const usesOneBasedX = xValues.length > 0 && xValues.every((value) => value >= 1) && !xValues.includes(0);
    const usesOneBasedY = yValues.length > 0 && yValues.every((value) => value >= 1) && !yValues.includes(0);

    const nextCells: Record<string, GridCellData> = {};

    entries.forEach((entry) => {
      const { sensor, latest, color } = entry;
      if (latest.gridX === undefined || latest.gridX === null || latest.gridY === undefined || latest.gridY === null) {
        return;
      }

      let columnIndex = latest.gridX;
      let rowIndex = latest.gridY;

      if (usesOneBasedX) {
        columnIndex -= 1;
      }
      if (usesOneBasedY) {
        rowIndex -= 1;
      }

      if (columnIndex < 0 || columnIndex >= cols || rowIndex < 0 || rowIndex >= rows) {
        return;
      }

      const key = `${rowIndex}-${columnIndex}`;
      const current = nextCells[key];
      const currentTimestamp = current ? new Date(current.timestamp).getTime() : Number.NEGATIVE_INFINITY;
      const latestTimestamp = new Date(latest.ts).getTime();

      if (!current || latestTimestamp >= currentTimestamp) {
        nextCells[key] = {
          sensorTag: sensor.tag,
          sensorName: sensor.name,
          unit: sensor.unit ?? '',
          accuracyDecimals: sensor.accuracyDecimals ?? 2,
          value: latest.value ?? null,
          color,
          timestamp: latest.ts,
        };
      }
    });

    Object.keys(cellData).forEach((key) => delete cellData[key]);
    Object.entries(nextCells).forEach(([key, value]) => {
      cellData[key] = value;
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

interface SensorGridEntry {
  sensor: DeviceSensor;
  latest: DataPointItem;
  color: string;
}
</script>

<style lang="scss" scoped>
.grid-controls {
  flex-wrap: wrap;
}

.grid-wrapper {
  position: relative;
}

.grid-loading {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 240px;
}

.device-grid {
  display: grid;
  gap: 1rem;
  width: 100%;
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
</style>
