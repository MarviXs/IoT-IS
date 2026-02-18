<template>
  <div id="chart">
    <div class="row items-center justify-start q-mb-md q-col-gutter-x-sm q-col-gutter-y-sm">
      <p class="text-weight-medium text-h6 chart-title">{{ t('chart.label') }}</p>
      <q-space></q-space>

      <div class="col-12 col-md-auto gt-sm">
        <q-btn
          padding="0.5rem 1rem"
          outline
          no-caps
          color="grey-7"
          text-color="grey-5"
          class="options-btn full-width"
          @click="resetZoom"
        >
          <template #default>
            <div class="text-grey-10 text-weight-regular">Reset Zoom</div>
          </template>
        </q-btn>
      </div>

      <div class="col-12 col-md-auto gt-sm">
        <q-btn
          padding="0.5rem 1rem"
          outline
          no-caps
          color="grey-7"
          text-color="grey-5"
          class="options-btn full-width"
          @click="openExportDialog"
        >
          <div class="text-grey-9">{{ t('chart.export_csv') }}</div>
        </q-btn>
      </div>

      <div class="col-12 col-md-auto gt-sm">
        <q-btn
          padding="0.5rem 1rem"
          outline
          no-caps
          color="grey-7"
          text-color="grey-5"
          class="options-btn full-width"
          @click="openDeleteDialog"
        >
          <div class="text-grey-9">{{ t('chart.delete_data_points') }}</div>
        </q-btn>
      </div>

      <div class="col-12 col-md-auto gt-sm">
        <q-btn
          padding="0.5rem 1rem"
          outline
          no-caps
          color="grey-7"
          text-color="grey-5"
          class="options-btn full-width"
          @click="isGraphOptionsDialogOpen = true"
        >
          <template #default>
            <div class="text-grey-10 text-weight-regular">
              {{ t('global.options') }}
            </div>
          </template>
        </q-btn>
      </div>
      <div class="col-12 lt-md">
        <q-btn
          padding="0.5rem 1rem"
          outline
          no-caps
          color="grey-7"
          text-color="grey-5"
          class="options-btn full-width"
          :icon="mdiDotsHorizontal"
        >
          <q-menu anchor="bottom right" self="top right">
            <q-list style="min-width: 180px">
              <q-item clickable v-close-popup @click="openExportDialog">
                <q-item-section>{{ t('chart.export_csv') }}</q-item-section>
              </q-item>
              <q-item clickable v-close-popup @click="openDeleteDialog">
                <q-item-section>{{ t('chart.delete_data_points') }}</q-item-section>
              </q-item>
              <q-item clickable v-close-popup @click="isGraphOptionsDialogOpen = true">
                <q-item-section>{{ t('global.options') }}</q-item-section>
              </q-item>
              <q-item clickable v-close-popup @click="resetZoom">
                <q-item-section>Reset zoom</q-item-section>
              </q-item>
              <q-item clickable v-close-popup @click="refresh">
                <q-item-section>{{ t('global.refresh') }}</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
        </q-btn>
      </div>
      <div class="col-12 col-md-auto gt-sm">
        <q-btn
          padding="0.5rem 1rem"
          outline
          no-caps
          color="grey-7"
          text-color="grey-5"
          class="options-btn full-width"
          :icon="mdiRefresh"
          @click="refresh"
        >
          <template #default>
            <div class="text-grey-10 text-weight-regular q-ml-sm">
              {{ t('global.refresh') }}
            </div>
          </template>
        </q-btn>
      </div>
      <div class="col-12 col-lg-auto">
        <chart-time-range-select
          ref="timeRangeSelectRef"
          :initial-time-range="chartInitialTimeRange"
          :initial-custom-time-range="chartInitialCustomTimeRange"
          @update:model-value="updateTimeRange"
          @time-range-changed="onTimeRangeChanged"
        ></chart-time-range-select>
      </div>
    </div>
    <div class="chart-wrapper">
      <canvas id="chart" ref="chartRef" class="chart"></canvas>
    </div>
    <dialog-common v-model="isGraphOptionsDialogOpen">
      <template #title>{{ t('global.options') }}</template>
      <template #default>
        <GraphOptionsForm v-model="graphOptions" @on-submit="isGraphOptionsDialogOpen = false" />
      </template>
    </dialog-common>
    <dialog-common v-model="isExportDialogOpen" min-width="520px">
      <template #title>{{ t('chart.export_csv') }}</template>
      <template #default>
        <q-form @submit.prevent="submitExport">
          <q-card-section class="q-pt-none column q-gutter-y-lg">
            <div class="column q-col-gutter-y-sm">
              <div class="text-subtitle2">{{ t('device.sensors') }}</div>
              <q-option-group
                v-model="exportSelectedSensors"
                type="checkbox"
                dense
                :options="sensorOptions"
                color="primary"
              />
            </div>
            <div class="column q-col-gutter-y-sm">
              <div class="text-subtitle2">{{ t('time_range.label') }}</div>
              <q-select
                v-model="exportSelectedTimeRange"
                outlined
                emit-value
                map-options
                :options="exportTimeRangeOptions"
              />
              <div v-if="exportSelectedTimeRange === 'custom'" class="column q-gutter-sm">
                <DateTimeInput v-model="exportCustomRange.from" :label="t('time_range.from')" />
                <DateTimeInput v-model="exportCustomRange.to" :label="t('time_range.to')" :show-now-button="true" />
              </div>
            </div>
            <div class="column q-col-gutter-y-sm">
              <div class="text-subtitle2">{{ t('chart.export_options') }}</div>
              <q-checkbox v-model="exportRoundNumbers" :label="t('chart.export_round_numbers')" dense />
              <div class="column q-gutter-y-xs">
                <div class="text-body2">{{ t('chart.export_orientation') }}</div>
                <q-option-group
                  v-model="exportOrientation"
                  type="radio"
                  dense
                  color="primary"
                  :options="exportOrientationOptions"
                />
              </div>
            </div>
          </q-card-section>
          <q-card-actions align="right" class="text-primary">
            <q-btn v-close-popup flat :label="t('global.cancel')" no-caps :disable="exportLoading" />
            <q-btn
              unelevated
              color="primary"
              :label="t('chart.export_csv')"
              no-caps
              padding="6px 20px"
              type="submit"
              :loading="exportLoading"
            />
          </q-card-actions>
        </q-form>
      </template>
    </dialog-common>
    <dialog-common v-model="isDeleteDialogOpen" min-width="520px">
      <template #title>{{ t('chart.delete_data_points') }}</template>
      <template #default>
        <q-form @submit.prevent="submitDelete">
          <q-card-section class="q-pt-none column q-gutter-y-lg">
            <div class="column q-col-gutter-y-sm">
              <div class="text-subtitle2">{{ t('device.sensors') }}</div>
              <q-option-group
                v-model="deleteSelectedSensors"
                type="checkbox"
                dense
                :options="sensorOptions"
                color="primary"
              />
            </div>
            <div class="column q-col-gutter-y-sm">
              <div class="text-subtitle2">{{ t('time_range.label') }}</div>
              <q-select
                v-model="deleteSelectedTimeRange"
                outlined
                emit-value
                map-options
                :options="deleteTimeRangeOptions"
              />
              <div v-if="deleteSelectedTimeRange === 'custom'" class="column q-gutter-sm">
                <DateTimeInput v-model="deleteCustomRange.from" :label="t('time_range.from')" />
                <DateTimeInput v-model="deleteCustomRange.to" :label="t('time_range.to')" :show-now-button="true" />
              </div>
            </div>
            <div class="column q-col-gutter-y-sm">
              <div class="text-subtitle2">{{ t('chart.delete_summary') }}</div>
              <div v-if="deleteCountsLoading" class="column q-gutter-y-xs">
                <q-skeleton type="text" class="w-100" />
                <q-skeleton type="text" class="w-100" />
              </div>
              <div v-else class="column q-gutter-y-xs">
                <div class="text-body2">
                  {{ t('chart.delete_total_points', { count: formatCount(deleteTotalDataPoints) }) }}
                </div>
                <div class="text-body2">
                  {{ t('chart.delete_selected_points', { count: formatCount(deleteSelectedDataPoints) }) }}
                </div>
              </div>
            </div>
          </q-card-section>
          <q-card-actions align="right" class="text-primary">
            <q-btn v-close-popup flat :label="t('global.cancel')" no-caps :disable="deleteLoading" />
            <q-btn
              unelevated
              color="negative"
              :label="t('chart.delete_action')"
              no-caps
              padding="6px 20px"
              type="submit"
              :disable="deleteActionDisabled"
              :loading="deleteLoading"
            />
          </q-card-actions>
        </q-form>
      </template>
    </dialog-common>
  </div>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import { ref, reactive, onMounted, shallowRef, watch, watchEffect, computed } from 'vue';
