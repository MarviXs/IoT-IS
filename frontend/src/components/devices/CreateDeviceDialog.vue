<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('device.add_device') }}</template>
    <template #default>
      <DeviceForm ref="deviceForm" v-model="device" @on-submit="createDevice" :loading="creatingDevice" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import { CreateDeviceRequest } from '@/api/services/DeviceService';
import DeviceForm from '@/components/devices/DeviceForm.vue';
import { DeviceFormData } from '@/components/devices/DeviceForm.vue';
import DeviceService from '@/api/services/DeviceService';

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onCreate']);

const { t } = useI18n();

const creatingDevice = ref(false);
const device = ref<DeviceFormData>({
  name: '',
  accessToken: '',
  deviceTemplate: undefined,
});
const deviceForm = ref();

async function createDevice() {
  const createRequest: CreateDeviceRequest = {
    name: device.value.name,
    templateId: device.value.deviceTemplate?.id,
    accessToken: device.value.accessToken ?? '',
  };

  creatingDevice.value = true;
  const { data, error } = await DeviceService.createDevice(createRequest);
  creatingDevice.value = false;

  if (error) {
    handleError(error, t('device.toasts.create_failed'));
    return;
  }

  emit('onCreate', data);
  isDialogOpen.value = false;

  toast.success(t('device.toasts.create_success'));
}
</script>

<style lang="scss" scoped></style>
