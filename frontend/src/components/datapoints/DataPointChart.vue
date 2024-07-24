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
    <apexchart
      ref="chart"
      height="350"
      width="100%"
      type="line"
      :options="chartOptions"
      :series="series"
      @mounted="setChartUpdate"
      @legend-click="legendClick"
      @before-reset-zoom="beforeResetZoom"
      @zoomed="onChartZoomed"
    ></apexchart>
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
import { PropType, computed, nextTick, ref, watch } from 'vue';
import ChartTimeRangeSelect from '@/components/datapoints/ChartTimeRangeSelect.vue';
import { TimeRange } from '@/models/TimeRange';
import { graphColors } from '@/utils/colors';
import { now, useStorage } from '@vueuse/core';
import { useI18n } from 'vue-i18n';
import DialogCommon from '../core/DialogCommon.vue';
import { mdiRefresh } from '@quasar/extras/mdi-v6';
import GraphOptionsForm, { GraphOptions } from './GraphOptionsForm.vue';
import { Sensor } from '@/api/types/Sensor';
import { GetDataPointsQuery } from '@/api/types/DataPoint';
import { DataPoint } from '@/models/DataPoint';
import DataPointService from '@/api/services/DataPointService';
import { ApexOptions } from 'apexcharts';

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

const { t } = useI18n();

const selectedTimeRange = ref<TimeRange>();
const timeRangeSelect = ref();

const series = computed(() => {
  return props.sensors?.map((tag, index) => ({
    name: `${tag.name} (${tag.unit})`,
    unit: tag.unit,
    accuracyDecimals: tag.accuracyDecimals,
    id: tag.id,
    color: graphColors[index],
    data: dataPoints.value[tag.id]?.map((point) => [new Date(point.timeStamp).getTime(), point.value]) ?? [],
  }));
});

const yaxisOptions = computed(() => {
  const shownUnits = new Set();
  const options: ApexYAxis[] = [];

  series.value.forEach((tag) => {
    const isVisible = tickedNodes.value?.includes(tag.id ?? '');
    if (isVisible && !shownUnits.has(tag.unit)) {
      shownUnits.add(tag.unit);

      options.push({
        seriesName: series.value.filter((t) => t.unit === tag.unit).map((t) => t.name),
        tickAmount: 1,
        labels: {
          formatter: (val: number) => `${val.toFixed(tag.accuracyDecimals ?? 2)} ${tag.unit}`,
          style: {
            colors: tag.color,
          },
        },
      });
    }
  });

  return options;
});

const chart = ref<ApexCharts | null>(null);
async function updateTimeRange(timeRange: TimeRange) {
  lastZoom.value = null;
  selectedTimeRange.value = timeRange;
  const newMin = new Date(timeRange.from).getTime();
  const newMax = new Date(timeRange.to).getTime();

  if (chart.value) {
    chart.value.updateOptions({
      xaxis: {
        min: newMin,
        max: newMax,
      },
    });
  } else {
    chartOptions.value.xaxis!.min = newMin;
    chartOptions.value.xaxis!.max = newMax;
  }
  await refreshStoredData();
}

async function onClickRefresh() {
  if (!lastZoom.value) {
    timeRangeSelect.value?.emitUpdate();
  } else if (lastZoom.value && lastZoom.value[1] == chartOptions.value.xaxis!.max) {
    lastZoom.value = null;
    timeRangeSelect.value?.emitUpdate();
  } else {
    await refreshStoredData();
  }
}

async function refreshStoredData() {
  if (props.sensors.length === 0) return;
  const from = new Date(selectedTimeRange.value?.from ?? 0);

  const zoomTo = lastZoom.value?.[1] ?? Date.now();
  const selectedTimeRangeTo = new Date(selectedTimeRange.value?.to ?? Date.now());

  const to = new Date(zoomTo > selectedTimeRangeTo.getTime() ? zoomTo : selectedTimeRangeTo.getTime());

  const promises = props.sensors.map((tag) => {
    if (!tag.id) return;

    const query: GetDataPointsQuery = {
      From: from.toISOString(),
      To: to.toISOString(),
      Downsample: graphOptions.value.downsampleResolution,
    };
    return DataPointService.getDataPoints(tag.deviceId, tag.tag, query);
  });
  const results = await Promise.all(promises);

  disableChartUpdate();

  results.forEach((result, index) => {
    if (!result) return;
    const { data, error } = result;
    if (error) {
      console.error(error);
      return;
    }
    dataPoints.value[props.sensors[index].id] = data;
  });

  enableChartUpdate();

  await updateSeriesVisibility(props.sensors, tickedNodes.value ?? []);

  // Zoom back after data refresh
  if (chart.value && lastZoom.value) {
    chart.value?.zoomX(lastZoom.value[0], lastZoom.value[1]);
  }
}

