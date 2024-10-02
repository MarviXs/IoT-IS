<template>
  <div>
    <q-expansion-item switch-toggle-side>
      <template #header>
        <div class="text-weight-medium text-subtitle1 flex items-center">{{ t('job.label') }}</div>
        <JobStatusBadges
          v-if="activeJob"
          class="q-ml-md flex items-center"
          :job-status="activeJob.status"
          :paused="activeJob.paused"
        />
        <q-space></q-space>
        <q-btn
          color="grey-color"
          unelevated
          :icon="mdiPlayCircleOutline"
          no-caps
          flat
          dense
          round
          size="15px"
          @click.stop="openDialog = true"
        >
          <q-tooltip :offset="[0, 4]">{{ t('job.run_job') }}</q-tooltip>
        </q-btn>
        <q-btn
          v-if="activeJob"
          dense
          size="14px"
          :icon="mdiEye"
          color="grey-color"
          :to="`/jobs/${activeJob.id}?device=${activeJob.deviceId}`"
          flat
          round
          ><q-tooltip :offset="[0, 4]">{{ t('global.details') }}</q-tooltip>
        </q-btn>
      </template>
      <div class="q-px-lg q-pb-sm">
        <div class="column full-height">
          <div v-if="activeJob" class="column justify-between col-grow q-mt-sm wrap">
            <div class="row justify-start items-center q-mb-sm">
              <q-circular-progress
                show-value
                font-size="16px"
                :value="activeJob.progress"
                size="100px"
                :thickness="0.22"
                color="primary"
                track-color="grey-3"
                class="q-mr-md"
              >
                {{ activeJob.progress.toFixed(0) }}%
              </q-circular-progress>
              <div class="column q-ml-sm q-gutter-y-xs justify-center q-my-md items-start">
                <div class="text-weight-medium">
                  {{ activeJob.name }}
                </div>
                <div>
                  {{ t('job.step_of', [activeJob.currentStep ?? 1, activeJob.totalSteps]) }}
                  <span v-if="activeJob.currentCommand">({{ activeJob.currentCommand }})</span>
                </div>
                <div>
                  {{ t('job.cycle_of', [activeJob.currentCycle ?? 1, activeJob.totalCycles]) }}
                </div>
              </div>
            </div>
            <JobControls
              class="col-grow"
              :job-id="activeJob.id"
              :paused="activeJob.paused"
              :status="activeJob.status"
            />
            <div class="flex justify-center q-mt-md">
              <div class="row items-center q-gutter-x-sm">
                <q-btn flat dense round :icon="mdiChevronLeft" :disable="currentJobIndex === 0" @click="previousJob" />
                <span class="text-caption">{{ currentJobIndex + 1 }} of {{ activeJobs.length }}</span>
                <q-btn
                  flat
                  dense
                  round
                  :icon="mdiChevronRight"
                  :disable="currentJobIndex === activeJobs.length - 1"
                  @click="nextJob"
                />
              </div>
            </div>
          </div>
          <div v-else class="column items-center justify-center col-grow q-mt-sm q-mb-md">
            <div class="q-mb-sm">{{ t('job.no_running_job') }}</div>
            <q-btn
              class="shadow"
              color="grey-color"
              outline
              unelevated
              no-caps
              size="15px"
              :label="t('job.run_job')"
              @click.stop="openDialog = true"
            />
          </div>
        </div>
      </div>
    </q-expansion-item>
    <StartJobDialog v-model="openDialog" :device-id="props.deviceId" @job-started="getActiveJobs" />
  </div>
</template>

<script setup lang="ts">
import StartJobDialog from '@/components/jobs/StartJobDialog.vue';
import JobControls from './JobControls.vue';
import JobStatusBadges from './JobStatusBadges.vue';
import { useI18n } from 'vue-i18n';
import { mdiChevronLeft, mdiChevronRight, mdiEye, mdiPlayCircleOutline } from '@quasar/extras/mdi-v7';
import JobService from '@/api/services/JobService';
import { ActiveJobsResponse } from '@/api/services/JobService';
import { handleError } from '@/utils/error-handler';
import { computed, onUnmounted, ref } from 'vue';
import { useSignalR } from '@/composables/useSignalR';

const props = defineProps({
  deviceId: {
    type: String,
    required: true,
  },
});

const { connection } = useSignalR();
const { t } = useI18n();
const openDialog = ref(false);

const activeJobs = ref<ActiveJobsResponse>([]);
const selectedJobId = ref<string | null>(null);

const isLoadingJobs = ref(false);

async function getActiveJobs() {
  isLoadingJobs.value = true;
  const { data, error } = await JobService.getActiveJobs(props.deviceId);
  isLoadingJobs.value = false;

  if (error) {
    handleError(error, t('device.toasts.loading_failed'));
    return;
  }
  activeJobs.value = data;
}
getActiveJobs();

async function subscribeToJobUpdates() {
  connection.on('ReceiveJobUpdate', (job) => {
    const jobId = job.id;
    const index = activeJobs.value.findIndex((j) => j.id === jobId);
    if (index !== -1) {
      activeJobs.value[index] = job;
    } else {
      activeJobs.value.push(job);
    }
    if (activeJobs.value.length === 1 || selectedJobId.value === jobId) {
      selectedJobId.value = job.id;
    }
  });
}
subscribeToJobUpdates();

const activeJob = computed(() => {
  if (!activeJobs.value || activeJobs.value.length === 0) {
    return null;
  }
  return activeJobs.value.find((job) => job.id === selectedJobId.value) || activeJobs.value[0];
});

const currentJobIndex = computed(() => {
  if (!activeJobs.value || !activeJob.value) {
    return -1;
  }
  return activeJobs.value.findIndex((job) => job.id === activeJob.value?.id);
});

function previousJob() {
  const index = currentJobIndex.value - 1;
  selectedJobId.value = activeJobs.value[index].id;
}

function nextJob() {
  const index = currentJobIndex.value + 1;
  selectedJobId.value = activeJobs.value[index].id;
}

onUnmounted(() => {
  connection.off('ReceiveJobUpdate');
});
</script>

<style lang="scss" scoped></style>
