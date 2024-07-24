<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('device.delete_device') }}</template>
    <template #description>{{ t('device.delete_device_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '@/components/core/DeleteConfirmationDialog.vue';
import DeviceService from '@/api/services/DeviceService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
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
  const { error } = await DeviceService.deleteDevice(props.deviceId);
  isDeleteInProgress.value = false;

  if (error) {
    handleError(error, t('device.toasts.delete_failed'));
    return;
  }

  emit('onDeleted');
  isDialogOpen.value = false;
  toast.success(t('device.toasts.delete_success'));
}
</script>
