<template>
  <DialogCommon v-model="isDialogOpen">
    <template #title>{{ t('experiment.edit_experiment') }}</template>
    <template #default>
      <ExperimentForm v-model="form" :loading="isUpdating" @on-submit="updateExperiment" />
    </template>
  </DialogCommon>
</template>

<script setup lang="ts">
import DialogCommon from '@/components/core/DialogCommon.vue';
import ExperimentForm from '@/components/experiments/ExperimentForm.vue';
import type { ExperimentFormData } from '@/components/experiments/ExperimentForm.vue';
import ExperimentService from '@/api/services/ExperimentService';
import type { UpdateExperimentRequest } from '@/api/services/ExperimentService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import { ref, watch } from 'vue';

const props = defineProps<{ experimentId: string }>();

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onUpdate']);

const { t } = useI18n();
const isUpdating = ref(false);

const form = ref<ExperimentFormData>({
  note: '',
  device: null,
  recipe: null,
  ranJob: null,
  mode: 'automatic',
  cycles: 1,
  startedAt: null,
  finishedAt: null,
});

watch(
  () => isDialogOpen.value,
  (isOpen) => {
    if (isOpen) {
      getExperiment();
    }
  },
  { immediate: true },
);

function toIsoDate(value?: string | null) {
  if (!value) {
    return null;
  }

  return new Date(value).toISOString();
}

function toDateTimeLocal(value?: string | null) {
  if (!value) {
    return null;
  }

  const date = new Date(value);
  const offsetMs = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}

async function getExperiment() {
  const { data, error } = await ExperimentService.getExperiment(props.experimentId);

  if (error) {
    handleError(error, t('experiment.toasts.load_failed'));
    return;
  }

  form.value = {
    note: data.note || '',
    device: data.deviceId
      ? {
          id: data.deviceId,
          name: data.deviceName || data.deviceId,
        }
      : null,
    recipe: null,
    ranJob: data.ranJobId
      ? {
          id: data.ranJobId,
          name: data.ranJobName || data.ranJobId,
        }
      : null,
    mode: data.ranJobId ? 'manual' : 'automatic',
    cycles: 1,
    startedAt: toDateTimeLocal(data.startedAt),
    finishedAt: toDateTimeLocal(data.finishedAt),
  };
}

async function updateExperiment() {
  const updateRequest: UpdateExperimentRequest = {
    note: form.value.note?.trim() || null,
    recipeToRunId: null,
    deviceId: form.value.device?.id || null,
    ranJobId: form.value.ranJob?.id || null,
    startedAt: toIsoDate(form.value.startedAt),
    finishedAt: toIsoDate(form.value.finishedAt),
  };

  isUpdating.value = true;
  const { error } = await ExperimentService.updateExperiment(props.experimentId, updateRequest);
  isUpdating.value = false;

  if (error) {
    handleError(error, t('experiment.toasts.update_failed'));
    return;
  }

  toast.success(t('experiment.toasts.update_success'));
  emit('onUpdate');
  isDialogOpen.value = false;
}
</script>
