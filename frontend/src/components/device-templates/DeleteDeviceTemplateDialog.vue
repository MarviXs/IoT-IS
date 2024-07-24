<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('device_template.delete_device_template') }}</template>
    <template #description>{{ t('device_template.delete_device_template_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '../core/DeleteConfirmationDialog.vue';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  templateId: {
    type: String,
    default: '',
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();

const isDeleteInProgress = ref(false);

async function handleDelete() {
  isDeleteInProgress.value = true;
  const { error } = await DeviceTemplateService.deleteDeviceTemplate(props.templateId);
  isDeleteInProgress.value = false;

  if (error) {
    handleError(error, 'Error deleting template');
    return;
  }

  toast.success(t('device_template.toasts.delete_success'));
  emit('onDeleted');
  isDialogOpen.value = false;
}
</script>