import ChartTimeRangeSelect from '@/components/datapoints/ChartTimeRangeSelect.vue';
import type { TimeRange } from '@/models/TimeRange';
import { getGraphColor, transparentize } from '@/utils/colors';
import { useStorage } from '@vueuse/core';
import { useI18n } from 'vue-i18n';
import DialogCommon from '../core/DialogCommon.vue';
import type { GraphOptions } from './GraphOptionsForm.vue';
import GraphOptionsForm from './GraphOptionsForm.vue';
import DateTimeInput from './DateTimeInput.vue';
import type {
  DeleteDataPointsQuery,
  GetDataPointCountQuery,
  GetDataPointsQuery,
} from '@/api/services/DataPointService';
import type { DataPoint } from '@/models/DataPoint';
import DataPointService from '@/api/services/DataPointService';
import type { ChartDataset, ChartEvent, LegendItem, ScaleOptions } from 'chart.js/auto';
import { Chart } from 'chart.js/auto';
import 'chartjs-adapter-date-fns';
import zoomPlugin from 'chartjs-plugin-zoom';
import { mdiDotsHorizontal, mdiRefresh } from '@quasar/extras/mdi-v7';
import { useInterval } from '@/composables/useInterval';
import { mkConfig, generateCsv, download } from 'export-to-csv';
import { format } from 'date-fns/format';
import { toast } from 'vue3-toastify';
import type { SensorData } from '@/models/SensorData';

Chart.register(zoomPlugin);

// Hack to allow grouping of datasets by unit
type DataSetCustom = ChartDataset<'line', { x: string; y?: number | null }[]> & {
  sensorKey: string;
};

const props = defineProps({
  sensors: {
    type: Array as PropType<SensorData[]>,
    required: true,
  },
  initialTimeRange: {
    type: String,
    required: false,
    default: undefined,
  },
  initialCustomTimeRange: {
    type: Object as PropType<{ from: string; to: string }>,
    required: false,
    default: undefined,
  },
  timeRangeStorageKey: {
    type: String,
    required: false,
    default: undefined,
  },
});

const emit = defineEmits(['refresh']);

const tickedNodes = defineModel('tickedNodes', {
  type: Array as PropType<string[]>,
  default: [],
});

const isGraphOptionsDialogOpen = ref(false);
const isExportDialogOpen = ref(false);

// Use first sensor's deviceId for per-device settings (assumes all sensors are from the same device)
const deviceId = computed(() => props.sensors[0]?.deviceId ?? 'unknown');

// Store ticked sensor state per device
const storedTickedNodes = useStorage<string[]>('tickedSensors_unknown', []);

