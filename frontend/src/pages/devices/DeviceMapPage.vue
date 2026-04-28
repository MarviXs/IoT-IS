<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('device.label', 2), to: '/devices' },
      { label: device?.name, to: `/devices/${deviceId}` },
      { label: 'Map', to: `/devices/${deviceId}/map` },
    ]"
  >
    <template v-if="device" #actions> </template>
    <template v-if="device" #default>
      <div class="row q-col-gutter-x-lg q-col-gutter-y-lg justify-starrt">
        <div class="col-12">
          <MapSensorSelectionTree
            v-if="sensorTree"
            v-model:selectedSensor="selectedSensor"
            :sensors-tree="sensorTree"
            class="shadow container"
          />
        </div>
        <div class="col-12">
          <DataPointMap
            v-if="device && sensorTree"
            v-model:selected-sensor-id="selectedSensor"
            class="bg-white shadow q-pa-lg"
            :sensors="sensors"
          ></DataPointMap>
        </div>
        <LatestDataPoints v-model:sensors="sensors" :get-last-value="getLastValue" />
      </div>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import { computed, onUnmounted, reactive, ref } from 'vue';
import DeviceService from '@/api/services/DeviceService';
import { deviceToTreeNode } from '@/utils/sensor-nodes';
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import { handleError } from '@/utils/error-handler';
import type { DeviceResponse } from '@/api/services/DeviceService';
import type { SensorNode } from '@/models/SensorNode';
import { useSignalR } from '@/composables/useSignalR';
import type { LastDataPoint } from '@/models/LastDataPoint';
import DataPointService from '@/api/services/DataPointService';
import LatestDataPoints from '@/components/datapoints/LatestDataPoints.vue';
import MapSensorSelectionTree from '@/components/datapoints/MapSensorSelectionTree.vue';
import DataPointMap from '@/components/datapoints/DataPointMap.vue';
import type { SensorData } from '@/models/SensorData';
import {
  subscribeToDeviceDataPoints,
  subscribeToLastDataPointEvents,
  unsubscribeFromDeviceDataPoints,
} from '@/utils/signalrDataPoints';

const { t } = useI18n();
const { connection, connect } = useSignalR();
const route = useRoute();

const deviceId = route.params.id.toString();
const device = ref<DeviceResponse>();
const isLoadingDevice = ref(false);

const sensorTree = ref<SensorNode>();
const selectedSensor = ref<string | null>(null);
const lastDataPointValues = reactive(new Map<string, number | null | undefined>());

function getLastValue(deviceId: string, sensorTag: string) {
  return lastDataPointValues.get(`${deviceId}-${sensorTag}`);
}

function setLastDataPointValue(dataPoint: LastDataPoint) {
  lastDataPointValues.set(`${dataPoint.deviceId}-${dataPoint.tag}`, dataPoint.value);
}

const sensors = computed<SensorData[]>(() => {
  return (
    device.value?.deviceTemplate?.sensors.map((sensor) => {
      const deviceId = device.value?.id ?? '';
      return {
        id: sensor.id,
        deviceId,
        tag: sensor.tag,
        name: sensor.name,
        unit: sensor.unit,
        accuracyDecimals: sensor.accuracyDecimals,
        group: sensor.group,
      };
    }) ?? []
  );
});

async function getDevice() {
  isLoadingDevice.value = true;
  const { data, error } = await DeviceService.getDevice(deviceId);
  isLoadingDevice.value = false;

  if (error) {
    handleError(error, t('device.toasts.loading_failed'));
    return;
  }

  device.value = data;

  if (!sensorTree.value) {
    sensorTree.value = deviceToTreeNode(device.value);
  }
  // Set first sensor as default selected
  if (!selectedSensor.value && sensorTree.value?.items.length > 0) {
    selectedSensor.value = sensorTree.value?.items[0]?.id ?? null;
  }
  getLastDataPoints();
}
getDevice();

async function subscribeDeviceUpdates() {
  await connect();
  await subscribeToDeviceDataPoints(connection, deviceId);
}
subscribeDeviceUpdates();

async function getLastDataPoints() {
  for (const sensor of sensors.value) {
    getLastDataPoint(device.value?.id ?? '', sensor.tag);
  }
}

async function getLastDataPoint(deviceId: string, tag: string) {
  const { data, error } = await DataPointService.getLatestDataPoints(deviceId, tag);
  if (error || !data) {
    return;
  }
  const newDataPoint: LastDataPoint = {
    deviceId: device.value?.id ?? '',
    tag,
    value: data.value ?? null,
    ts: data.ts ?? null,
    latitude: data.latitude ?? null,
    longitude: data.longitude ?? null,
    gridX: data.gridX ?? null,
    gridY: data.gridY ?? null,
  };
  setLastDataPointValue(newDataPoint);
}

const unsubscribeFromLastDataPointUpdates = subscribeToLastDataPointEvents(connection, setLastDataPointValue);

onUnmounted(() => {
  void unsubscribeFromDeviceDataPoints(connection, deviceId);
  unsubscribeFromLastDataPointUpdates();
});
</script>

<style lang="scss" scoped></style>
