<template>
  <div class="row q-col-gutter-sm items-center justify-center">
    <div v-if="!paused" class="col-12 col-sm-3">
      <JobControlButton
        :label="t('job.controls.pause')"
        color="grey-color"
        :icon="mdiPause"
        :loading="pausingJob"
        :disable="status === FeiIsApiDataEnumsJobStatusEnum.JOB_PAUSED"
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
        :loading="skippingStep"
        @click="skipStep"
      ></JobControlButton>
    </div>
    <div class="col-12 col-sm-3">
      <JobControlButton
        :label="t('job.controls.skip_cycle')"
        color="primary"
        :icon="mdiSkipForward"
        :loading="skippingCycle"
        @click="skipCycle"
      ></JobControlButton>
    </div>
    <div class="col-12 col-sm-3">
      <JobControlButton
        :label="t('job.controls.stop')"
        color="red"
        :icon="mdiStop"
        :loading="stoppingJob"
        @click="stopJob"
      ></JobControlButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import { toast } from 'vue3-toastify';
import { ref } from 'vue';
import JobService from '@/api/services/JobService';
import JobControlButton from './JobControlButton.vue';
import { handleError } from '@/utils/error-handler';
import { useI18n } from 'vue-i18n';
import { mdiPause, mdiPlay, mdiSkipNext, mdiSkipForward, mdiStop } from '@quasar/extras/mdi-v7';
import { JobStatusEnum } from '@/models/JobStatusEnum';
import { PropType } from 'vue';
import { FeiIsApiDataEnumsJobStatusEnum } from '@/api/generated/schema.d';

const props = defineProps({
  jobId: {
    type: String,
    required: true,
  },
  paused: {
    type: Boolean,
    required: true,
  },
  status: {
    type: String as PropType<FeiIsApiDataEnumsJobStatusEnum>,
    required: true,
  },
});
const emit = defineEmits(['action-performed']);

const { t } = useI18n();

const stoppingJob = ref(false);
async function stopJob() {
  stoppingJob.value = true;
  const { data, error } = await JobService.cancelJob(props.jobId);
  stoppingJob.value = false;

  if (error) {
    handleError(error, t('job.toasts.stop_failed'));
    return;
  }

  toast.success(t('job.toasts.stop_success'));
  emit('action-performed');
}

const pausingJob = ref(false);
async function pauseJob() {
  pausingJob.value = true;
  const { data, error } = await JobService.pauseJob(props.jobId);
  pausingJob.value = false;

  if (error) {
    handleError(error, t('job.toasts.pause_failed'));
    return;
  }

  toast.success(t('job.toasts.pause_success'));
  emit('action-performed');
}

const resumingJob = ref(false);
async function resumeJob() {
  resumingJob.value = true;
  const { data, error } = await JobService.resumeJob(props.jobId);
  resumingJob.value = false;

  if (error) {
    handleError(error, t('job.toasts.resume_failed'));
    return;
  }

  toast.success(t('job.toasts.resume_success'));
  emit('action-performed');
}

const skippingStep = ref(false);
async function skipStep() {
  skippingStep.value = true;
  const { data, error } = await JobService.skipStep(props.jobId);
  skippingStep.value = false;

  if (error) {
    handleError(error, t('job.toasts.skip_step_failed'));
    return;
  }

  toast.success(t('job.toasts.skip_step_success'));
  emit('action-performed');
}

const skippingCycle = ref(false);
async function skipCycle() {
  skippingCycle.value = true;
  const { data, error } = await JobService.skipCycle(props.jobId);
  skippingCycle.value = false;

  if (error) {
    handleError(error, t('job.toasts.skip_cycle_failed'));
    return;
  }

  toast.success(t('job.toasts.skip_cycle_success'));
  emit('action-performed');
}
</script>
