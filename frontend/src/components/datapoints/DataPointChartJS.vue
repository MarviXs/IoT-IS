<template>
  <div id="chart">
    <div class="row items-center justify-start q-mb-md q-gutter-x-md q-gutter-y-sm">
      <p class="text-weight-medium text-h6 chart-title">{{ t('chart.label') }}</p>
      <q-space></q-space>
      <chart-time-range-select
        class="col-grow col-lg-auto"
        ref="timeRangeSelectRef"
        @update:model-value="updateTimeRange"
      ></chart-time-range-select>
      <q-btn
        padding="0.5rem 1rem"
        outline
        no-caps
        color="grey-7"
        text-color="grey-5"
        class="options-btn col-grow col-lg-auto"
        @click="resetZoom"
      >
        <template #default>
          <div class="text-grey-10 text-weight-regular">Reset Zoom</div>
        </template>
      </q-btn>
      <q-btn
        padding="0.5rem 1rem"
        outline
        no-caps
        color="grey-7"
        text-color="grey-5"
        class="options-btn col-grow col-lg-auto"
        :icon="mdiRefresh"
        @click="refresh"
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
        @click="isGraphOptionsDialogOpen = true"
      >
        <template #default>
          <div class="text-grey-10 text-weight-regular">
            {{ t('global.options') }}
          </div>
        </template>
      </q-btn>
    </div>
    <div class="chart-wrapper">
      <canvas id="chart" ref="chartRef" class="chart"></canvas>
    </div>
    <dialog-common
      v-model="isGraphOptionsDialogOpen"
      :action-label="t('global.refresh')"
      @on-submit="isGraphOptionsDialogOpen = false"
    >
      <template #title>{{ t('global.options') }}</template>
      <template #default>
        <GraphOptionsForm v-model="graphOptions" />
      </template>
    </dialog-common>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, PropType, shallowRef, watch, watchEffect, computed } from 'vue';
import ChartTimeRangeSelect from '@/components/datapoints/ChartTimeRangeSelect.vue';
import { TimeRange } from '@/models/TimeRange';
import { getGraphColor, transparentize } from '@/utils/colors';
import { useStorage } from '@vueuse/core';
import { useI18n } from 'vue-i18n';
import DialogCommon from '../core/DialogCommon.vue';
import GraphOptionsForm, { GraphOptions } from './GraphOptionsForm.vue';
import { GetDataPointsQuery } from '@/api/services/DataPointService';
import { DataPoint } from '@/models/DataPoint';
import DataPointService from '@/api/services/DataPointService';
import { Chart, ChartDataset, ChartEvent, LegendItem, ScaleOptions } from 'chart.js/auto';
import 'chartjs-adapter-date-fns';
import zoomPlugin from 'chartjs-plugin-zoom';
import { mdiRefresh } from '@quasar/extras/mdi-v7';
import { useInterval } from '@/composables/useInterval';

Chart.register(zoomPlugin);

// Hack to allow grouping of datasets by unit
type DataSetCustom = ChartDataset<'line', { x: string; y?: number | null }[]> & {
  sensorKey: string;
};

export type SensorData = {
  id: string;
  deviceId: string;
  tag: string;
  name: string;
  unit?: string | null;
  accuracyDecimals?: number | null;
  lastValue?: number | null;
};

const props = defineProps({
  sensors: {
    type: Array as PropType<SensorData[]>,
    required: true,
  },
});

const tickedNodes = defineModel('tickedNodes', {
  type: Array as PropType<string[]>,
  default: [],
});

const isGraphOptionsDialogOpen = ref(false);
const graphOptions = useStorage<GraphOptions>('graphOptions', {
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

const { t } = useI18n();

const timeRangeSelectRef = ref();
const selectedTimeRange = ref<TimeRange>();

const chartRef = ref<HTMLCanvasElement>();
const chart = shallowRef<Chart<'line', { x: string; y?: number | null }[]>>();

function getSensorUniqueId(sensor: SensorData) {
  return `${sensor.deviceId}-${sensor.tag}`;
}

const currentXMin = ref<string>();
const currentXMax = ref<string>();
async function updateTimeRange(timeRange: TimeRange) {
  selectedTimeRange.value = timeRange;

  currentXMin.value = timeRange.from;
  currentXMax.value = timeRange.to;

  await getDataPoints();
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
}

function roundNumber(num: number | undefined | null, decimals: number) {
  if (num === undefined || num === null) return num;
  const factor = 10 ** decimals;
  return Math.round(num * factor) / factor;
}

function updateChartData() {
  if (!chart.value) return;
  const isZoomed = chart.value.isZoomedOrPanned();

  const datasets: DataSetCustom[] = [];
  const uniqueUnits = new Map<string, { key: string; color: string }>();
  const yScales: { [key: string]: ScaleOptions } = {};

  props.sensors.forEach((sensor, index) => {
    const key = getSensorUniqueId(sensor);
    const unit = sensor.unit ?? '';

    if (!uniqueUnits.has(unit)) {
      uniqueUnits.set(unit, { key, color: getGraphColor(index) });
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
      backgroundColor: transparentize(getGraphColor(index), 0.5),
      borderColor: getGraphColor(index),
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
    const { key, color } = unitInfo;
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
          return `${roundNumber(value, props.sensors[index].accuracyDecimals ?? 2)} ${unit}`;
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

  if (chart.value.options.scales.x && !isZoomed) {
    chart.value.options.scales.x.min = selectedTimeRange.value?.from;
    chart.value.options.scales.x.max = selectedTimeRange.value?.to;
  }

  Object.assign(chart.value.options.scales, yScales);

  chart.value.update();
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

watchEffect(() => {
  updateDatasetVisibility(tickedNodes.value);
});

watch(
  () => isGraphOptionsDialogOpen.value,
  () => {
    if (!isGraphOptionsDialogOpen.value) {
      getDataPoints();
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
});

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
