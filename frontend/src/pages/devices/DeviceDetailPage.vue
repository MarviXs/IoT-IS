<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('device.label', 2), to: '/devices' },
      { label: device?.name, to: `/devices/${deviceId}` },
    ]"
  >
    <template #after-title>
      <StatusDot v-if="device" :connected="device.connected" />
    </template>
    <template v-if="device" #actions>
      <q-btn
        class="shadow bg-white col-grow col-lg-auto"
        :to="`/devices/${device.id}/jobs`"
        text-color="grey-color"
        unelevated
        no-caps
        size="15px"
        :label="t('job.label', 2)"
        :icon="mdiListStatus"
      />
      <q-btn
        class="shadow bg-white col-grow col-lg-auto"
        :to="`/devices/${device.id}/map`"
        text-color="grey-color"
        unelevated
        no-caps
        size="15px"
        v-if="device.deviceTemplate?.enableMap"
        :label="t('device.map')"
        :icon="mdiMapMarker"
      />
      <q-btn
        class="shadow bg-white col-grow col-lg-auto"
        :to="`/devices/${device.id}/controls`"
        text-color="grey-color"
        unelevated
        no-caps
        size="15px"
        :label="t('device_template.controls')"
        :icon="mdiTuneVariant"
      />
      <q-btn
        class="shadow bg-white col-grow col-lg-auto"
        :to="`/devices/${device.id}/grid`"
        text-color="grey-color"
        unelevated
        no-caps
        size="15px"
        v-if="device.deviceTemplate?.enableGrid"
        :label="t('device_template.grid_settings')"
        :icon="mdiViewGridOutline"
      />
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('global.edit')"
        :icon="mdiPencil"
        @click="isUpdateDialogOpen = true"
      />
    </template>
    <template v-if="device" #default>
      <div class="row q-col-gutter-x-lg q-col-gutter-y-lg justify-starrt">
        <div class="col-12 col-xl-4">
          <DeviceInfoCard :device="device" class="shadow container"></DeviceInfoCard>
        </div>
        <div class="col-12 col-lg-6 col-xl-5">
          <CurrentJobCard ref="currentJobCard" class="shadow container" :device-id="device.id"></CurrentJobCard>
        </div>
        <div class="col-12 col-lg-6 col-xl-3">
          <SensorSelectionTree
            v-if="sensorTree"
            v-model:tickedNodes="tickedNodes"
            :sensors-tree="sensorTree"
            class="shadow container"
          >
          </SensorSelectionTree>
        </div>
        <div class="col-12">
          <DeviceNotificationTable
            v-if="device"
            :device="device"
            class="shadow container"
            :device-id="device.id"
          ></DeviceNotificationTable>
        </div>
        <div class="col-12">
          <DataPointChartJS
            v-if="device && sensorTree"
            ref="dataPointChart"
            v-model:tickedNodes="tickedNodes"
            class="bg-white shadow q-pa-lg"
            :sensors="sensors"
            @refresh="getDevice"
          ></DataPointChartJS>
        </div>
        <LatestDataPoints v-model:sensors="sensors" />
      </div>
    </template>
  </PageLayout>
  <EditDeviceDialog v-model="isUpdateDialogOpen" :device-id="deviceId" @on-update="getDevice" />
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import DeviceInfoCard from '@/components/devices/DeviceInfoCard.vue';
import DataPointChartJS from '@/components/datapoints/DataPointChartJS.vue';
import { computed, onUnmounted, ref } from 'vue';
import DeviceService from '@/api/services/DeviceService';
import { deviceToTreeNode, extractNodeKeys } from '@/utils/sensor-nodes';
import SensorSelectionTree from '@/components/datapoints/SensorSelectionTree.vue';
import CurrentJobCard from '@/components/jobs/CurrentJobCard.vue';
import { useI18n } from 'vue-i18n';
import { mdiListStatus, mdiMapMarker, mdiPencil, mdiTuneVariant, mdiViewGridOutline } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { handleError } from '@/utils/error-handler';
import type { DeviceResponse } from '@/api/services/DeviceService';
import EditDeviceDialog from '@/components/devices/EditDeviceDialog.vue';
import type { SensorNode } from '@/models/SensorNode';
import StatusDot from '@/components/devices/StatusDot.vue';
import { useSignalR } from '@/composables/useSignalR';
import type { LastDataPoint } from '@/models/LastDataPoint';
import DataPointService from '@/api/services/DataPointService';
import LatestDataPoints from '@/components/datapoints/LatestDataPoints.vue';
import DeviceNotificationTable from '@/components/devices/DeviceNotificationTable.vue';
import type { SensorData } from '@/models/SensorData';

const { t } = useI18n();
const { connection, connect } = useSignalR();
const route = useRoute();

const deviceId = route.params.id.toString();
const device = ref<DeviceResponse>();
const isLoadingDevice = ref(false);

const dataPointChart = ref();
const currentJobCard = ref();

const sensorTree = ref<SensorNode>();
const tickedNodes = ref<string[]>();

const sensors = computed<SensorData[]>(() => {
  return (
    device.value?.deviceTemplate?.sensors.map((sensor) => {
      return {
        id: sensor.id,
        deviceId: device.value?.id ?? '',
        tag: sensor.tag,
        name: sensor.name,
        unit: sensor.unit,
        accuracyDecimals: sensor.accuracyDecimals,
        lastValue: lastDataPoints.value.find((dp) => dp.deviceId === device.value?.id && dp.tag === sensor.tag)?.value,
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

  if (!tickedNodes.value || !sensorTree.value) {
    sensorTree.value = deviceToTreeNode(device.value);
    tickedNodes.value = extractNodeKeys(sensorTree.value);
  }
  getLastDataPoints();
}
getDevice();

async function subscribeDeviceUpdates() {
  await connect();
  connection.send('SubscribeToDevice', deviceId);
}
subscribeDeviceUpdates();

const lastDataPoints = ref<LastDataPoint[]>([]);
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
  const index = lastDataPoints.value.findIndex(
    (dp) => dp.deviceId === newDataPoint.deviceId && dp.tag === newDataPoint.tag,
  );
  if (index !== -1) {
    lastDataPoints.value[index] = newDataPoint;
  } else {
    lastDataPoints.value.push(newDataPoint);
  }
}

async function subscribeToLastDataPointUpdates() {
  connection.on('ReceiveSensorLastDataPoint', (dataPoint: LastDataPoint) => {
    const index = lastDataPoints.value.findIndex(
      (dp) => dp.deviceId === dataPoint.deviceId && dp.tag === dataPoint.tag,
    );
    if (index !== -1) {
      lastDataPoints.value[index] = dataPoint;
    } else {
      lastDataPoints.value.push(dataPoint);
    }
  });
}
subscribeToLastDataPointUpdates();

const isUpdateDialogOpen = ref(false);
onUnmounted(() => {
  connection.send('UnsubscribeFromDevice', deviceId);
  connection.off('ReceiveSensorLastDataPoint');
});
</script>

<style lang="scss" scoped></style>