//Legend control
const tickedNodes = defineModel<string[]>('tickedNodes');
const seriesVisibility = ref<boolean[]>(props.sensors.map(() => true));
const chartCnt = ref();
const chartUpdate = ref();
function setChartUpdate(chartContext: { update: unknown }) {
  chartCnt.value = chartContext;
  chartUpdate.value = chartContext.update;
}

function enableChartUpdate() {
  if (!chartCnt.value) return;
  chartCnt.value.update = chartUpdate.value;
  chartCnt.value.update();
}

function disableChartUpdate() {
  if (!chartCnt.value) return;
  chartCnt.value.update = () => {
    return Promise.resolve();
  };
}

function legendClick(_chartContext: unknown, seriesIndex: number) {
  seriesVisibility.value[seriesIndex] = !seriesVisibility.value[seriesIndex];
  tickedNodes.value = props.sensors.filter((_, index) => seriesVisibility.value[index]).map((tag) => tag.id);
}

// Zoom for chart update, to keep the zoom after data refresh
const lastZoom = ref<[number, number] | null>();
function beforeResetZoom() {
  lastZoom.value = null;
}
function onChartZoomed(chartContext: unknown, { xaxis }: { xaxis: { min: number; max: number } }) {
  lastZoom.value = [xaxis.min, xaxis.max];
}

function areAllSeriesVisible(newTicked: string[]) {
  if (!tickedNodes.value) return false;
  const previousAllVisible = props.sensors.every((tag) => tickedNodes.value?.includes(tag.id));
  const newAllVisible = props.sensors.every((tag) => newTicked?.includes(tag.id));
  const seriesVisibilityAll = seriesVisibility.value.every((visible) => visible);

  return previousAllVisible && newAllVisible && seriesVisibilityAll;
}

async function updateSeriesVisibility(tags: Sensor[], ticked: string[]) {
  if (!chartCnt.value) return;

  // For performance, we check if all series are visible and if so, we don't update the chart
  if (areAllSeriesVisible(ticked)) return;

  await nextTick();
  disableChartUpdate();

  if (chart.value) {
    tags.forEach((tag) => {
      const seriesName = `${tag.name} (${tag.unit})`;
      if (ticked.includes(tag.id)) {
        chart.value?.showSeries(seriesName);
        seriesVisibility.value[tags.indexOf(tag)] = true;
      } else {
        chart.value?.hideSeries(seriesName);
        seriesVisibility.value[tags.indexOf(tag)] = false;
      }
    });
  }
  enableChartUpdate();
}

const optionsDialogOpen = ref(false);

const graphOptions = ref<GraphOptions>({
  refreshInterval: 0,
  samplingOption: 1,
  downsampleResolution: 1000,
  downsampleMethod: 'Lttb',
  timeBucketSizeSeconds: 60,
  timeBucketMethod: 'Average',
  interpolationMethod: 'smooth',
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

watch(
  () => tickedNodes.value,
  async (newTicked) => {
    updateSeriesVisibility(props.sensors, newTicked ?? []);
  },
);

watch(
  () => props.sensors,
  async () => {
    await onClickRefresh();
  },
);

watch(
  () => optionsDialogOpen.value,
  (isOpen) => {
    if (isOpen) {
      disableChartUpdate();
    } else {
      onClickRefresh();
    }
  },
);

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
  },
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
    y: props.sensors?.map((tag) => ({
      formatter: function (val: number) {
        return `${val?.toFixed(tag.accuracyDecimals ?? 2) ?? 'NaN'} ${tag.unit}`;
      },
    })),
    x: {
      formatter: function (val: number) {
        return new Date(val).toLocaleString();
      },
    },
  },
}));
</script>

<style lang="scss">
.options-btn {
  .q-icon {
    color: #757575 !important;
  }
}
</style>
