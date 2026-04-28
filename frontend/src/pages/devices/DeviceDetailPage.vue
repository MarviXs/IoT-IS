<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('device.label', 2), to: '/devices' },
      { label: device?.name, to: `/devices/${deviceId}` },
    ]"
  >
    <template #after-title>
      <StatusDot v-if="device" :status="device.connectionState" />
    </template>
    <template v-if="device" #actions>
      <q-btn
        class="shadow bg-white col-12 col-md-auto"
        :to="`/devices/${device.id}/jobs`"
        text-color="grey-color"
        unelevated
        no-caps
        size="15px"
        :label="t('job.label', 2)"
        :icon="mdiListStatus"
      />
      <q-btn
        v-if="!device.syncedFromEdge"
        class="shadow bg-white col-12 col-md-auto"
        :to="`/devices/${device.id}/schedules`"
        text-color="grey-color"
        unelevated
        no-caps
        size="15px"
        :label="t('job_schedule.label', 2)"
        :icon="mdiCalendarClock"
      />
      <q-btn
        class="shadow bg-white col-1 col-md-auto"
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
        class="shadow bg-white col-12 col-md-auto"
        :to="`/devices/${device.id}/controls`"
        text-color="grey-color"
        unelevated
        no-caps
        size="15px"
        :label="t('device_template.controls')"
        :icon="mdiTuneVariant"
      />
      <q-btn
        class="shadow bg-white col-12 col-lg-auto"
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
        v-if="!device.syncedFromEdge"
        class="shadow col-grow col-lg-auto device-edit-action"
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
        <LatestDataPoints v-model:sensors="sensors" :get-last-value="getLastValue" />
      </div>
    </template>
  </PageLayout>
  <EditDeviceDialog v-model="isUpdateDialogOpen" :device-id="deviceId" @on-update="getDevice" />
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import DeviceInfoCard from '@/components/devices/DeviceInfoCard.vue';
import DataPointChartJS from '@/components/datapoints/DataPointChartJS.vue';
import { computed, onUnmounted, reactive, ref, watch } from 'vue';
import DeviceService from '@/api/services/DeviceService';
import { deviceToTreeNode, extractNodeKeys } from '@/utils/sensor-nodes';
import SensorSelectionTree from '@/components/datapoints/SensorSelectionTree.vue';
import CurrentJobCard from '@/components/jobs/CurrentJobCard.vue';
import { useI18n } from 'vue-i18n';
import {
  mdiCalendarClock,
  mdiListStatus,
  mdiMapMarker,
  mdiPencil,
  mdiTuneVariant,
  mdiViewGridOutline,
} from '@quasar/extras/mdi-v7';
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
import { useStorage } from '@vueuse/core';
import type { GraphOptions } from '@/components/datapoints/GraphOptionsForm.vue';
import type { GetLatestDataPointsQuery } from '@/api/services/DataPointService';
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

const dataPointChart = ref();
const currentJobCard = ref();

const sensorTree = ref<SensorNode>();
const tickedNodes = ref<string[]>();
type DeviceLatestDataPointsQuery = GetLatestDataPointsQuery & { MaskStaleValue?: boolean };

const graphOptions = useStorage<GraphOptions>(`graphOptions_${deviceId}`, {
  refreshInterval: 30,
  maskStaleValue: false,
  timeFormat: '24h',
  interpolationMethod: 'straight',
  lineStyle: 'lines',
  lineWidth: 3,
  markerSize: 3,
  samplingOption: 'BUCKETS',
  downsampleResolution: 100,
  downsampleMethod: 'Lttb',
  timeBucketSizeSeconds: 60,
  timeBucketMethod: 'Average',
});

if (graphOptions.value.maskStaleValue == null) {
  graphOptions.value.maskStaleValue = false;
}

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
  await subscribeToDeviceDataPoints(connection, deviceId);
}
subscribeDeviceUpdates();

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

async function getLastDataPoints() {
  for (const sensor of sensors.value) {
    getLastDataPoint(device.value?.id ?? '', sensor.tag);
  }
}

async function getLastDataPoint(deviceId: string, tag: string) {
  const query: DeviceLatestDataPointsQuery = {
    MaskStaleValue: graphOptions.value.maskStaleValue ?? false,
  };
  const { data, error } = await DataPointService.getLatestDataPoints(deviceId, tag, query);
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

watch(
  () => graphOptions.value.maskStaleValue,
  () => {
    void getLastDataPoints();
  },
);

const isUpdateDialogOpen = ref(false);
onUnmounted(() => {
  void unsubscribeFromDeviceDataPoints(connection, deviceId);
  unsubscribeFromLastDataPointUpdates();
});
</script>

<style lang="scss" scoped>
:deep(.actions .device-edit-action) {
  order: 1;
}

@media (max-width: 1023.98px) {
  :deep(.actions .device-edit-action) {
    order: -1;
    flex-basis: 100%;
    width: 100%;
  }
}
</style>