const storageKeySuffix = computed(() => props.timeRangeStorageKey ?? deviceId.value);

const graphOptions = useStorage<GraphOptions>(`graphOptions_${storageKeySuffix.value}`, {
  refreshInterval: 30,
  timeFormat: '24h',
  interpolationMethod: 'straight',
  lineStyle: 'lines',
  lineWidth: 3,
  markerSize: 3,
  samplingOption: 'BUCKETS',
  downsampleResolution: 100,
  downsampleMethod: 'Lttb',
  timeBucketSizeSeconds: 60,
  timeBucketMethod: 'Average',
});

const storedTimeRange = useStorage(`chartTimeRange_${storageKeySuffix.value}`, '6h');
const storedCustomTimeRange = useStorage(`chartCustomTimeRange_${storageKeySuffix.value}`, {
  from: '',
  to: '',
});

const { t } = useI18n();

const timeRangeSelectRef = ref();
const selectedTimeRange = ref<TimeRange>();

const chartRef = ref<HTMLCanvasElement>();
const chart = shallowRef<Chart<'line', { x: string; y?: number | null }[]>>();

const exportSelectedSensors = ref<string[]>([]);
const exportSelectedTimeRange = ref('');
const exportRoundNumbers = ref(true);
const exportOrientation = ref<'column' | 'row'>('column');
const exportCustomRange = reactive({
  from: '',
  to: '',
});
const exportLoading = ref(false);

const isDeleteDialogOpen = ref(false);
const deleteSelectedSensors = ref<string[]>([]);
const deleteSelectedTimeRange = ref('all');
const deleteCustomRange = reactive({
  from: '',
  to: '',
});
const deleteLoading = ref(false);
const deleteCountsLoading = ref(false);
const deleteTotalDataPoints = ref(0);
const deleteSelectedDataPoints = ref(0);

type TimeRangeOption = {
  label: string;
  value: string;
  seconds?: number;
};

const relativeTimeRangeDefinitions = [
  {
    value: '5m',
    seconds: 300,
    exportLabelKey: 'time_range.predefined.last_5min',
    deleteLabelKey: 'time_range.predefined.older_than_5min',
  },
  {
    value: '15m',
    seconds: 900,
    exportLabelKey: 'time_range.predefined.last_15min',
    deleteLabelKey: 'time_range.predefined.older_than_15min',
  },
  {
    value: '30m',
    seconds: 1800,
    exportLabelKey: 'time_range.predefined.last_30min',
    deleteLabelKey: 'time_range.predefined.older_than_30min',
  },
  {
    value: '1h',
    seconds: 3600,
    exportLabelKey: 'time_range.predefined.last_1h',
    deleteLabelKey: 'time_range.predefined.older_than_1h',
  },
  {
    value: '6h',
    seconds: 21600,
    exportLabelKey: 'time_range.predefined.last_6h',
    deleteLabelKey: 'time_range.predefined.older_than_6h',
  },
  {
    value: '12h',
    seconds: 43200,
    exportLabelKey: 'time_range.predefined.last_12h',
    deleteLabelKey: 'time_range.predefined.older_than_12h',
  },
  {
    value: '24h',
    seconds: 86400,
    exportLabelKey: 'time_range.predefined.last_24h',
    deleteLabelKey: 'time_range.predefined.older_than_24h',
  },
  {
    value: '1w',
    seconds: 604800,
    exportLabelKey: 'time_range.predefined.last_week',
    deleteLabelKey: 'time_range.predefined.older_than_week',
  },
  {
    value: '1m',
    seconds: 2592000,
    exportLabelKey: 'time_range.predefined.last_month',
    deleteLabelKey: 'time_range.predefined.older_than_month',
  },
] as const;

type RelativeTimeRangeDefinition = (typeof relativeTimeRangeDefinitions)[number];

const exportRelativeTimeRangeOptions = computed<TimeRangeOption[]>(() =>
  relativeTimeRangeDefinitions.map((range) => ({
    label: t(range.exportLabelKey),
    value: range.value,
    seconds: range.seconds,
  })),
);

const deleteRelativeTimeRangeOptions = computed<TimeRangeOption[]>(() =>
  relativeTimeRangeDefinitions.map((range) => ({
    label: t(range.deleteLabelKey),
    value: range.value,
    seconds: range.seconds,
  })),
);

const exportTimeRangeOptions = computed<TimeRangeOption[]>(() => [
  ...exportRelativeTimeRangeOptions.value,
  { label: t('time_range.custom'), value: 'custom' },
]);

const deleteTimeRangeOptions = computed<TimeRangeOption[]>(() => [
  { label: t('time_range.predefined.all'), value: 'all' },
  ...deleteRelativeTimeRangeOptions.value,
  { label: t('time_range.custom'), value: 'custom' },
]);

function findRelativeTimeRange(value: string): RelativeTimeRangeDefinition | undefined {
  return relativeTimeRangeDefinitions.find((range) => range.value === value);
}

const exportOrientationOptions = computed(() => [
  { label: t('chart.export_orientation_column'), value: 'column' },
  { label: t('chart.export_orientation_row'), value: 'row' },
]);

const sensorOptions = computed(() =>
  props.sensors.map((sensor) => ({
    label: `${sensor.name}${sensor.unit ? ` (${sensor.unit})` : ''}`,
    value: getSensorUniqueId(sensor),
  })),
);

const numberFormatter = new Intl.NumberFormat();

function formatCount(value: number | null | undefined) {
  return numberFormatter.format(value ?? 0);
}

function getSensorUniqueId(sensor: SensorData) {
  return `${sensor.deviceId}-${sensor.tag}`;
}

function openExportDialog() {
  initializeExportForm();
  isExportDialogOpen.value = true;
}

