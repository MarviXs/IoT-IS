<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('command.delete_command') }}</template>
    <template #description>{{ t('command.delete_command_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import type { ProblemDetails } from '@/api/types/ProblemDetails';
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

type CommandDeleteProblemDetails = ProblemDetails & {
  detail?: string | null;
  recipeUsageCount?: number;
  recipeNames?: string[];
};

async function handleDelete() {
  isDeleteInProgress.value = true;
  const { error } = await CommandService.deleteCommand(props.commandId);
  isDeleteInProgress.value = false;
  isDialogOpen.value = false;

  if (error) {
    const deleteError = error as CommandDeleteProblemDetails;
    if (Array.isArray(deleteError.recipeNames) && deleteError.recipeNames.length > 0) {
      const names = deleteError.recipeNames.join(', ');
      const key =
        deleteError.recipeNames.length === 1
          ? 'command.toasts.delete_used_in_recipe'
          : 'command.toasts.delete_used_in_recipes';
      toast.error(t(key, { names }));
      return;
    }

    if (typeof deleteError.recipeUsageCount === 'number' && deleteError.recipeUsageCount > 0) {
      const key =
        deleteError.recipeUsageCount === 1
          ? 'command.toasts.delete_used_in_recipe_count'
          : 'command.toasts.delete_used_in_recipes_count';
      toast.error(t(key, { count: deleteError.recipeUsageCount }));
      return;
    }

    handleError(error, t('command.toasts.delete_failed'));
    return;
  }

  emit('onDeleted');
  toast.success(t('command.toasts.delete_success'));
}
</script>
