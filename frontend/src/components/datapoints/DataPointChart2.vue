<template>
  <div id="chart">
    <div class="row items-center justify-start q-mb-md q-gutter-x-md">
      <p class="text-weight-medium text-h6">{{ t('chart.label') }}</p>
      <q-space></q-space>
      <chart-time-range-select ref="timeRangeSelect" @update:model-value="updateTimeRange"></chart-time-range-select>
      <q-btn
        padding="0.5rem 1rem"
        outline
        no-caps
        color="grey-7"
        text-color="grey-5"
        class="options-btn"
        :icon="mdiRefresh"
        @click="onClickRefresh"
      >
        <template #default>
          <div class="text-grey-10 text-weight-regular q-ml-sm">
            {{ t('global.refresh') }}
          </div>
        </template>
      </q-btn>

      <!-- <q-btn
        padding="0.5rem 1rem"
        outline
        no-caps
        color="grey-7"
        text-color="grey-5"
        class="options-btn"
        @click="download(csvConfig)(generateCSVData())"
      >
        <div class="text-grey-9">{{ t('chart.export_csv') }}</div>
      </q-btn> -->

      <q-btn
        padding="0.5rem 1rem"
        outline
        no-caps
        color="grey-7"
        text-color="grey-5"
        class="options-btn"
        @click="optionsDialogOpen = true"
      >
        <template #default>
          <div class="text-grey-10 text-weight-regular">
            {{ t('global.options') }}
          </div>
        </template>
      </q-btn>
    </div>
    <div id="chart" ref="chartRef"></div>
    <dialog-common
      v-model="optionsDialogOpen"
      :action-label="t('global.refresh')"
      @on-submit="optionsDialogOpen = false"
    >
      <template #title>{{ t('global.options') }}</template>
      <template #default>
        <GraphOptionsForm v-model="graphOptions" />
      </template>
    </dialog-common>
  </div>
</template>

<script setup lang="ts">
import { PropType, computed, nextTick, onMounted, ref, watch } from 'vue';
import ChartTimeRangeSelect from '@/components/datapoints/ChartTimeRangeSelect.vue';
import { TimeRange } from '@/models/TimeRange';
import { graphColors } from '@/utils/colors';
import { now, useStorage } from '@vueuse/core';
import { useI18n } from 'vue-i18n';
import DialogCommon from '../core/DialogCommon.vue';
import { mdiRefresh } from '@quasar/extras/mdi-v7';
import GraphOptionsForm, { GraphOptions } from './GraphOptionsForm.vue';
import { Sensor } from '@/api/types/Sensor';
import { GetDataPointsQuery } from '@/api/types/DataPoint';
import { DataPoint } from '@/models/DataPoint';
import DataPointService from '@/api/services/DataPointService';
import { ApexOptions } from 'apexcharts';
import ApexCharts from 'apexcharts';

export type SensorData = {
  id: string;
  deviceId: string;
  tag: string;
  name: string;
  unit?: string | null;
  accuracyDecimals?: number | null;
};

const props = defineProps({
  sensors: {
    type: Array as PropType<SensorData[]>,
    required: true,
  },
});

// Data for sensors
const dataPoints = ref<Record<string, DataPoint[]>>({});

const { t, locale } = useI18n();

const selectedTimeRange = ref<TimeRange>();
const timeRangeSelect = ref();

function getSensorUniqueId(sensor: SensorData) {
  return `${sensor.deviceId}-${sensor.tag}`;
}

const series = computed<ApexOptions['series']>(() => {
  return props.sensors?.map((sensor, index) => ({
    name: getSensorUniqueId(sensor),
    color: graphColors[index],
    data:
      dataPoints.value[getSensorUniqueId(sensor)]?.map((dataPoint) => ({
        x: new Date(dataPoint.timeStamp).getTime(),
        y: dataPoint.value,
      })) ?? [],
  }));
});

const yaxisOptions = computed(() => {
  const shownUnits = new Set();
  const options: ApexYAxis[] = [];

  props.sensors.forEach((sensor, index) => {
    if (!shownUnits.has(sensor.unit)) {
      shownUnits.add(sensor.unit);
      options.push({
        seriesName: props.sensors?.filter((t) => t.unit === sensor.unit).map((t) => getSensorUniqueId(t)) ?? [],
        tickAmount: 5,
        labels: {
          formatter: (val: number) => `${val.toFixed(sensor.accuracyDecimals ?? 2)} ${sensor.unit}`,
          style: {
            colors: graphColors[index],
          },
        },
      });
    }
  });

  return options;
});

