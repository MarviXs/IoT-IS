<template>
  <DialogCommon v-model="isDialogOpen">
    <template #title>{{ t('device_template.firmwares.edit_firmware') }}</template>
    <template #default>
      <div v-if="isLoadingFirmware" class="q-pa-lg row justify-center">
        <q-spinner color="primary" size="32px" />
      </div>
      <DeviceFirmwareForm
        v-else
        v-model="form"
        :is-loading="isSubmitting"
        :existing-file-name="existingFileName"
        @on-submit="handleSubmit"
      />
    </template>
  </DialogCommon>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
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
  firmwareId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['updated']);

const form = ref<DeviceFirmwareFormModel>({
  versionNumber: '',
  versionDate: '',
  isActive: false,
  firmwareFile: null,
});

const firmwareMeta = ref<{ originalFileName: string }>();
const existingFileName = computed(() => firmwareMeta.value?.originalFileName ?? '');
const isLoadingFirmware = ref(false);

async function loadFirmware() {
  isLoadingFirmware.value = true;
  const { data, error } = await DeviceFirmwareService.getDeviceFirmware(props.templateId, props.firmwareId);
  isLoadingFirmware.value = false;

  if (error) {
    handleError(error, t('device_template.firmwares.toasts.load_failed'));
    return;
  }

  form.value = {
    versionNumber: data.versionNumber,
    versionDate: data.versionDate,
    isActive: data.isActive,
    firmwareFile: null,
  };
  firmwareMeta.value = { originalFileName: data.originalFileName };
}

const isSubmitting = ref(false);

async function handleSubmit() {
  isSubmitting.value = true;
  const { error } = await DeviceFirmwareService.updateDeviceFirmware(props.templateId, props.firmwareId, form.value);
  isSubmitting.value = false;

  if (error) {
    handleError(error, t('device_template.firmwares.toasts.update_failed'));
    return;
  }

  toast.success(t('device_template.firmwares.toasts.update_success'));
  emit('updated');
  isDialogOpen.value = false;
}

watch(
  () => props.firmwareId,
  () => {
    if (isDialogOpen.value) {
      loadFirmware();
    }
  },
);

watch(isDialogOpen, (open) => {
  if (open) {
    loadFirmware();
  } else {
    form.value = {
      versionNumber: '',
      versionDate: '',
      isActive: false,
      firmwareFile: null,
    };
    firmwareMeta.value = undefined;
  }
});
</script>

<style lang="scss" scoped></style>
