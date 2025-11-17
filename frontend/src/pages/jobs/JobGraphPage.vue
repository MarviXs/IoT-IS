<template>
  <PageLayout :breadcrumbs="breadcrumbs">
    <template #default>
      <div class="column q-gutter-y-lg">
        <q-skeleton
          v-if="isLoadingJob || (needsDeviceFetch && isLoadingDevice)"
          class="bg-white shadow"
          height="120px"
          square
        />
        <template v-else-if="job">
          <q-card class="shadow bg-white q-pa-lg">
            <div class="text-h6 q-mb-sm">{{ job.name }}</div>
            <div class="text-body2">
              {{ t('job.started_at') }}:
              <span class="text-weight-medium">{{ formatDate(jobStartedAt) }}</span>
            </div>
            <div class="text-body2 q-mt-xs">
              {{ t('job.finished_at') }}:
              <span class="text-weight-medium">
                {{ jobFinishedAt ? formatDate(jobFinishedAt) : t('job.not_finished') }}
              </span>
            </div>
          </q-card>
          <q-banner v-if="!initialCustomTimeRange" class="bg-warning text-white shadow" dense rounded>
            {{ t('job.graph_missing_range') }}
          </q-banner>
          <q-banner v-else-if="sensors.length === 0" class="bg-warning text-white shadow" dense rounded>
            {{ t('job.graph_no_sensors') }}
          </q-banner>
          <DataPointChartJS
            v-else
            class="bg-white shadow q-pa-lg"
            :sensors="sensors"
            :initial-custom-time-range="initialCustomTimeRange"
            :time-range-storage-key="`job-graph`"
          />
        </template>
        <q-banner v-else class="bg-negative text-white shadow" dense rounded>
          {{ t('job.graph_load_failed') }}
        </q-banner>
      </div>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { format } from 'date-fns';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import DataPointChartJS from '@/components/datapoints/DataPointChartJS.vue';
import JobService from '@/api/services/JobService';
import DeviceService from '@/api/services/DeviceService';
import { handleError } from '@/utils/error-handler';
import type { JobResponse } from '@/api/services/JobService';
import type { DeviceResponse } from '@/api/services/DeviceService';
import type { SensorData } from '@/models/SensorData';

const { t } = useI18n();
const route = useRoute();

const jobId = computed(() => route.params.id?.toString() ?? '');
type JobWithTimestamps = JobResponse & {
  startedAt?: string | null;
  finishedAt?: string | null;
};
const job = ref<JobWithTimestamps>();
const device = ref<DeviceResponse>();

const isLoadingJob = ref(false);
const isLoadingDevice = ref(false);

const jobStartedAt = computed(() => job.value?.startedAt ?? undefined);
const jobFinishedAt = computed(() => job.value?.finishedAt ?? undefined);

const needsDeviceFetch = computed(() => !!job.value?.deviceId && !device.value);

const initialCustomTimeRange = computed(() => {
  if (!jobStartedAt.value || !jobFinishedAt.value) {
    return undefined;
  }

  const fromDate = new Date(jobStartedAt.value);
  const toDate = new Date(jobFinishedAt.value);

  if (Number.isNaN(fromDate.getTime()) || Number.isNaN(toDate.getTime())) {
    return undefined;
  }

  return {
    from: format(fromDate, 'yyyy-MM-dd HH:mm:ss.SSS'),
    to: format(toDate, 'yyyy-MM-dd HH:mm:ss.SSS'),
  };
});

const sensors = computed<SensorData[]>(() => {
  if (!device.value?.deviceTemplate?.sensors) {
    return [];
  }

  return device.value.deviceTemplate.sensors.map((sensor) => ({
    id: sensor.id,
    deviceId: device.value?.id ?? '',
    tag: sensor.tag,
    name: sensor.name,
    unit: sensor.unit,
    accuracyDecimals: sensor.accuracyDecimals,
    group: sensor.group,
  }));
});

const breadcrumbs = computed(() => [
  { label: t('job.label', 2), to: '/jobs' },
  { label: job.value?.name ?? jobId.value, to: `/jobs/${jobId.value}` },
  { label: t('job.graph') },
]);

function formatDate(date: string | undefined) {
  if (!date) {
    return t('job.graph_missing_range');
  }

  return new Date(date).toLocaleString(undefined, {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    fractionalSecondDigits: 3,
    hour12: false,
  });
}

async function loadJob() {
  if (!jobId.value) {
    return;
  }

  isLoadingJob.value = true;
  const { data, error } = await JobService.getJob(jobId.value);
  isLoadingJob.value = false;

  if (error || !data) {
    handleError(error, t('job.toasts.load_failed'));
    return;
  }

  job.value = data;
}

async function loadDevice(deviceId: string) {
  if (device.value?.id === deviceId) {
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

watch(
  () => job.value?.deviceId,
  (deviceId) => {
    if (deviceId) {
      loadDevice(deviceId);
    }
  },
  { immediate: true },
);

loadJob();
</script>

<style scoped></style>
