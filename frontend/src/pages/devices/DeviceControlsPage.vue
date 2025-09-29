<template>
  <PageLayout :breadcrumbs="breadcrumbs">
    <template #after-title>
      <StatusDot v-if="device" :connected="device.connected" />
    </template>
    <template #default>
      <q-card class="shadow container q-pa-lg">
        <q-inner-loading :showing="isLoading">
          <q-spinner color="primary" size="42px" />
        </q-inner-loading>
        <div v-if="!isLoading && controls.length > 0" class="row q-col-gutter-md">
          <div v-for="control in controls" :key="control.id" class="col-12 col-sm-6 col-md-4 col-lg-3">
            <q-btn
              class="full-width control-button"
              :color="control.color || 'primary'"
              no-caps
              unelevated
              :label="control.name"
              :loading="loadingControlId === control.id"
              :disable="loadingControlId === control.id"
              @click="startControl(control)"
            />
          </div>
        </div>
        <div v-else-if="!isLoading" class="text-center text-grey-6 q-my-xl">
          {{ t('device.controls.no_controls') }}
        </div>
      </q-card>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import PageLayout from '@/layouts/PageLayout.vue';
import StatusDot from '@/components/devices/StatusDot.vue';
import DeviceService from '@/api/services/DeviceService';
import DeviceTemplateControlService from '@/api/services/DeviceTemplateControlService';
import JobService from '@/api/services/JobService';
import { handleError } from '@/utils/error-handler';
import type { DeviceResponse } from '@/api/services/DeviceService';
import type { DeviceTemplateControlResponse } from '@/api/services/DeviceTemplateControlService';
import type { StartJobRequest } from '@/api/services/JobService';

const { t } = useI18n();
const route = useRoute();
const deviceId = route.params.id as string;

const device = ref<DeviceResponse>();
const controls = ref<DeviceTemplateControlResponse[]>([]);
const isLoading = ref(false);
const loadingControlId = ref<string | null>(null);

const breadcrumbs = computed(() => {
  const list: { label: string; to?: string }[] = [{ label: t('device.label', 2), to: '/devices' }];
  if (device.value) {
    list.push({ label: device.value.name, to: `/devices/${deviceId}` });
  }
  list.push({ label: t('device.controls.title') });
  return list;
});

onMounted(async () => {
  await loadData();
});

async function loadData() {
  isLoading.value = true;
  try {
    await loadDevice();
    await loadControls();
  } finally {
    isLoading.value = false;
  }
}

async function loadDevice() {
  const { data, error } = await DeviceService.getDevice(deviceId);
  if (error) {
    handleError(error, t('device.toasts.loading_failed'));
    return;
  }
  device.value = data;
}

async function loadControls() {
  if (!device.value?.deviceTemplate?.id) {
    controls.value = [];
    return;
  }
  const { data, error } = await DeviceTemplateControlService.getTemplateControls(
    device.value.deviceTemplate.id,
  );
  if (error) {
    handleError(error, t('device.controls.toasts.load_failed'));
    return;
  }
  controls.value = data ?? [];
}

async function startControl(control: DeviceTemplateControlResponse) {
  if (!device.value) {
    return;
  }
  const payload: StartJobRequest = {
    recipeId: control.recipeId,
    cycles: control.cycles,
    isInfinite: control.isInfinite,
  };
  loadingControlId.value = control.id;
  const { error } = await JobService.startJob(device.value.id, payload);
  loadingControlId.value = null;

  if (error) {
    handleError(error, t('device.controls.toasts.start_failed'));
    return;
  }

  toast.success(t('device.controls.toasts.start_success'));
}
</script>

<style scoped>
.control-button {
  min-height: 56px;
}
</style>