function initializeExportForm() {
  const availableKeys = props.sensors.map((sensor) => getSensorUniqueId(sensor));
  const defaultSelection = tickedNodes.value.filter((key) => availableKeys.includes(key));

  exportSelectedSensors.value = defaultSelection.length > 0 ? [...defaultSelection] : [...availableKeys];

  const storedRange = storedTimeRange.value ?? '';
  const matchingOption = exportTimeRangeOptions.value.find((option) => option.value === storedRange);
  const defaultRange = exportTimeRangeOptions.value.find((option) => option.value === '6h');

  exportSelectedTimeRange.value =
    matchingOption?.value ?? defaultRange?.value ?? exportTimeRangeOptions.value[0]?.value ?? '';

  exportCustomRange.from = storedCustomTimeRange.value?.from ?? '';
  exportCustomRange.to = storedCustomTimeRange.value?.to ?? '';

  if (exportSelectedTimeRange.value === 'custom' && !exportCustomRange.from) {
    const now = new Date();
    const oneHourAgo = new Date(now.getTime() - 60 * 60 * 1000);
    exportCustomRange.from = format(oneHourAgo, 'yyyy-MM-dd HH:mm:ss');
    exportCustomRange.to = format(now, 'yyyy-MM-dd HH:mm:ss');
  }

  exportRoundNumbers.value = true;
  exportOrientation.value = 'column';
}

function openDeleteDialog() {
  initializeDeleteForm();
  isDeleteDialogOpen.value = true;
  void loadDeleteTotals();
}

function initializeDeleteForm() {
  const availableKeys = props.sensors.map((sensor) => getSensorUniqueId(sensor));
  const defaultSelection = tickedNodes.value.filter((key) => availableKeys.includes(key));

  deleteSelectedSensors.value = defaultSelection.length > 0 ? [...defaultSelection] : [...availableKeys];
  deleteSelectedTimeRange.value = 'all';

  deleteCustomRange.from = storedCustomTimeRange.value?.from ?? '';
  deleteCustomRange.to = storedCustomTimeRange.value?.to ?? '';

  deleteTotalDataPoints.value = 0;
  deleteSelectedDataPoints.value = 0;
}

type ExportTimeRange = {
  from: string;
  to: string;
};

function parseDateInput(value?: string | null): Date | null {
  if (!value) {
    return null;
  }

  const parsedDate = new Date(value);
  return Number.isNaN(parsedDate.getTime()) ? null : parsedDate;
}

function resolveExportTimeRange(): ExportTimeRange | null {
  if (exportSelectedTimeRange.value === 'custom') {
    const fromDate = parseDateInput(exportCustomRange.from);
    if (!fromDate) {
      return null;
    }

    const toDate = parseDateInput(exportCustomRange.to) ?? new Date();

    if (toDate.getTime() < fromDate.getTime()) {
      return null;
    }

    return {
      from: format(fromDate, 'yyyy-MM-dd HH:mm:ss'),
      to: format(toDate, 'yyyy-MM-dd HH:mm:ss'),
    };
  }

  const selectedDefinition = findRelativeTimeRange(exportSelectedTimeRange.value);

  if (!selectedDefinition) {
    return null;
  }

  const now = new Date();
  const fromDate = new Date(now.getTime() - selectedDefinition.seconds * 1000);

  return {
    from: format(fromDate, 'yyyy-MM-dd HH:mm:ss'),
    to: format(now, 'yyyy-MM-dd HH:mm:ss'),
  };
}

type DeleteTimeRange = {
  from?: string;
  to?: string;
};

type DeleteResolvedTimeRange = DeleteTimeRange | 'all';

function resolveDeleteTimeRange(): DeleteResolvedTimeRange | null {
  if (deleteSelectedTimeRange.value === 'all') {
    return 'all';
  }

  if (deleteSelectedTimeRange.value === 'custom') {
    const fromDate = parseDateInput(deleteCustomRange.from);
    if (!fromDate) {
      return null;
    }

    const toDate = parseDateInput(deleteCustomRange.to) ?? new Date();

    if (toDate.getTime() < fromDate.getTime()) {
      return null;
    }

    return {
      from: format(fromDate, 'yyyy-MM-dd HH:mm:ss'),
      to: format(toDate, 'yyyy-MM-dd HH:mm:ss'),
    };
  }

  const selectedDefinition = findRelativeTimeRange(deleteSelectedTimeRange.value);

  if (!selectedDefinition) {
    return null;
  }

  const now = new Date();
  const toDate = new Date(now.getTime() - selectedDefinition.seconds * 1000);

  return {
    to: format(toDate, 'yyyy-MM-dd HH:mm:ss'),
  };
}

async function getDataPointCountForSensors(sensorKeys: string[], range?: DeleteTimeRange): Promise<number> {
  const selectedKeys = new Set(sensorKeys);
  const sensors = props.sensors.filter((sensor) => selectedKeys.has(getSensorUniqueId(sensor)));

  if (sensors.length === 0) {
    return 0;
  }

  const queries = sensors.map(async (sensor) => {
    const query: GetDataPointCountQuery = {};

    if (range?.from) {
      query.From = new Date(range.from).toISOString();
    }

    if (range?.to) {
      query.To = new Date(range.to).toISOString();
    }

    const { data, error } = await DataPointService.getDataPointCount(sensor.deviceId, sensor.tag, query);

    if (error) {
      return -1;
    }

    return data?.totalCount ?? 0;
  });

  const counts = await Promise.all(queries);
  return counts.reduce((sum, value) => sum + value, 0);
}

