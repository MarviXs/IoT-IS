<template>
  <div class="row items-center gap-md">
    <q-select
      v-model="selectedDeviceId"
      label="Device"
      :options="filteredDeviceOptions"
      outlined
      emit-value
      map-options
      use-input
      input-debounce="0"
      @filter="filterDevices"
      :rules="deviceRules"
      class="q-mr-sm"
      style="min-width: 150px"
    />
    <q-select
      v-model="selectedSensorTag"
      label="Sensor"
      :options="sensorTagOptions"
      outlined
      emit-value
      class="q-mr-sm"
      map-options
      style="min-width: 150px"
      :rules="sensorTagRules"
    />
  </div>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import { computed, ref } from 'vue';
import type { SceneDevice } from '@/api/services/DeviceService';
import type { JsonLogicVar } from 'json-logic-js';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  devices: { type: Array as PropType<SceneDevice>, required: true },
});
const selectedDevice = defineModel<JsonLogicVar>({ required: true });
const deviceSearch = ref('');

const selectedDeviceId = computed({
  get: () => {
    const deviceVar = selectedDevice.value?.var;
    const deviceId = typeof deviceVar === 'string' ? deviceVar.split('.')[1] : '';
    return deviceId;
  },
  set: (value: string) => {
    if (value !== selectedDeviceId.value) {
      selectedDevice.value = { var: `device.${value}.` };
    } else {
      selectedDevice.value = { var: `device.${value}.${selectedSensorTag.value || ''}` };
    }
  },
});

const selectedSensorTag = computed({
  get: () => {
    const deviceVar = selectedDevice.value?.var;
    const sensorTag = typeof deviceVar === 'string' ? deviceVar.split('.')[2] : '';
    return sensorTag;
  },
  set: (value: string) => {
    selectedDevice.value = { var: `device.${selectedDeviceId.value}.${value}` };
  },
});

const deviceOptions = computed(() => {
  return props.devices.map((device) => ({ label: device.name, value: device.id }));
});

const filteredDeviceOptions = computed(() => {
  const search = normalizeSearch(deviceSearch.value);

  if (!search) {
    return deviceOptions.value;
  }

  return deviceOptions.value.filter((device) => normalizeSearch(device.label).includes(search));
});

const sensorTagOptions = computed(() => {
  const selectedDeviceObject = props.devices.find((device) => device.id === selectedDeviceId.value);
  return selectedDeviceObject
    ? selectedDeviceObject.sensors.map((sensor) => ({ label: sensor.name, value: sensor.tag }))
    : [];
});

function filterDevices(value: string, update: (callbackFn: () => void) => void) {
  update(() => {
    deviceSearch.value = value;
  });
}

function normalizeSearch(value: string) {
  return value.trim().toLocaleLowerCase();
}

const deviceRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const sensorTagRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
</script>
