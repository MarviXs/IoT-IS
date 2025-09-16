<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('device.label', 2), to: '/devices' },
      { label: device?.name, to: `/devices/${deviceId}` },
      { label: t('job.label', 2), to: `/devices/${deviceId}/jobs` },
    ]"
  >
    <JobTable :device-id="deviceId" />
  </PageLayout>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import { ref } from 'vue';
import JobTable from '@/components/jobs/JobTable.vue';
import { handleError } from '@/utils/error-handler';
import type { DeviceResponse } from '@/api/services/DeviceService';
import DeviceService from '@/api/services/DeviceService';

const { t } = useI18n();

const route = useRoute();
const deviceId = route.params.id as string;

const device = ref<DeviceResponse>();
async function getDevice() {
  const { data, error } = await DeviceService.getDevice(deviceId);
  if (error) {
    handleError(error, t('device.toasts.loading_failed'));
    return;
  }
  device.value = data;
}
getDevice();
</script>

<style lang="scss" scoped></style>
