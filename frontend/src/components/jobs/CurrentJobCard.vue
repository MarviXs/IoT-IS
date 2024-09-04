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
          v-if="activeJob"
          dense
          size="14px"
          :icon="mdiOpenInNew"
          color="grey-color"
          :to="`/jobs/${activeJob.id}?device=${activeJob.deviceId}`"
          flat
          round
          ><q-tooltip :offset="[0, 4]">{{ t('global.details') }}</q-tooltip>
        </q-btn>
      </template>
      <div class="q-px-lg q-pb-md">
        <div class="column full-height">
          <div class="row items-center"></div>
          <div v-if="activeJob" class="column justify-between col-grow q-my-sm wrap">
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
              :status="activeJob.status as JobStatusEnum"
              @action-performed="getActiveJob"
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
      </div>
    </q-expansion-item>
    <StartJobDialog v-model="openDialog" :device-id="props.deviceId" @job-started="getActiveJob" />
  </div>
</template>

<script setup lang="ts">
import StartJobDialog from '@/components/jobs/StartJobDialog.vue';
import JobControls from './JobControls.vue';
import JobStatusBadges from './JobStatusBadges.vue';
import { useI18n } from 'vue-i18n';
import { mdiOpenInNew } from '@quasar/extras/mdi-v7';
import JobService from '@/api/services/JobService';
import { ActiveJobResponse } from '@/api/types/Job';
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { ProblemDetails } from '@/api/types/ProblemDetails';
import { JobStatusEnum } from '@/models/JobStatusEnum';

const props = defineProps({
  deviceId: {
    type: String,
    required: true,
  },
});

const { t } = useI18n();
const openDialog = ref(false);

const activeJob = ref<ActiveJobResponse | null>(null);
const isLoadingJob = ref(false);

async function getActiveJob() {
  isLoadingJob.value = true;
  const { data, error } = await JobService.getActiveJob(props.deviceId);
  isLoadingJob.value = false;

  if (error) {
    const problemDetails = error as ProblemDetails;

    if (problemDetails.status === 404) {
      activeJob.value = null;
      return;
    }

    handleError(error, t('device.toasts.loading_failed'));
    return;
  }

  activeJob.value = data;
}
getActiveJob();
</script>

<style lang="scss" scoped></style>
