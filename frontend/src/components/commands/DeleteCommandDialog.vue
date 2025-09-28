<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('command.delete_command') }}</template>
    <template #description>{{ t('command.delete_command_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '@/components/core/DeleteConfirmationDialog.vue';
import CommandService from '@/api/services/CommandService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  commandId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();

const isDeleteInProgress = ref(false);
async function handleDelete() {
  isDeleteInProgress.value = true;
  const { error } = await CommandService.deleteCommand(props.commandId);
  isDeleteInProgress.value = false;
  isDialogOpen.value = false;

  if (error) {
    handleError(error, t('command.toasts.delete_failed'));
    return;
  }

  emit('onDeleted');
  toast.success(t('command.toasts.delete_success'));
}
</script>
