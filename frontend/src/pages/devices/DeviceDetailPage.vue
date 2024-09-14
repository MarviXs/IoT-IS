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
      <AutoRefreshButton
        class="col-grow col-lg-auto"
        v-model="refreshInterval"
        :loading="isRefreshingDataPoints"
        @on-refresh="refreshDatapoints"
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
          <CurrentJobCard ref="currentJobCard" class="shadow bg-white" :device-id="device.id"></CurrentJobCard>
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
          <DataPointChartJS
            v-if="device && sensorTree"
            ref="dataPointChart"
            v-model:tickedNodes="tickedNodes"
            class="bg-white shadow q-pa-lg"
            :sensors="sensors"
            @refresh="getDevice"
          ></DataPointChartJS>
        </div>

        <div class="col-12 col-md-4 col-lg-3 col-xl-2" v-for="(sensor, index) in sensors">
          <LatestDataPointCard
            ref="latestDataPointCards"
            :key="sensor.id"
            :device-id="device.id"
            :sensor-tag="sensor.tag"
            :name="sensor.name"
            :unit="sensor.unit ?? ''"
            :accuracy-decimals="sensor.accuracyDecimals ?? 2"
            :color="getGraphColor(index)"
          />
        </div>
      </div>
    </template>
  </PageLayout>
  <EditDeviceDialog v-model="isUpdateDialogOpen" :device-id="deviceId" @on-update="getDevice" />
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import DeviceInfoCard from '@/components/devices/DeviceInfoCard.vue';
import DataPointChartJS, { SensorData } from '@/components/datapoints/DataPointChartJS.vue';
import { computed, ref } from 'vue';
import DeviceService from '@/api/services/DeviceService';
import { deviceToTreeNode, extractNodeKeys } from '@/utils/sensor-nodes';
import SensorSelectionTree from '@/components/datapoints/SensorSelectionTree.vue';
import CurrentJobCard from '@/components/jobs/CurrentJobCard.vue';
import { useI18n } from 'vue-i18n';
import { mdiListStatus, mdiPencil } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { useStorage } from '@vueuse/core';
import { handleError } from '@/utils/error-handler';
import { DeviceResponse } from '@/api/types/Device';
import EditDeviceDialog from '@/components/devices/EditDeviceDialog.vue';
import { SensorNode } from '@/models/SensorNode';
import StatusDot from '@/components/devices/StatusDot.vue';
import LatestDataPointCard from '@/components/datapoints/LatestDataPointCard.vue';
import { getGraphColor } from '@/utils/colors';
import AutoRefreshButton from '@/components/core/AutoRefreshButton.vue';

const { t } = useI18n();

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
}
getDevice();

const isRefreshingDataPoints = ref(false);
const latestDataPointCards = ref<InstanceType<typeof LatestDataPointCard>[]>();
async function refreshDatapoints() {
  isRefreshingDataPoints.value = true;
  dataPointChart.value.refresh();
  latestDataPointCards.value?.forEach((card) => {
    card.refresh();
  });
  isRefreshingDataPoints.value = false;
}

const isUpdateDialogOpen = ref(false);

const refreshInterval = useStorage('DeviceDetailRefreshInterval', 30);
</script>

<style lang="scss" scoped></style>
