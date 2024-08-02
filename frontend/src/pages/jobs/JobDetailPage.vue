<template>
  <PageLayout
    v-if="job"
    :breadcrumbs="breadcrumbs"
    :title="job.name"
    :previous-title="t('job.label', 2)"
    :previous-route="`/devices/${job.deviceId}`"
  >
    <template #description>
      <q-badge class="q-pa-xs q-ml-sm" color="primary">
        <!-- eslint-disable-next-line @intlify/vue-i18n/no-raw-text -->
        {{ t('job.job_cycle') }}: {{ job?.currentCycle ?? 1 }}/{{ job?.totalCycles }}
      </q-badge>
      <job-status-badges v-if="job" class="q-ml-sm" :job-status="job.status" :paused="job.paused"></job-status-badges>
    </template>
    <template #actions>
      <AutoRefreshButton v-model="refreshInterval" :loading="isLoadingJob" @on-refresh="getJob" />
      <JobControls class="col-grow" :job-id="job.id" :paused="job.paused" @action-performed="getJob" />
    </template>
    <template #default>
      <q-table
        :rows="job.commands"
        :columns="columns"
        :loading="isLoadingJob"
        flat
        :rows-per-page-options="[0]"
        class="shadow"
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
        hide-bottom
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiListStatus" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>
        <template #body-cell-progress="props">
          <q-td auto-width :props="props">
            <div style="min-height: 45px" class="row items-center justify-center">
              <div v-if="props.row.progress == 'CommandProcessing'" class="row items-center justify-center">
                <q-circular-progress size="20px" :thickness="0.25" color="primary" indeterminate> </q-circular-progress>
              </div>
              <q-icon
                v-else-if="props.row.progress == 'CommandDone'"
                :name="mdiCheck"
                size="28px"
                color="green"
              ></q-icon>
              <q-icon v-else :name="mdiCheck" size="28px" color="grey"></q-icon>
            </div>
          </q-td>
        </template>
        <template #body-cell-step="props">
          <q-td auto-width :props="props">
            {{ props.row.order }}
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useRoute } from 'vue-router';
import JobService from '@/api/services/JobService';
import JobControls from '@/components/jobs/JobControls.vue';
import JobStatusBadges from '@/components/jobs/JobStatusBadges.vue';
import { useI18n } from 'vue-i18n';
import { mdiCheck, mdiListStatus } from '@quasar/extras/mdi-v6';
import PageLayout from '@/layouts/PageLayout.vue';
import { useStorage } from '@vueuse/core';
import AutoRefreshButton from '@/components/core/AutoRefreshButton.vue';
import { JobResponse } from '@/api/types/Job';
import { handleError } from '@/utils/error-handler';
import DeviceService from '@/api/services/DeviceService';
import { DeviceResponse } from '@/api/types/Device';

const { t } = useI18n();

const route = useRoute();

const refreshInterval = useStorage('JobDetailRefreshInterval', 30);

const job = ref<JobResponse>();
const jobId = route.params.id as string;
const isLoadingJob = ref(false);

// If from device detail page, we have the device id
const deviceId = route.query.device as string;
const device = ref<DeviceResponse>();
async function getDevice() {
  if (deviceId) {
    const { data, error } = await DeviceService.getDevice(deviceId);
    if (error) {
      handleError(error, "Couldn't fetch device");
      return;
    }
    device.value = data;
  }
}
getDevice();

async function getJob() {
  isLoadingJob.value = true;
  const { data, error } = await JobService.getJob(jobId);
  isLoadingJob.value = false;

  if (error) {
    handleError(error, "Couldn't fetch job details");
    return;
  }

  job.value = data;
}
getJob();

// Breadcrumbs, device if available otherwise jobs
const breadcrumbs = computed(() => {
  if (deviceId) {
    return [
      { label: t('device.label', 2), to: '/devices' },
      { label: device.value?.name, to: `/devices/${deviceId}` },
      { label: t('job.label', 2), to: `/devices/${deviceId}/jobs` },
      { label: job.value?.name, to: `/jobs/${jobId}` },
    ];
  }
  return [
    { label: t('job.label', 2), to: '/jobs' },
    { label: job.value?.name, to: `/jobs/${jobId}` },
  ];
});

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'step',
    label: t('job.step'),
    field: 'order',
    sortable: false,
    align: 'center',
  },
  {
    name: 'progress',
    label: t('job.progress'),
    field: '',
    sortable: false,
    align: 'center',
  },
  {
    name: 'name',
    label: t('command.label'),
    field: 'name',
    sortable: false,
    align: 'left',
  },
]);
</script>

<style lang="scss" scoped>
.job-name {
  font-size: 1.8em;
  font-weight: 600;
  margin: 0 0 0 0.75rem;
  padding: 0;
  color: $accent;
}

.current-step-progress {
  font-size: 0.75rem;
  font-weight: 500;
  color: $primary;
}
</style>
