<template>
  <q-form class="chart-form" @submit="emit('onSubmit')">
    <q-card-section class="q-pt-none column q-gutter-md">
      <div>
        <q-input
          v-model.number="graphOptions.refreshInterval"
          type="number"
          :label="t('global.automatic_refresh_interval')"
          :hint="t('global.enter_refresh_interval_s')"
        />
      </div>
      <div>
        <q-select
          v-model="graphOptions.timeFormat"
          :options="timeFormatOptions"
          :label="t('chart.time_format')"
          map-options
          emit-value
        ></q-select>
      </div>
      <div>
        <div class="form-subtitle text-primary">Graph Style</div>
        <div class="chart-form">
          <q-select
            v-model="graphOptions.interpolationMethod"
            :options="interpolationMethods"
            :label="t('chart.curve_type')"
            map-options
            emit-value
          ></q-select>
          <q-select
            v-model="graphOptions.lineStyle"
            :options="lineStyleOptions"
            :label="t('chart.graph_type')"
            map-options
            emit-value
          ></q-select>
          <q-input
            v-if="graphOptions.lineStyle === 'lines' || graphOptions.lineStyle === 'linesmarkers'"
            v-model.number="graphOptions.lineWidth"
            type="number"
            label="Line Width"
          />
          <q-input
            v-if="graphOptions.lineStyle === 'markers' || graphOptions.lineStyle === 'linesmarkers'"
            v-model.number="graphOptions.markerSize"
            type="number"
            label="Marker Size"
          />
        </div>
      </div>
      <div>
        <div class="form-subtitle text-primary">Sampling</div>
        <div class="chart-form">
          <q-select
            v-model="graphOptions.samplingOption"
            :options="samplingOptions"
            label="Sample"
            map-options
            emit-value
          ></q-select>
          <template v-if="graphOptions.samplingOption === 'DOWNSAMPLE'">
            <q-input
              v-model.number="graphOptions.downsampleResolution"
              type="number"
              :label="t('chart.maximum_values')"
            />
            <q-select
              v-model="graphOptions.downsampleMethod"
              :options="downsampleMethods"
              label="Downsample method"
              map-options
              emit-value
            ></q-select>
          </template>
          <template v-else-if="graphOptions.samplingOption === 'BUCKETS'">
            <q-select
              v-model="graphOptions.timeBucketSizeSeconds"
              :options="timeBuckets"
              label="Time bucket size"
              map-options
              emit-value
            ></q-select>
            <q-select
              v-model="graphOptions.timeBucketMethod"
              :options="timeBucketMethods"
              :label="t('chart.statistical_method')"
              map-options
              emit-value
            ></q-select>
          </template>
        </div>
      </div>
    </q-card-section>
    <q-card-actions align="right" class="text-primary">
      <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
      <q-btn unelevated color="primary" :label="t('global.save')" type="submit" no-caps padding="6px 20px" />
    </q-card-actions>
  </q-form>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import { useI18n } from 'vue-i18n';

export type GraphOptions = {
  refreshInterval: number;
  timeFormat: '12h' | '24h';

  interpolationMethod: 'straight' | 'bezier';
  lineStyle: string;
  lineWidth: number;
  markerSize: number;

  samplingOption: 'DOWNSAMPLE' | 'BUCKETS' | 'RAW';
  downsampleResolution: number;
  downsampleMethod: 'Asap' | 'Lttb';
  timeBucketSizeSeconds: number;
  timeBucketMethod: 'Average' | 'Sum';
};

const { t } = useI18n();

const timeFormatOptions = [
  { value: '12h', label: '12 hours' },
  { value: '24h', label: '24 hours' },
];

const samplingOptions = [
  { value: 'DOWNSAMPLE', label: 'Downsample' },
  { value: 'BUCKETS', label: 'Time buckets' },
  { value: 'RAW', label: 'Raw data' },
];

const downsampleMethods = [
  { value: 'Lttb', label: 'Largest Triangle Three Buckets (LTTB)' },
  { value: 'Asap', label: 'ASAP Smoothing' },
];

const timeBucketMethods = [
  { value: 'Average', label: 'Average' },
  { value: 'Max', label: 'Max' },
  { value: 'Min', label: 'Min' },
  { value: 'Sum', label: 'Sum' },
  { value: 'StdDev', label: 'Standard Deviation' },
];

const timeBuckets = [
  { value: 1, label: '1 second' },
  { value: 5, label: '5 seconds' },
  { value: 10, label: '10 seconds' },
  { value: 30, label: '30 seconds' },
  { value: 60, label: '1 minute' },
  { value: 300, label: '5 minutes' },
  { value: 600, label: '10 minutes' },
  { value: 900, label: '15 minutes' },
  { value: 1800, label: '30 minutes' },
  { value: 3600, label: '1 hour' },
  { value: 7200, label: '2 hours' },
  { value: 14400, label: '4 hours' },
  { value: 21600, label: '6 hours' },
  { value: 43200, label: '12 hours' },
  { value: 86400, label: '1 day' },
];

const interpolationMethods = [
  { value: 'straight', label: t('chart.curve_options.straight') },
  { value: 'bezier', label: t('chart.curve_options.bezier') },
];
const lineStyleOptions = [
  { value: 'lines', label: t('chart.graph_options.lines') },
  { value: 'markers', label: t('chart.graph_options.markers') },
  { value: 'linesmarkers', label: t('chart.graph_options.lines_markers') },
];

const graphOptions = defineModel<GraphOptions>({
  type: Object as PropType<GraphOptions>,
  required: true,
});

const emit = defineEmits(['onSubmit']);
</script>

<style scoped>
.chart-form {
  display: flex;
  flex-wrap: wrap;
  flex-direction: column;
  gap: 1rem;
}

.form-subtitle {
  font-size: 1rem;
  font-weight: 450;
  margin-top: 0.5rem;
  margin-bottom: 0.5rem;
}
</style>
