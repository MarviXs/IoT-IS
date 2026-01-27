<template>
  <PageLayout :breadcrumbs="breadcrumbs">
    <template #after-title>
      <StatusDot v-if="device" :status="device.connectionState" />
    </template>
    <template #default>
      <q-card class="shadow container q-pa-lg">
        <q-inner-loading :showing="isLoading">
          <q-spinner color="primary" size="42px" />
        </q-inner-loading>
        <div v-if="!isLoading && controls.length > 0" class="row q-col-gutter-md">
          <div v-for="control in controls" :key="control.id" class="col-12 col-sm-6 col-md-4 col-lg-3">
            <div
              v-if="isToggleControl(control)"
              class="control-toggle"
              :class="{ 'control-toggle--disabled': isToggleDisabled(control.id) || !hasToggleDependencies(control) }"
              :style="getButtonStyle(control.color)"
              @click="toggleControl(control, !toggleStates[control.id])"
            >
              <div class="control-toggle__label" :style="getToggleLabelStyle(control.color)">
                {{ control.name }}
              </div>
              <q-toggle
                :model-value="toggleStates[control.id] ?? false"
                :disable="isToggleDisabled(control.id) || !hasToggleDependencies(control)"
                :color="getButtonColor(control.color)"
                keep-color
                dense
                @update:model-value="(value) => toggleControl(control, value)"
              />
            </div>
            <q-btn
              v-else
              class="full-width control-button"
              :color="getButtonColor(control.color)"
              :text-color="getButtonTextColor(control.color)"
              :style="getButtonStyle(control.color)"
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
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import PageLayout from '@/layouts/PageLayout.vue';
import StatusDot from '@/components/devices/StatusDot.vue';
import DeviceService from '@/api/services/DeviceService';
import DeviceControlService from '@/api/services/DeviceControlService';
import JobService from '@/api/services/JobService';
import { handleError } from '@/utils/error-handler';
import type { DeviceResponse } from '@/api/services/DeviceService';
import type { DeviceControlResponse } from '@/api/services/DeviceControlService';
import type { StartJobRequest } from '@/api/services/JobService';
import DataPointService from '@/api/services/DataPointService';
import type { LastDataPoint } from '@/models/LastDataPoint';
import { useSignalR } from '@/composables/useSignalR';

const { t } = useI18n();
const route = useRoute();
const deviceId = route.params.id as string;

const device = ref<DeviceResponse>();
const controls = ref<DeviceControlResponse[]>([]);
const isLoading = ref(false);
const loadingControlId = ref<string | null>(null);
const toggleStates = ref<Record<string, boolean>>({});
const toggleDisabled = ref<Record<string, boolean>>({});
const pendingToggleStates = ref<Record<string, boolean>>({});

const breadcrumbs = computed(() => {
  const list: { label: string; to?: string }[] = [{ label: t('device.label', 2), to: '/devices' }];
  if (device.value) {
    list.push({ label: device.value.name, to: `/devices/${deviceId}` });
  }
  list.push({ label: t('device.controls.title') });
  return list;
});

const { connection, connect } = useSignalR();

onMounted(async () => {
  await loadData();
  void subscribeToDeviceUpdates();
});

