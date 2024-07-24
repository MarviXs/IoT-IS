<template>
  <dialog-common
    v-model="isDialogOpen"
    :action-label="t('global.add')"
    :loading="addingInProgress"
    @on-submit="addDevice"
  >
    <template #title>{{ t('device.add_device') }}</template>
    <template #default>
      <q-select
        v-model="selectedDeviceId"
        :options="filteredDevices"
        :label="t('device.label')"
        emit-value
        map-options
        option-value="uid"
        option-label="name"
      />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { PropType, ref, watch } from 'vue';
import ModuleService from '@/services/ModuleService';
import { handleError } from '@/utils/error-handler';
import { computed } from 'vue';
import { toast } from 'vue3-toastify';
import { Module } from '@/models/Module';
import { useDeviceStore } from '@/stores/device-store';
import { Device } from '@/models/Device';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  module: {
    type: Object as PropType<Module>,
    required: true,
  },
  alreadyAddedDevices: {
    type: Array as PropType<Device[]>,
    required: true,
  },
});
const emit = defineEmits(['onAdded']);

const { t } = useI18n();
const deviceStore = useDeviceStore();

const filteredDevices = computed(() => {
  if (!deviceStore.devices.data) return [];
  return deviceStore.devices.data.filter(
    (device) => !props.alreadyAddedDevices.find((alreadyAddedDevice) => alreadyAddedDevice.uid === device.uid),
  );
});

const selectedDeviceId = ref<string>();
const addingInProgress = ref(false);

async function addDevice() {
  if (!selectedDeviceId.value) return;
  addingInProgress.value = true;
  try {
    await ModuleService.addDeviceToModule(props.module.uid, selectedDeviceId.value);
    toast.success(t('module.toasts.add_device_to_module_success'));
    emit('onAdded');
    selectedDeviceId.value = undefined;
    isDialogOpen.value = false;
  } catch (error) {
    handleError(error, t('module.toasts.add_device_to_module_failed'));
  } finally {
    addingInProgress.value = false;
  }
}

watch(
  () => isDialogOpen,
  (value) => {
    if (!value) {
      selectedDeviceId.value = undefined;
    }
  },
);

if (!deviceStore.devices.data) deviceStore.devices.refresh();
</script>

<style lang="scss" scoped></style>
@/stores/device-store
