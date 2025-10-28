<template>
  <DialogCommon v-model="isDialogOpen">
    <template #title>{{ t('device_template.firmwares.create_firmware') }}</template>
    <template #default>
      <DeviceFirmwareForm
        v-model="form"
        :is-loading="isSubmitting"
        require-file
        @on-submit="handleSubmit"
      />
    </template>
  </DialogCommon>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DialogCommon from '@/components/core/DialogCommon.vue';
import DeviceFirmwareForm, { type DeviceFirmwareFormModel } from './DeviceFirmwareForm.vue';
import DeviceFirmwareService from '@/api/services/DeviceFirmwareService';
import { handleError } from '@/utils/error-handler';

const { t } = useI18n();

const isDialogOpen = defineModel<boolean>({ default: false });
const props = defineProps({
  templateId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['created']);

const createFormDefaults: DeviceFirmwareFormModel = {
  versionNumber: '',
  versionDate: '',
  isActive: false,
  firmwareFile: null,
};

const form = ref<DeviceFirmwareFormModel>({ ...createFormDefaults });

const isSubmitting = ref(false);

async function handleSubmit() {
  if (!form.value.firmwareFile) {
    return;
  }

  isSubmitting.value = true;
  const { error } = await DeviceFirmwareService.createDeviceFirmware(props.templateId, form.value);
  isSubmitting.value = false;

  if (error) {
    handleError(error, t('device_template.firmwares.toasts.create_failed'));
    return;
  }

  toast.success(t('device_template.firmwares.toasts.create_success'));
  emit('created');
  isDialogOpen.value = false;
}

watch(isDialogOpen, (open) => {
  if (open) {
    form.value = { ...createFormDefaults };
  }
});
</script>

<style lang="scss" scoped></style>
