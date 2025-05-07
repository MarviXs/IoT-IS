<template>
  <div class="row items-center gap-md">
    <q-select
      v-model="selectedDeviceId"
      label="Device"
      :options="deviceOptions"
      outlined
      emit-value
      map-options
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
import { computed, PropType } from 'vue';
import { SceneDevice } from '@/api/services/DeviceService';
import { JsonLogicVar } from 'json-logic-js';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  devices: { type: Array as PropType<SceneDevice>, required: true },
});
const selectedDevice = defineModel<JsonLogicVar>({ required: true });

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

const sensorTagOptions = computed(() => {
  const selectedDeviceObject = props.devices.find((device) => device.id === selectedDeviceId.value);
  return selectedDeviceObject
    ? selectedDeviceObject.sensors.map((sensor) => ({ label: sensor.name, value: sensor.tag }))
    : [];
});

const deviceRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const sensorTagRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
</script>