async function loadDeleteTotals() {
  if (!isDeleteDialogOpen.value) {
    return;
  }

  if (deleteSelectedSensors.value.length === 0) {
    deleteTotalDataPoints.value = 0;
    deleteSelectedDataPoints.value = 0;
    return;
  }

  deleteCountsLoading.value = true;

  try {
    const total = await getDataPointCountForSensors(deleteSelectedSensors.value);
    deleteTotalDataPoints.value = total;

    const range = resolveDeleteTimeRange();

    if (range === null) {
      deleteSelectedDataPoints.value = 0;
      return;
    }

    if (range === 'all') {
      deleteSelectedDataPoints.value = total;
      return;
    }

    const selected = await getDataPointCountForSensors(deleteSelectedSensors.value, range);
    deleteSelectedDataPoints.value = selected;
  } catch (error) {
    console.error(error);
    deleteTotalDataPoints.value = 0;
    deleteSelectedDataPoints.value = 0;
    toast.error(t('chart.delete_count_error'));
  } finally {
    deleteCountsLoading.value = false;
  }
}

const deleteActionDisabled = computed(() => {
  if (deleteLoading.value) {
    return true;
  }

  if (deleteSelectedSensors.value.length === 0) {
    return true;
  }

  return resolveDeleteTimeRange() === null;
});

async function submitExport() {
  if (exportSelectedSensors.value.length === 0) {
    toast.error(t('chart.export_select_sensor_error'));
    return;
  }

  const timeRange = resolveExportTimeRange();

  if (!timeRange) {
    toast.error(t('chart.export_invalid_range'));
    return;
  }

  exportLoading.value = true;

  try {
    const exportData = new Map<string, DataPoint[]>();
    const sensorKeys = new Set(exportSelectedSensors.value);

    const promises = props.sensors.map(async (sensor) => {
      const key = getSensorUniqueId(sensor);

      if (!sensorKeys.has(key) || !sensor.id) {
        return;
      }

      const query: GetDataPointsQuery = {
        From: new Date(timeRange.from).toISOString(),
        To: new Date(timeRange.to).toISOString(),
      };

      const { data, error } = await DataPointService.getDataPoints(sensor.deviceId, sensor.tag, query);

      if (error) {
        console.error(error);
        return;
      }

      exportData.set(key, data ?? []);
    });

    await Promise.all(promises);

    const csvContent = generateCSVData({
      data: exportData,
      sensorKeys: exportSelectedSensors.value,
      roundNumbers: exportRoundNumbers.value,
      orientation: exportOrientation.value,
    });

    if (!csvContent) {
      return;
    }

    download(csvConfig)(csvContent);
    isExportDialogOpen.value = false;
  } finally {
    exportLoading.value = false;
  }
}

async function submitDelete() {
  if (deleteSelectedSensors.value.length === 0) {
    toast.error(t('chart.delete_select_sensor_error'));
    return;
  }

  const range = resolveDeleteTimeRange();

  if (range === null) {
    toast.error(t('chart.delete_invalid_range'));
    return;
  }

  const selectedKeys = new Set(deleteSelectedSensors.value);
  const sensors = props.sensors.filter((sensor) => selectedKeys.has(getSensorUniqueId(sensor)));

  if (sensors.length === 0) {
    toast.error(t('chart.delete_select_sensor_error'));
    return;
  }

  deleteLoading.value = true;

  try {
    const query: DeleteDataPointsQuery = {};

    if (range !== 'all') {
      if (range.from) {
        query.From = new Date(range.from).toISOString();
      }

      if (range.to) {
        query.To = new Date(range.to).toISOString();
      }
    }

    const sensorTagsByDevice = new Map<string, Set<string>>();
    for (const sensor of sensors) {
      const tags = sensorTagsByDevice.get(sensor.deviceId) ?? new Set<string>();
      tags.add(sensor.tag);
      sensorTagsByDevice.set(sensor.deviceId, tags);
    }

    const deletions = Array.from(sensorTagsByDevice.entries()).map(async ([deviceId, sensorTags]) => {
      const { data, error } = await DataPointService.deleteDataPoints(deviceId, Array.from(sensorTags), query);

      if (error) {
        console.error(error);
        throw new Error(`Failed to delete datapoints for device ${deviceId}`);
      }

      return data?.deletedCount ?? 0;
    });

    const deletedCounts = await Promise.all(deletions);
    const totalDeleted = deletedCounts.reduce((sum, value) => sum + value, 0);

    toast.success(t('chart.delete_success', { count: formatCount(totalDeleted) }));

    isDeleteDialogOpen.value = false;
    await getDataPoints();
  } catch (error) {
    console.error(error);
    toast.error(t('chart.delete_error'));
  } finally {
    deleteLoading.value = false;
  }
}

const currentXMin = ref<string>();
const currentXMax = ref<string>();
const shouldResetZoom = ref(false);

function onTimeRangeChanged(timeRangeName: string, customRangeData?: { from: string; to: string } | null) {
  shouldResetZoom.value = true;

  if (storedTimeRange.value !== timeRangeName) {
    storedTimeRange.value = timeRangeName;
  }

  if (timeRangeName === 'custom' && customRangeData) {
    const currentRange = storedCustomTimeRange.value ?? { from: '', to: '' };
    if (currentRange.from !== customRangeData.from || currentRange.to !== customRangeData.to) {
      storedCustomTimeRange.value = { ...customRangeData };
    }
  }
}

async function updateTimeRange(timeRange: TimeRange) {
  selectedTimeRange.value = timeRange;

  currentXMin.value = timeRange.from;
  currentXMax.value = timeRange.to;

  await getDataPoints();
}

function getSensorColor(index: number): string {
  return getGraphColor(index);
}

