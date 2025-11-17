<template>
  <PageLayout :breadcrumbs="breadcrumbs">
    <div class="column q-gutter-y-lg">
      <q-card class="shadow bg-white q-pa-lg">
        <div class="text-subtitle1 text-weight-medium q-mb-md">
          {{ t('job.compare.description') }}
        </div>
        <div class="row q-col-gutter-md">
          <div class="col-12 col-md-4">
            <q-input
              v-model.number="jobCount"
              type="number"
              dense
              outlined
              :label="t('job.compare.count_label')"
              :min="1"
              :hint="t('job.compare.count_hint')"
            />
          </div>
          <div class="col-12 col-md-8">
            <q-select
              v-model="sensorTag"
              dense
              outlined
              emit-value
              map-options
              :options="sensorOptions"
              :label="t('job.compare.sensor_label')"
              :disable="sensorOptions.length === 0"
              option-label="label"
              option-value="value"
            />
          </div>
        </div>
      </q-card>

      <q-skeleton v-if="isLoadingDevice" class="bg-white shadow" height="120px" square />
      <q-banner v-else-if="!device" class="bg-negative text-white shadow" dense rounded>
        {{ t('device.toasts.loading_failed') }}
      </q-banner>
      <template v-else>
        <q-banner v-if="sensorOptions.length === 0" class="bg-warning text-white shadow" dense rounded>
          {{ t('job.compare.no_sensors') }}
        </q-banner>
        <q-banner v-else-if="comparisonError" class="bg-negative text-white shadow" dense rounded>
          {{ comparisonError }}
        </q-banner>
        <q-card v-else class="shadow bg-white q-pa-lg">
          <div class="text-h6 text-weight-medium q-mb-md">
            {{ t('job.compare.chart_title') }}
          </div>
          <div v-if="isLoadingComparison">
            <q-skeleton height="360px" square />
          </div>
          <div v-else-if="comparisonSeries.length === 0" class="text-grey-7 text-body2">
            {{ t('job.compare.no_data') }}
          </div>
          <JobComparisonChart
            v-else
            :series="comparisonSeries"
            :x-label="t('job.compare.x_axis')"
            :y-label="yAxisLabel"
          />
        </q-card>
      </template>
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import DeviceService from '@/api/services/DeviceService';
import type { DeviceResponse } from '@/api/services/DeviceService';
import JobService from '@/api/services/JobService';
import type { JobsQueryParams, JobsResponse } from '@/api/services/JobService';
import DataPointService from '@/api/services/DataPointService';
import type { GetDataPointsQuery } from '@/api/services/DataPointService';
import JobComparisonChart from '@/components/jobs/JobComparisonChart.vue';
import type { JobComparisonSeries } from '@/models/JobComparison';
import { handleError } from '@/utils/error-handler';
import { getGraphColor } from '@/utils/colors';

const { t } = useI18n();
const route = useRoute();
const deviceId = route.params.id as string;

const device = ref<DeviceResponse>();
const isLoadingDevice = ref(false);

const jobCount = ref(10);
const sensorTag = ref<string>('');
const comparisonSeries = ref<JobComparisonSeries[]>([]);
const isLoadingComparison = ref(false);
const comparisonError = ref<string | null>(null);
let currentComparisonRequest = 0;

const sensors = computed(() => device.value?.deviceTemplate?.sensors ?? []);

const sensorOptions = computed(() =>
  sensors.value.map((sensor) => ({
    label: sensor.name ?? sensor.tag,
    value: sensor.tag,
    unit: sensor.unit,
  })),
);

const selectedSensor = computed(() => sensors.value.find((sensor) => sensor.tag === sensorTag.value));

const breadcrumbs = computed(() => [
  { label: t('device.label', 2), to: '/devices' },
  { label: device.value?.name ?? t('global.loading'), to: `/devices/${deviceId}` },
  { label: t('job.label', 2), to: `/devices/${deviceId}/jobs` },
  { label: t('job.compare.title') },
]);

