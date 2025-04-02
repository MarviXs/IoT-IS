<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>Delete scene</template>
    <template #description>Are you sure you want to delete this scene?</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '@/components/core/DeleteConfirmationDialog.vue';
import SceneService from '@/api/services/SceneService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  sceneId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();

const isDeleteInProgress = ref(false);
async function handleDelete() {
  isDeleteInProgress.value = true;
  const { data, error } = await SceneService.deleteScene(props.sceneId);
  isDeleteInProgress.value = false;
  isDialogOpen.value = false;

  if (error) {
    handleError(error, 'Failed to delete scene');
    return;
  }

  emit('onDeleted');
  toast.success(t('Scene deleted successfully'));
}
</script>
