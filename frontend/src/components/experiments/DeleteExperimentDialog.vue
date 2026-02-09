<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('experiment.delete_experiment') }}</template>
    <template #description>{{ t('experiment.delete_experiment_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '@/components/core/DeleteConfirmationDialog.vue';
import ExperimentService from '@/api/services/ExperimentService';
import { handleError } from '@/utils/error-handler';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  experimentId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();
const isDeleteInProgress = ref(false);

async function handleDelete() {
  isDeleteInProgress.value = true;
  const { error } = await ExperimentService.deleteExperiment(props.experimentId);
  isDeleteInProgress.value = false;
  isDialogOpen.value = false;

  if (error) {
    handleError(error, t('experiment.toasts.delete_failed'));
    return;
  }

  emit('onDeleted');
  toast.success(t('experiment.toasts.delete_success'));
}
</script>
