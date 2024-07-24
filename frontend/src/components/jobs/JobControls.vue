<template>
  <div class="row q-col-gutter-sm items-center justify-center">
    <div v-if="!runningJob.paused" class="col-12 col-sm-3">
      <JobControlButton
        :label="t('job.controls.pause')"
        color="grey-color"
        :icon="mdiPause"
        :loading="pausingJob"
        @click="pauseJob"
      ></JobControlButton>
    </div>
    <div v-else class="col-12 col-sm-3">
      <JobControlButton
        :label="t('job.controls.resume')"
        color="primary"
        :icon="mdiPlay"
        :loading="resumingJob"
        @click="resumeJob"
      ></JobControlButton>
    </div>
    <div class="col-12 col-sm-3">
      <JobControlButton
        :label="t('job.controls.skip_step')"
        color="green-9"
        :icon="mdiSkipNext"
        :loading="skipStepLoading"
        @click="skipStep"
      ></JobControlButton>
    </div>
    <div class="col-12 col-sm-3">
      <JobControlButton
        :label="t('job.controls.skip_cycle')"
        color="primary"
        :icon="mdiSkipForward"
        :loading="skipCycleLoading"
        @click="skipCycle"
      ></JobControlButton>
    </div>
    <div class="col-12 col-sm-3">
      <JobControlButton
        :label="t('job.controls.stop')"
        color="red"
        :icon="mdiStop"
        :loading="stoppingJob"
        :disable="runningJob.toCancel == true"
        @click="stopJob"
      ></JobControlButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { toast } from 'vue3-toastify';
import { JobStatusEnum } from '@/models/JobStatusEnum';
import { Job } from '@/models/Job';
import { PropType, ref, Ref } from 'vue';
import jobService from '@/services/JobService';
import JobControlButton from './JobControlButton.vue';
import { handleError } from '@/utils/error-handler';
import { useI18n } from 'vue-i18n';
import { mdiPause, mdiPlay, mdiSkipNext, mdiSkipForward, mdiStop } from '@quasar/extras/mdi-v6';

const props = defineProps({
  runningJob: {
    type: Object as PropType<Job>,
    required: true,
  },
});
const emit = defineEmits(['action-performed']);

const { t } = useI18n();

//Job actions
async function performJobAction(
  action: { (jobId: string): Promise<Job> },
  successMessage: string,
  errorMessage: string,
  loadingRef: Ref<boolean>,
) {
  if (!props.runningJob) return;
  try {
    loadingRef.value = true;
    await action(props.runningJob.uid);
    toast.success(successMessage);
    emit('action-performed');
  } catch (e) {
    handleError(e, errorMessage);
  } finally {
    loadingRef.value = false;
  }
}

const stoppingJob = ref(false);
async function stopJob() {
  await performJobAction(jobService.cancelJob, t('job.toasts.stop_success'), t('job.toasts.stop_failed'), stoppingJob);
}

const pausingJob = ref(false);
async function pauseJob() {
  await performJobAction(jobService.pauseJob, t('job.toasts.pause_success'), t('job.toasts.pause_failed'), pausingJob);
}

const resumingJob = ref(false);
async function resumeJob() {
  await performJobAction(
    jobService.continueJob,
    t('job.toasts.resume_success'),
    t('job.toasts.resume_failed'),
    resumingJob,
  );
}

const skipStepLoading = ref(false);
async function skipStep() {
  await performJobAction(
    jobService.skipStep,
    t('job.toasts.skip_step_success'),
    t('job.toasts.skip_step_failed'),
    skipStepLoading,
  );
}

const skipCycleLoading = ref(false);
async function skipCycle() {
  await performJobAction(
    jobService.skipCycle,
    t('job.toasts.skip_cycle_success'),
    t('job.toasts.skip_cycle_failed'),
    skipCycleLoading,
  );
}
</script>
src/models/JobStatusEnum
