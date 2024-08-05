<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('collection.delete_collection') }}</template>
    <template #description>{{ t('collection.delete_collection_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '@/components/core/DeleteConfirmationDialog.vue';
import DeviceCollectionService from '@/api/services/DeviceCollectionService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  collectionId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();

const isDeleteInProgress = ref(false);
async function handleDelete() {
  isDeleteInProgress.value = true;
  const { data, error } = await DeviceCollectionService.deleteCollection(props.collectionId);
  isDeleteInProgress.value = false;
  isDialogOpen.value = false;

  if (error) {
    handleError(error, 'Failed to delete collection');
    return;
  }

  emit('onDeleted', props.collectionId);
  toast.success(t('collection.toasts.delete_success'));
}
</script>