async function updateTimeRange(timeRange: TimeRange) {
  selectedTimeRange.value = timeRange;
  const newMin = new Date(timeRange.from).getTime();
  const newMax = new Date(timeRange.to).getTime();

  chartOptions.value.xaxis!.min = newMin;
  chartOptions.value.xaxis!.max = newMax;

  await refreshStoredData();
}

async function refreshStoredData() {
  if (props.sensors.length === 0) return;
  const from = new Date(selectedTimeRange.value?.from ?? 0);

  const zoomTo = lastZoom.value?.[1] ?? Date.now();
  const selectedTimeRangeTo = new Date(selectedTimeRange.value?.to ?? Date.now());

  const to = new Date(zoomTo > selectedTimeRangeTo.getTime() ? zoomTo : selectedTimeRangeTo.getTime());

  const promises = props.sensors.map(async (sensor) => {
    if (!sensor.id) return;

    const query: GetDataPointsQuery = {
      From: from.toISOString(),
      To: to.toISOString(),
      Downsample: graphOptions.value.downsampleResolution,
    };
    const { data, error } = await DataPointService.getDataPoints(sensor.deviceId, sensor.tag, query);

    if (error) {
      console.error(error);
      return;
    }
    dataPoints.value[getSensorUniqueId(sensor)] = data ?? [];
  });
  await Promise.all(promises);

  if (chart.value && series.value) {
    await chart.value.updateOptions(chartOptions.value);
  }

  // Zoom back after data refresh
  if (chart.value && lastZoom.value) {
    chart.value.zoomX(lastZoom.value[0], lastZoom.value[1]);
  }
}

async function onClickRefresh() {
  await refreshStoredData();
}

const optionsDialogOpen = ref(false);

const graphOptions = ref<GraphOptions>({
  refreshInterval: 0,
  samplingOption: 1,
  downsampleResolution: 99999999,
  downsampleMethod: 'Lttb',
  timeBucketSizeSeconds: 60,
  timeBucketMethod: 'Average',
  interpolationMethod: 'straight',
  lineStyle: 'lines',
});

const markerSize = computed(() => {
  if (graphOptions.value.lineStyle === 'markers' || graphOptions.value.lineStyle === 'linesmarkers') {
    return 5;
  }
  return 0;
});

const strokeWidth = computed(() => {
  if (graphOptions.value.lineStyle === 'lines' || graphOptions.value.lineStyle === 'linesmarkers') {
    return 5;
  }

  return 0;
});

const lastZoom = ref<[number, number] | null>();
function beforeResetZoom() {
  lastZoom.value = null;
}
function onChartZoomed(chartContext: unknown, { xaxis }: { xaxis: { min: number; max: number } }) {
  lastZoom.value = [xaxis.min, xaxis.max];
}

const chartOptions = computed<ApexOptions>(() => ({
  chart: {
    type: 'line',
    height: 350,
    zoom: {
      type: 'x',
      enabled: true,
    },
    toolbar: {
      autoSelected: 'zoom',
      tools: {
        download: false,
      },
    },
    animations: {
      enabled: false,
    },
  },
  dataLabels: {
    enabled: false,
  },
  stroke: {
    curve: graphOptions.value.interpolationMethod,
    width: strokeWidth.value,
  },
  markers: {
    size: markerSize.value,
  },
  legend: {
    position: 'bottom',
    horizontalAlign: 'center',
    formatter: function (seriesName: string, opts: { seriesIndex: number }) {
      return `${props.sensors[opts.seriesIndex].name} (${props.sensors[opts.seriesIndex].unit})2`;
    },
  },
  series: series.value,
  yaxis: yaxisOptions.value,
  xaxis: {
    type: 'datetime',
    labels: {
      datetimeUTC: false,
    },
    min: new Date(selectedTimeRange.value?.from ?? now()).getTime(),
    max: new Date(selectedTimeRange.value?.to ?? now()).getTime(),
  },
  tooltip: {
    shared: false,

    y: props.sensors.map((sensor) => ({
      formatter: function (val: number) {
        return `${val.toFixed(sensor.accuracyDecimals ?? 2)} ${sensor.unit}`;
      },

      title: {
        formatter: function () {
          return sensor.name;
        },
      },
    })),
    x: {
      show: true,
      formatter: function (val: number) {
        return new Date(val).toLocaleString(locale.value);
      },
    },
  },
}));

const chartRef = ref<HTMLDivElement>();
const chart = ref<ApexCharts>();

const initApexCharts = async () => {
  chart.value = new ApexCharts(chartRef.value, chartOptions.value);
  chart.value.render();
};

onMounted(async () => {
  initApexCharts();
});
</script>

<style lang="scss">
.options-btn {
  .q-icon {
    color: #757575 !important;
  }
}
</style>