onUnmounted(() => {
  try {
    connection.off('ReceiveSensorLastDataPoint', handleSensorUpdate);
    void connection.send('UnsubscribeFromDevice', deviceId);
  } catch (error) {
    console.error('Failed to unsubscribe from device updates', error);
  }
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
  if (!device.value) {
    controls.value = [];
    toggleStates.value = {};
    toggleDisabled.value = {};
    pendingToggleStates.value = {};
    return;
  }
  const { data, error } = await DeviceControlService.getDeviceControls(device.value.id);
  if (error) {
    handleError(error, t('device.controls.toasts.load_failed'));
    return;
  }
  controls.value = data ?? [];
  await initializeToggleStates();
}

async function startControl(control: DeviceControlResponse) {
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

async function toggleControl(control: DeviceControlResponse, desiredState: boolean) {
  if (!device.value || !hasToggleDependencies(control)) {
    return;
  }

  const recipeId = desiredState ? control.recipeOnId : control.recipeOffId;
  if (!recipeId) {
    toast.error(t('device.controls.toasts.toggle_failed'));
    return;
  }

  if (isMqttDevice.value) {
    pendingToggleStates.value[control.id] = desiredState;
    toggleDisabled.value[control.id] = true;
  }

  const payload: StartJobRequest = {
    recipeId,
    cycles: 1,
    isInfinite: false,
  };

  const { error } = await JobService.startJob(device.value.id, payload);

  if (error) {
    handleError(error, t('device.controls.toasts.toggle_failed'));
    if (isMqttDevice.value) {
      toggleDisabled.value[control.id] = false;
      delete pendingToggleStates.value[control.id];
    }
    return;
  }

  if (!isMqttDevice.value) {
    toggleStates.value = { ...toggleStates.value, [control.id]: desiredState };
    toast.success(t('device.controls.toasts.toggle_success'));
  }
}

function getButtonColor(color: string | undefined | null) {
  if (color && isHexColor(color)) {
    return void 0;
  }
  return color || 'primary';
}

function getButtonTextColor(color: string | undefined | null) {
  if (color && isHexColor(color)) {
    return getContrastingTextColor(color);
  }
  return void 0;
}

function getButtonStyle(color: string | undefined | null) {
  if (!color || !isHexColor(color)) {
    return {};
  }
  return {
    backgroundColor: color,
  };
}

function isHexColor(value: string) {
  return /^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$/.test(value.trim());
}

function getContrastingTextColor(hexColor: string) {
  const color = hexColor.replace('#', '');
  const normalized =
    color.length === 3
      ? color
          .split('')
          .map((char) => char + char)
          .join('')
      : color;
  const r = parseInt(normalized.slice(0, 2), 16);
  const g = parseInt(normalized.slice(2, 4), 16);
  const b = parseInt(normalized.slice(4, 6), 16);
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
  return luminance > 0.6 ? 'black' : 'white';
}

const isMqttDevice = computed(() => device.value?.protocol === 'MQTT');

const sensorTagById = computed<Record<string, string>>(() => {
  const sensors = device.value?.deviceTemplate?.sensors ?? [];
  return sensors.reduce(
    (map, sensor) => {
      map[sensor.id] = sensor.tag;
      return map;
    },
    {} as Record<string, string>,
  );
});

const controlIdBySensorTag = computed<Record<string, string>>(() => {
  return controls.value.reduce(
    (map, control) => {
      if (isToggleControl(control) && control.sensorId) {
        const tag = sensorTagById.value[control.sensorId];
        if (tag) {
          map[tag] = control.id;
        }
      }
      return map;
    },
    {} as Record<string, string>,
  );
});

function isToggleControl(control: DeviceControlResponse) {
  const type = control.type as number | string;
  return type === 1 || type === 'Toggle';
}

function hasToggleDependencies(control: DeviceControlResponse) {
  return Boolean(control.recipeOnId && control.recipeOffId && control.sensorId);
}

function isToggleDisabled(controlId: string) {
  return !!toggleDisabled.value[controlId];
}

function getToggleLabelStyle(color: string | undefined | null) {
  const textColor = getButtonTextColor(color);
  if (!textColor) {
    return {};
  }
  return { color: textColor };
}

async function initializeToggleStates() {
  if (!device.value) {
    toggleStates.value = {};
    return;
  }

  const toggleControls = controls.value.filter(isToggleControl);
  const states: Record<string, boolean> = {};

  await Promise.all(
    toggleControls.map(async (control) => {
      if (!control.sensorId) {
        states[control.id] = false;
        return;
      }

      const tag = sensorTagById.value[control.sensorId];
      if (!tag) {
        states[control.id] = false;
        return;
      }

      try {
        const { data, error } = await DataPointService.getLatestDataPoints(device.value.id, tag);
        if (!error && data && typeof data.value === 'number') {
          states[control.id] = data.value === 1;
        } else if (!(control.id in states)) {
          states[control.id] = false;
        }
      } catch (error) {
        console.error('Failed to load toggle state', error);
        if (!(control.id in states)) {
          states[control.id] = false;
        }
      }
    }),
  );

  toggleStates.value = states;
  toggleDisabled.value = {};
  pendingToggleStates.value = {};
}

async function subscribeToDeviceUpdates() {
  try {
    await connect();
    connection.off('ReceiveSensorLastDataPoint', handleSensorUpdate);
    connection.on('ReceiveSensorLastDataPoint', handleSensorUpdate);
    await connection.send('SubscribeToDevice', deviceId);
  } catch (error) {
    console.error('Failed to subscribe to device updates', error);
  }
}

const handleSensorUpdate = (dataPoint: LastDataPoint) => {
  if (!device.value || dataPoint.deviceId !== device.value.id) {
    return;
  }

  const controlId = controlIdBySensorTag.value[dataPoint.tag];
  if (!controlId || typeof dataPoint.value !== 'number') {
    return;
  }

  const newState = dataPoint.value === 1;
  toggleStates.value = { ...toggleStates.value, [controlId]: newState };

  if (pendingToggleStates.value[controlId] !== undefined) {
    const desiredState = pendingToggleStates.value[controlId];
    delete pendingToggleStates.value[controlId];
    toggleDisabled.value[controlId] = false;

    if (desiredState === newState) {
      toast.success(t('device.controls.toasts.toggle_success'));
    } else {
      toast.error(t('device.controls.toasts.toggle_failed'));
    }
  }
};
</script>

<style scoped>
.control-button {
  min-height: 60px;
}

.control-toggle {
  min-height: 60px;
  border-radius: 4px;
  border: 1px solid rgba(0, 0, 0, 0.12);
  padding: 12px 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: var(--q-color-grey-2, #f5f5f5);
  transition: opacity 0.2s ease;
  cursor: pointer;
}

.control-toggle--disabled {
  opacity: 0.6;
}

.control-toggle__label {
  font-weight: 500;
  margin-right: 16px;
}
</style>
