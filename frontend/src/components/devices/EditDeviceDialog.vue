<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('device.edit_device') }}</template>
    <template #default>
      <DeviceForm ref="deviceForm" v-model="device" :loading="updatingDevice" @on-submit="updateDevice" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import DeviceForm from '@/components/devices/DeviceForm.vue';
import { DeviceFormData } from '@/components/devices/DeviceForm.vue';
import DeviceService from '@/api/services/DeviceService';
import { UpdateDeviceRequest } from '@/api/services/DeviceService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  deviceId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onUpdate']);

const { t } = useI18n();

const device = ref<DeviceFormData>({} as DeviceFormData);
async function getDevice() {
  const { data, error } = await DeviceService.getDevice(props.deviceId);
  if (error) {
    handleError(error, t('command.toasts.load_failed'));
    return;
  }

  device.value = {
    name: data.name,
    deviceTemplate: {
      id: data.deviceTemplate?.id,
      name: data.deviceTemplate?.name,
    },
    accessToken: data.accessToken ?? '',
    protocol: data.protocol,
  };

  if (!data.deviceTemplate?.id) {
    device.value.deviceTemplate = undefined;
  }
}

const updatingDevice = ref(false);
const deviceForm = ref();
async function updateDevice() {
  const updateRequest: UpdateDeviceRequest = {
    name: device.value.name,
    templateId: device.value.deviceTemplate?.id,
    accessToken: device.value.accessToken,
    protocol: device.value.protocol,
  };

  updatingDevice.value = true;
  const { data, error } = await DeviceService.updateDevice(props.deviceId, updateRequest);
  updatingDevice.value = false;

  if (error) {
    handleError(error, t('device.toasts.update_failed'));
    return;
  }

  emit('onUpdate', data);
  isDialogOpen.value = false;
  toast.success(t('device.toasts.update_success'));
}

watch(
  () => isDialogOpen.value,
  (isOpen) => {
    if (isOpen) {
      device.value = {} as DeviceFormData;
      getDevice();
    }
  },
  { immediate: true },
);
</script>

<style lang="scss" scoped></style>
