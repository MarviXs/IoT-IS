<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleting" @on-submit="handleDelete">
    <template #title>{{ t('device_template.firmwares.delete_firmware') }}</template>
    <template #description>{{ t('device_template.firmwares.delete_firmware_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '@/components/core/DeleteConfirmationDialog.vue';
import DeviceFirmwareService from '@/api/services/DeviceFirmwareService';
import { handleError } from '@/utils/error-handler';

const isDialogOpen = defineModel<boolean>({ default: false });

const props = defineProps({
  templateId: {
    type: String,
    required: true,
  },
  firmwareId: {
    type: String,
    required: true,
  },
});

const emit = defineEmits(['deleted']);

const { t } = useI18n();

const isDeleting = ref(false);

async function handleDelete() {
  isDeleting.value = true;
  const { error } = await DeviceFirmwareService.deleteDeviceFirmware(props.templateId, props.firmwareId);
  isDeleting.value = false;
  isDialogOpen.value = false;

  if (error) {
    handleError(error, t('device_template.firmwares.toasts.delete_failed'));
    return;
  }

  toast.success(t('device_template.firmwares.toasts.delete_success'));
  emit('deleted');
}
</script>

<style lang="scss" scoped></style>
