<template v-if="store.device">
  <div>
    <div class="column full-height">
      <div class="row items-center">
        <div class="text-weight-medium text-h6 col-shrink">
          {{ t('job.label') }}
        </div>
        <q-space></q-space>
        <q-btn
          v-if="activeJob"
          dense
          size="14px"
          :icon="mdiOpenInNew"
          color="grey-color"
          :to="`/jobs/${activeJob.uid}`"
          flat
          round
          ><q-tooltip :offset="[0, 4]">{{ t('global.details') }}</q-tooltip>
        </q-btn>
      </div>
      <div v-if="activeJob" class="column justify-between col-grow q-my-sm wrap">
        <div class="row justify-start items-center q-mb-sm">
          <q-circular-progress
            show-value
            font-size="16px"
            :value="currentProgress * 100"
            size="100px"
            :thickness="0.22"
            color="primary"
            track-color="grey-3"
            class="q-mr-md"
          >
            <!-- eslint-disable-next-line @intlify/vue-i18n/no-raw-text -->
            {{ (currentProgress * 100).toFixed(0) }}%
          </q-circular-progress>
          <div class="column q-ml-sm q-gutter-y-xs justify-center q-my-md items-start">
            <JobStatusBadges :job="activeJob" />
            <div class="text-weight-medium">
              {{ activeJob.name }}
            </div>
            <div>
              {{ t('job.step_of', [activeJob.status?.currentStep ?? 1, activeJob.noOfCmds]) }}
              <!-- eslint-disable-next-line @intlify/vue-i18n/no-raw-text -->
              <span v-if="currentCommandName">({{ currentCommandName }})</span>
            </div>
            <div>
              {{ t('job.cycle_of', [activeJob.status?.currentCycle ?? 1, activeJob.noOfReps]) }}
            </div>
          </div>
        </div>
        <JobControls
          v-if="authStore.isAdmin"
          class="col-grow"
          :running-job="activeJob"
          @action-performed="jobs.refresh"
        />
      </div>
      <div v-else class="column items-center justify-center col-grow">
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
    <StartJobDialog v-model="openDialog" :device="props.device" @job-started="jobs.refresh" />
  </div>
</template>

<script setup lang="ts">
import StartJobDialog from '@/components/jobs/StartJobDialog.vue';
import { computed, ref, PropType, reactive } from 'vue';
import { JobStatusEnum } from '@/models/JobStatusEnum';
import { Job } from '@/models/Job';
import jobService from '@/services/JobService';
import JobControls from './JobControls.vue';
import JobStatusBadges from './JobStatusBadges.vue';
import { Device } from '@/models/Device';
import { useAuthStore } from '@/stores/auth-store';
import { useI18n } from 'vue-i18n';
import { mdiOpenInNew } from '@quasar/extras/mdi-v6';
import { useAsyncData } from '@/composables/useAsyncData';

const props = defineProps({
  device: {
    type: Object as PropType<Device>,
    required: true,
  },
});

const { t } = useI18n();
const authStore = useAuthStore();
const jobs = reactive(useAsyncData(() => jobService.getJobsOnDevice(props.device.uid)));
const openDialog = ref(false);

function findActiveJob(jobs: Job[]) {
  return jobs.find(
    (job) => job.currentStatus == JobStatusEnum.JOB_PROCESSING || job.currentStatus == JobStatusEnum.JOB_PENDING,
  );
}

const activeJob = computed(() => {
  if (jobs.data) {
    return findActiveJob(jobs.data);
  } else if (props.device.jobs) {
    return findActiveJob(props.device.jobs);
  }
  return null;
});

const currentProgress = computed(() => {
  if (activeJob.value && activeJob.value.status) {
    const { status, noOfReps, noOfCmds } = activeJob.value;
    const total = noOfReps * noOfCmds;

    const current = status.currentStep + ((status.currentCycle ?? 1) - 1) * noOfCmds;

    return Math.min(current / total, 1);
  }
  return 0;
});

const currentCommandName = computed(() => {
  if (activeJob.value && activeJob.value.status) {
    const { status, commands } = activeJob.value;
    const command = commands[(status.currentStep ?? 1) - 1];

    return command?.name ?? '';
  }
  return '';
});

async function refreshJobs() {
  await jobs.refresh();
}

defineExpose({
  refresh: refreshJobs,
});
</script>

<style lang="scss" scoped></style>