const yAxisLabel = computed(() => {
  if (!selectedSensor.value) {
    return t('job.compare.value_label');
  }

  if (selectedSensor.value.unit) {
    return `${selectedSensor.value.name ?? selectedSensor.value.tag} (${selectedSensor.value.unit})`;
  }

  return selectedSensor.value.name ?? selectedSensor.value.tag;
});

watch(
  sensors,
  (value) => {
    if (!value.length) {
      sensorTag.value = '';
      return;
    }
    if (!sensorTag.value) {
      sensorTag.value = value[0].tag;
    }
  },
  { immediate: true },
);

watch(
  () => jobCount.value,
  (value, oldValue) => {
    if (!Number.isFinite(value) || value <= 0) {
      jobCount.value = oldValue && Number.isFinite(oldValue) && oldValue > 0 ? oldValue : 1;
    }
  },
);

watch(
  [() => jobCount.value, () => sensorTag.value],
  () => {
    if (!device.value || !sensorTag.value) {
      return;
    }
    loadComparison();
  },
);

async function loadDevice() {
  if (!deviceId) {
    return;
  }

  isLoadingDevice.value = true;
  const { data, error } = await DeviceService.getDevice(deviceId);
  isLoadingDevice.value = false;
  if (error || !data) {
    handleError(error, t('device.toasts.loading_failed'));
    return;
  }

  device.value = data;
}
loadDevice();

async function loadComparison() {
  if (!device.value || !sensorTag.value) {
    return;
  }

  const requestId = ++currentComparisonRequest;
  isLoadingComparison.value = true;
  comparisonError.value = null;

  const query: JobsQueryParams = {
    DeviceId: deviceId,
    SortBy: 'CreatedAt',
    Descending: true,
    PageNumber: 1,
    PageSize: jobCount.value,
  };

  const { data, error } = await JobService.getJobs(query);
  if (requestId !== currentComparisonRequest) {
    return;
  }

  if (error || !data) {
    handleError(error, t('job.compare.error_loading'));
    comparisonError.value = t('job.compare.error_loading');
    comparisonSeries.value = [];
    isLoadingComparison.value = false;
    return;
  }

  const jobs = (data.items ?? []).filter((job) => job.startedAt);
  if (!jobs.length) {
    comparisonSeries.value = [];
    isLoadingComparison.value = false;
    return;
  }

  const series: JobComparisonSeries[] = [];
  for (let index = 0; index < jobs.length; index += 1) {
    const job = jobs[index];
    const jobSeries = await buildSeries(job, index);
    if (requestId !== currentComparisonRequest) {
      return;
    }
    if (jobSeries) {
      series.push(jobSeries);
    }
  }

  comparisonSeries.value = series;
  isLoadingComparison.value = false;
}

async function buildSeries(job: NonNullable<JobsResponse['items']>[number], colorIndex: number) {
  if (!job.startedAt) {
    return null;
  }

  const startDate = new Date(job.startedAt);
  const endDate = job.finishedAt ? new Date(job.finishedAt) : new Date();

  if (Number.isNaN(startDate.getTime()) || Number.isNaN(endDate.getTime())) {
    return null;
  }

  const query: GetDataPointsQuery = {
    From: startDate.toISOString(),
    To: endDate.toISOString(),
  };

  const { data, error } = await DataPointService.getDataPoints(deviceId, sensorTag.value, query);
  if (error) {
    handleError(error, t('job.compare.error_loading'));
    comparisonError.value = t('job.compare.error_loading');
    comparisonSeries.value = [];
    isLoadingComparison.value = false;
    return null;
  }

  const points = (data ?? []).map((point) => ({
    x: new Date(point.ts).getTime() - startDate.getTime(),
    y: typeof point.value === 'number' ? point.value : null,
  }));

  if (!points.length) {
    return null;
  }

  return {
    jobId: job.id,
    label: `${job.name} (${formatDate(job.startedAt)})`,
    color: getGraphColor(colorIndex),
    data: points,
  };
}

function formatDate(value?: string | null) {
  if (!value) {
    return '';
  }
  return new Date(value).toLocaleString();
}
</script>

<style scoped>
.q-card :deep(.q-field__bottom) {
  padding-bottom: 0;
}
</style>
