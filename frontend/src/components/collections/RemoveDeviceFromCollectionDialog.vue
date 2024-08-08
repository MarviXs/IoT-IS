<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>Remove device from collection</template>
    <template #description>Are you sure you want to remove this device from the collection?</template>
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
  deviceId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();

const isDeleteInProgress = ref(false);
async function handleDelete() {
  isDeleteInProgress.value = true;
  const { data, error } = await DeviceCollectionService.removeDeviceFromCollection(props.collectionId, props.deviceId);
  isDeleteInProgress.value = false;
  isDialogOpen.value = false;

  if (error) {
    handleError(error, 'Failed remove device from collection');
    return;
  }

  emit('onDeleted', props.deviceId);
  toast.success('Device removed from collection');
}
</script>