const dataPoints = reactive<Map<string, DataPoint[]>>(new Map());
async function getDataPoints() {
  if (props.sensors.length === 0) return;

  const from = new Date(selectedTimeRange.value?.from ?? 0);
  const to = new Date(selectedTimeRange.value?.to ?? Date.now());

  const promises = props.sensors.map(async (sensor) => {
    if (!sensor.id) return;

    const query: GetDataPointsQuery = {
      From: from.toISOString(),
      To: to.toISOString(),
    };

    if (graphOptions.value.samplingOption === 'DOWNSAMPLE') {
      query.Downsample = graphOptions.value.downsampleResolution;
      query.DownsampleMethod = graphOptions.value.downsampleMethod;
    } else if (graphOptions.value.samplingOption === 'BUCKETS') {
      query.TimeBucket = graphOptions.value.timeBucketSizeSeconds;
      query.TimeBucketMethod = graphOptions.value.timeBucketMethod;
    }

    const { data, error } = await DataPointService.getDataPoints(sensor.deviceId, sensor.tag, query);

    if (error) {
      console.error(error);
      return;
    }
    dataPoints.set(getSensorUniqueId(sensor), data ?? []);
  });

  await Promise.all(promises);

  updateChartData();
}

async function refresh() {
  timeRangeSelectRef.value?.updateTimeRange();
  emit('refresh');
}

function roundNumber(num: number | undefined | null, decimals: number) {
  if (num === undefined || num === null) return num;
  const factor = 10 ** decimals;
  return Math.round(num * factor) / factor;
}

function updateChartData() {
  if (!chart.value) return;
  const isZoomed = chart.value.isZoomedOrPanned();
  const shouldResetChartZoom = shouldResetZoom.value;

  const datasets: DataSetCustom[] = [];
  const uniqueUnits = new Map<string, { key: string; color: string; accuracyDecimals: number }>();
  const yScales: { [key: string]: ScaleOptions } = {};

  props.sensors.forEach((sensor, index) => {
    const key = getSensorUniqueId(sensor);
    const unit = sensor.unit ?? '';
    const sensorColor = getSensorColor(index);
    const accuracyDecimals = sensor.accuracyDecimals ?? 2;

    if (!uniqueUnits.has(unit)) {
      uniqueUnits.set(unit, { key, color: sensorColor, accuracyDecimals });
    }

    const unitInfo = uniqueUnits.get(unit);
    datasets.push({
      sensorKey: key,
      yAxisID: unitInfo?.key,
      label: sensor.name,
      hidden: !tickedNodes.value.includes(key),
      data:
        dataPoints.get(key)?.map((dataPoint) => ({
          x: dataPoint.ts,
          y: dataPoint.value,
        })) ?? [],
      backgroundColor: transparentize(sensorColor, 0.5),
      borderColor: sensorColor,
      cubicInterpolationMode: graphOptions.value.interpolationMethod === 'bezier' ? 'monotone' : 'default',
      showLine: graphOptions.value.lineStyle === 'lines' || graphOptions.value.lineStyle === 'linesmarkers',
      borderWidth: graphOptions.value.lineWidth,
      borderJoinStyle: 'round',
      pointStyle: graphOptions.value.lineStyle === 'lines' ? false : 'circle',
      pointRadius: graphOptions.value.markerSize,
      pointHoverRadius: graphOptions.value.markerSize + 2,
      pointHoverBorderWidth: graphOptions.value.lineWidth,
    });
  });

  uniqueUnits.forEach((unitInfo, unit) => {
    const { key, color, accuracyDecimals } = unitInfo;
    const index = Array.from(uniqueUnits.values()).indexOf(unitInfo);

    yScales[key] = {
      type: 'linear',
      position: 'left',
      display: tickedNodes.value.some((node) =>
        datasets.find((dataset) => dataset.sensorKey === node && dataset.yAxisID === key),
      ),
      axis: 'y',
      grid: {
        drawOnChartArea: index === 0,
      },
      ticks: {
        color: color,
        callback: function (value) {
          if (typeof value !== 'number') return value;
          return `${roundNumber(value, accuracyDecimals)} ${unit}`;
        },
      },
    };
  });

  chart.value.data = {
    labels: props.sensors.map((sensor) => sensor.name),
    datasets,
  };

  if (!chart.value.options.scales) return;

  currentXMin.value = selectedTimeRange.value?.from;
  currentXMax.value = selectedTimeRange.value?.to;

  if (chart.value.options.scales.x && (!isZoomed || shouldResetChartZoom)) {
    chart.value.options.scales.x.min = selectedTimeRange.value?.from;
    chart.value.options.scales.x.max = selectedTimeRange.value?.to;
  }

  Object.assign(chart.value.options.scales, yScales);

  chart.value.update();

  if (shouldResetChartZoom) {
    chart.value?.resetZoom?.();
    shouldResetZoom.value = false;
  }
}

function onLegendClick(e: ChartEvent, legendItem: LegendItem) {
  const sensorIdx = legendItem.datasetIndex;
  if (sensorIdx === undefined) return;

  const sensor = props.sensors[sensorIdx];
  const key = getSensorUniqueId(sensor);
  const currentHidden = !tickedNodes.value.includes(key);

  if (currentHidden) {
    tickedNodes.value = [...tickedNodes.value, key];
  } else {
    tickedNodes.value = tickedNodes.value.filter((node) => node !== key);
  }
}

function resetZoom() {
  if (!chart.value?.options.scales?.x) return;

  chart.value.options.scales.x.min = currentXMin.value;
  chart.value.options.scales.x.max = currentXMax.value;
  chart.value.update();

  chart.value?.resetZoom();
}

