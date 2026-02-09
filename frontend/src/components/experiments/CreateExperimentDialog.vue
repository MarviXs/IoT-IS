<template>
  <DialogCommon v-model="isDialogOpen">
    <template #title>{{ t('experiment.create_experiment') }}</template>
    <template #default>
      <ExperimentForm v-model="form" :loading="isCreating" :is-create-mode="true" @on-submit="createExperiment" />
    </template>
  </DialogCommon>
</template>

<script setup lang="ts">
import DialogCommon from '@/components/core/DialogCommon.vue';
import ExperimentForm from '@/components/experiments/ExperimentForm.vue';
import type { ExperimentFormData } from '@/components/experiments/ExperimentForm.vue';
import ExperimentService from '@/api/services/ExperimentService';
import type { CreateExperimentRequest } from '@/api/services/ExperimentService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import { ref, watch } from 'vue';

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onCreate']);

const { t } = useI18n();
const isCreating = ref(false);

const form = ref<ExperimentFormData>(getDefaultForm());

watch(
  () => isDialogOpen.value,
  (isOpen) => {
    if (isOpen) {
      form.value = getDefaultForm();
    }
  },
);

function getDefaultForm(): ExperimentFormData {
  return {
    note: '',
    device: null,
    recipe: null,
    ranJob: null,
    mode: 'automatic',
    cycles: 1,
    startedAt: null,
    finishedAt: null,
  };
}

function toIsoDate(value?: string | null) {
  if (!value) {
    return null;
  }

  return new Date(value).toISOString();
}

async function createExperiment() {
  const isAutomatic = form.value.mode === 'automatic';

  const createRequest: CreateExperimentRequest = {
    note: form.value.note?.trim() || null,
    deviceId: form.value.device?.id || null,
    recipeToRunId: isAutomatic ? form.value.recipe?.id || null : null,
    allowCreateWithoutMatchedJob: isAutomatic ? undefined : true,
    cycles: isAutomatic ? form.value.cycles : 1,
    isInfinite: false,
    startedAt: isAutomatic ? null : toIsoDate(form.value.startedAt),
    finishedAt: isAutomatic ? null : toIsoDate(form.value.finishedAt),
  };

  isCreating.value = true;
  const { data: experimentId, error } = await ExperimentService.createExperiment(createRequest);
  isCreating.value = false;

  if (error) {
    handleError(error, t('experiment.toasts.create_failed'));
    return;
  }

  toast.success(t('experiment.toasts.create_success'));
  emit('onCreate', experimentId);
  isDialogOpen.value = false;
}
</script>
