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
              <span class="text-weight-medium">{{ formatDate(queryFrom) }}</span>
            </div>
            <div class="text-body2 q-mt-xs">
              {{ t('job.finished_at') }}:
              <span class="text-weight-medium">
                {{ queryTo ? formatDate(queryTo) : t('job.not_finished') }}
              </span>
            </div>
          </q-card>
          <q-banner
            v-if="!initialCustomTimeRange"
            class="bg-warning text-white shadow"
            dense
            rounded
          >
            {{ t('job.graph_missing_range') }}
          </q-banner>
          <q-banner
            v-else-if="sensors.length === 0"
            class="bg-warning text-white shadow"
            dense
            rounded
          >
            {{ t('job.graph_no_sensors') }}
          </q-banner>
          <DataPointChartJS
            v-else
            class="bg-white shadow q-pa-lg"
            :sensors="sensors"
            :initial-custom-time-range="initialCustomTimeRange"
            :time-range-storage-key="`job-${job.id}`"
          />
        </template>
        <q-banner
          v-else
          class="bg-negative text-white shadow"
          dense
          rounded
        >
          {{ t('job.graph_load_failed') }}
        </q-banner>
      </div>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { format } from 'date-fns';
import { useRoute, useRouter } from 'vue-router';
import type { LocationQueryRaw } from 'vue-router';
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
const router = useRouter();

const jobId = computed(() => route.params.id?.toString() ?? '');
const job = ref<JobResponse>();
const device = ref<DeviceResponse>();

const isLoadingJob = ref(false);
const isLoadingDevice = ref(false);

function normalizeQueryParam(value: unknown): string | undefined {
  if (Array.isArray(value)) {
    return value[0];
  }
  return typeof value === 'string' ? value : undefined;
}

const queryDeviceId = computed(() => normalizeQueryParam(route.query.device));
const queryFrom = computed(() => normalizeQueryParam(route.query.from));
const queryTo = computed(() => normalizeQueryParam(route.query.to));

const needsDeviceFetch = computed(() => !device.value && !!queryDeviceId.value);

const initialCustomTimeRange = computed(() => {
  if (!queryFrom.value || !queryTo.value) {
    return undefined;
  }

  const fromDate = new Date(queryFrom.value);
  const toDate = new Date(queryTo.value);

  if (Number.isNaN(fromDate.getTime()) || Number.isNaN(toDate.getTime())) {
    return undefined;
  }

  return {
    from: format(fromDate, 'yyyy-MM-dd HH:mm:ss'),
    to: format(toDate, 'yyyy-MM-dd HH:mm:ss'),
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

  return new Date(date).toLocaleString();
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
  ensureQueryParameters(data);
}

function ensureQueryParameters(jobData: JobResponse) {
  const currentQuery: LocationQueryRaw = { ...route.query };
  let requiresUpdate = false;

  if (!normalizeQueryParam(currentQuery.device) && jobData.deviceId) {
    currentQuery.device = jobData.deviceId;
    requiresUpdate = true;
  }

  if (requiresUpdate) {
    router.replace({
      params: route.params,
      query: currentQuery,
    });
  }
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
  () => queryDeviceId.value,
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