function updateDatasetVisibility(tickedNodes: string[]) {
  if (!chart.value) return;

  const datasets = chart.value.data.datasets as DataSetCustom[];

  datasets.forEach((dataset) => {
    const key = dataset.sensorKey;
    dataset.hidden = !tickedNodes.includes(key);
  });

  // Update y-axis visibility based on the visibility of datasets
  if (chart.value.options.scales) {
    Object.keys(chart.value.options.scales ?? {}).forEach((scaleID) => {
      if (scaleID === 'x') return; // Skip the x-axis
      const scale = chart.value?.options.scales?.[scaleID] as ScaleOptions;
      if (scale) {
        scale.display = datasets.some((dataset) => dataset.yAxisID === scaleID && !dataset.hidden);
      }
    });
  }

  chart.value.update();
}

// Update storage when deviceId changes and synchronize with model
watch(
  () => deviceId.value,
  (newDeviceId) => {
    // Update the storage key when device changes
    const storageKey = `tickedSensors_${newDeviceId}`;
    const storedValue = localStorage.getItem(storageKey);

    if (storedValue) {
      try {
        const parsedValue = JSON.parse(storedValue);
        if (Array.isArray(parsedValue)) {
          storedTickedNodes.value = parsedValue;
        }
      } catch (e) {
        console.error('Failed to parse stored ticked nodes:', e);
      }
    }
  },
  { immediate: true },
);

// Synchronize stored ticked nodes with model value
watch(
  () => tickedNodes.value,
  (newTickedNodes) => {
    const storageKey = `tickedSensors_${deviceId.value}`;
    localStorage.setItem(storageKey, JSON.stringify(newTickedNodes));
    storedTickedNodes.value = [...newTickedNodes];
  },
  { deep: true },
);

// Initialize tickedNodes from storage when sensors change
watch(
  () => props.sensors,
  () => {
    if (props.sensors.length > 0 && storedTickedNodes.value.length > 0) {
      // Only restore ticked nodes that still exist in current sensors
      const currentSensorKeys = props.sensors.map((sensor) => getSensorUniqueId(sensor));
      const validStoredNodes = storedTickedNodes.value.filter((key) => currentSensorKeys.includes(key));

      if (validStoredNodes.length > 0) {
        tickedNodes.value = validStoredNodes;
      } else {
        // If no stored nodes are valid, default to all sensors visible
        tickedNodes.value = currentSensorKeys;
      }
    } else if (props.sensors.length > 0 && tickedNodes.value.length === 0) {
      // If no stored state and no current ticked nodes, default to all sensors visible
      tickedNodes.value = props.sensors.map((sensor) => getSensorUniqueId(sensor));
    }
  },
  { immediate: true },
);

watchEffect(() => {
  updateDatasetVisibility(tickedNodes.value);
});

const chartInitialTimeRange = computed(() => {
  if (props.initialCustomTimeRange) {
    return undefined;
  }

  if (props.initialTimeRange && props.initialTimeRange !== 'custom') {
    return props.initialTimeRange;
  }

  return storedTimeRange.value === 'custom' ? undefined : storedTimeRange.value;
});

const chartInitialCustomTimeRange = computed(() => {
  if (props.initialCustomTimeRange) {
    return props.initialCustomTimeRange;
  }

  return storedTimeRange.value === 'custom' ? storedCustomTimeRange.value : undefined;
});

watch(
  () => props.initialCustomTimeRange,
  (range) => {
    if (range?.from && range?.to) {
      storedTimeRange.value = 'custom';
      storedCustomTimeRange.value = range;
    }
  },
  { immediate: true, deep: true },
);

watch(
  () => props.initialTimeRange,
  (range) => {
    if (range && range !== 'custom') {
      storedTimeRange.value = range;
    }
  },
  { immediate: true },
);

watch(
  () => isGraphOptionsDialogOpen.value,
  () => {
    if (!isGraphOptionsDialogOpen.value) {
      getDataPoints();
    }
  },
);

watch(
  () => [
    deleteSelectedSensors.value.join(','),
    deleteSelectedTimeRange.value,
    deleteCustomRange.from,
    deleteCustomRange.to,
  ],
  () => {
    if (!isDeleteDialogOpen.value) {
      return;
    }

    void loadDeleteTotals();
  },
);

watch(
  () => props.sensors.map((sensor) => getSensorUniqueId(sensor)).join(','),
  (currentKeysCsv) => {
    const currentKeys = currentKeysCsv ? currentKeysCsv.split(',') : [];
    deleteSelectedSensors.value = deleteSelectedSensors.value.filter((key) => currentKeys.includes(key));

    if (isDeleteDialogOpen.value) {
      void loadDeleteTotals();
    }
  },
);

onMounted(() => {
  const ctx = chartRef.value?.getContext('2d');
  if (!ctx) return;

  chart.value = new Chart(ctx, {
    type: 'line',
    data: {
      labels: [],
      datasets: [],
    },
    options: {
      maintainAspectRatio: false,
      animation: false,
      elements: {
        line: {
          tension: 0,
        },
      },
      plugins: {
        legend: {
          onClick: onLegendClick,
        },
        tooltip: {
          callbacks: {
            label: (context) => {
              const sensor = props.sensors[context.datasetIndex];
              const roundedValue =
                sensor.accuracyDecimals !== undefined && sensor.accuracyDecimals !== null
                  ? roundNumber(context.parsed.y, sensor.accuracyDecimals)
                  : context.parsed.y;
              return `${context.dataset.label}: ${roundedValue} ${sensor.unit || ''}`;
            },
          },
        },
        zoom: {
          pan: {
            enabled: true,
            mode: 'x',
            modifierKey: 'ctrl',
          },
          zoom: {
            drag: {
              enabled: true,
              backgroundColor: 'rgba(200,200,200,0.3)',
            },
            mode: 'x',
            // check if clicked inside chart area. If this returns false, zooming is aborted and onZoomRejected is invoked
            onZoomStart: (e) =>
              e.point.x > e.chart.chartArea.left &&
              e.point.x < e.chart.chartArea.right &&
              e.point.y > e.chart.chartArea.top &&
              e.point.y < e.chart.chartArea.bottom,
          },
        },
      },
      interaction: {
        intersect: false,
      },
      scales: {
        x: {
          axis: 'x',
          type: 'time',
          grid: {
            drawOnChartArea: false,
          },
          time: {
            tooltipFormat:
              graphOptions.value.timeFormat === '12h' ? 'MMM d, yyyy, hh:mm:ss a' : 'MMM d, yyyy, HH:mm:ss',
            displayFormats: {
              millisecond: graphOptions.value.timeFormat === '12h' ? 'hh:mm:ss.SSS a' : 'HH:mm:ss.SSS',
              second: graphOptions.value.timeFormat === '12h' ? 'hh:mm:ss a' : 'HH:mm:ss',
              minute: graphOptions.value.timeFormat === '12h' ? 'hh:mm a' : 'HH:mm',
              hour: graphOptions.value.timeFormat === '12h' ? 'hh:mm a' : 'HH:mm',
            },
          },
        },
      },
    },
  });

  // If there is a stored time range, trigger update
  if (selectedTimeRange.value) {
    updateTimeRange(selectedTimeRange.value);
  }
});

const csvConfig = mkConfig({
  useKeysAsHeaders: true,
  fieldSeparator: ';',
});

type ExportOrientation = 'column' | 'row';

type GenerateCsvOptions = {
  data?: Map<string, DataPoint[]>;
  sensorKeys?: string[];
  roundNumbers?: boolean;
  orientation?: ExportOrientation;
};

type AggregatedExportRecord = {
  formattedTime: string;
  values: Record<string, string | number>;
};

const getSensorExportLabel = (sensor: SensorData) => `${sensor.name}${sensor.unit ? ` (${sensor.unit})` : ''}`;

const formatDataPointValue = (sensor: SensorData, value: number | null | undefined, shouldRound: boolean) => {
  if (value === undefined || value === null) {
    return '';
  }

  if (!shouldRound) {
    return value;
  }

  const decimals = sensor.accuracyDecimals;
  if (decimals === undefined || decimals === null) {
    return value;
  }

  return roundNumber(value, decimals);
};

const generateCSVData = (options: GenerateCsvOptions = {}) => {
  const {
    data = dataPoints,
    sensorKeys = tickedNodes.value,
    roundNumbers = exportRoundNumbers.value,
    orientation = exportOrientation.value,
  } = options;

  const aggregatedData = new Map<string, AggregatedExportRecord>();

  const selectedSensorKeySet = new Set(sensorKeys);
  const orderedSensors = props.sensors.filter((sensor) => selectedSensorKeySet.has(getSensorUniqueId(sensor)));

  if (orderedSensors.length === 0) {
    toast.error(t('chart.export_select_sensor_error'));
    return null;
  }

  orderedSensors.forEach((sensor) => {
    const sensorKey = getSensorUniqueId(sensor);
    const sensorDataPoints = data.get(sensorKey) || [];
    sensorDataPoints.forEach((dataPoint) => {
      const timeKey = dataPoint.ts;
      if (!aggregatedData.has(timeKey)) {
        aggregatedData.set(timeKey, {
          formattedTime: format(new Date(dataPoint.ts), 'dd/MM/yyyy HH:mm:ss'),
          values: {},
        });
      }
      const record = aggregatedData.get(timeKey);
      if (!record) {
        return;
      }
      record.values[sensorKey] = formatDataPointValue(sensor, dataPoint.value, roundNumbers);
    });
  });

  const sortedTimeKeys = Array.from(aggregatedData.keys()).sort(
    (a, b) => new Date(a).getTime() - new Date(b).getTime(),
  );

  if (sortedTimeKeys.length === 0) {
    toast.error('No data to export');
    return null;
  }

  let formattedCsvData: Record<string, string | number>[] = [];

  if (orientation === 'row') {
    const sensorHeader = t('device.sensor');
    formattedCsvData = orderedSensors.map((sensor) => {
      const sensorKey = getSensorUniqueId(sensor);
      const row: Record<string, string | number> = {
        [sensorHeader]: getSensorExportLabel(sensor),
      };

      sortedTimeKeys.forEach((timeKey) => {
        const record = aggregatedData.get(timeKey);
        const header = record?.formattedTime ?? format(new Date(timeKey), 'dd/MM/yyyy HH:mm:ss');
        row[header] = record?.values[sensorKey] ?? '';
      });

      return row;
    });
  } else {
    formattedCsvData = sortedTimeKeys.map((timeKey) => {
      const record = aggregatedData.get(timeKey);
      const row: Record<string, string | number> = {
        Time: record?.formattedTime ?? format(new Date(timeKey), 'dd/MM/yyyy HH:mm:ss'),
      };

      orderedSensors.forEach((sensor) => {
        const sensorKey = getSensorUniqueId(sensor);
        row[getSensorExportLabel(sensor)] = record?.values[sensorKey] ?? '';
      });

      return row;
    });
  }

  if (formattedCsvData.length === 0) {
    toast.error('No data to export');
    return null;
  }

  return generateCsv(csvConfig)(formattedCsvData);
};

const intervalMilliseconds = computed(() => graphOptions.value.refreshInterval * 1000);
useInterval(() => {
  refresh();
}, intervalMilliseconds);

defineExpose({
  refresh,
});
</script>

<style lang="scss">
.options-btn {
  .q-icon {
    color: #757575 !important;
  }
}

.chart-wrapper {
  position: relative;
  height: 400px;
}

.chart-title {
  margin-bottom: 0px;
}
</style>
