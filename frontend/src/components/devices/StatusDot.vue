<template>
  <div class="row justify-center items-center">
    <div class="status-dot row items-center justify-center" :style="{ 'background-color': statusColor }">
      <q-icon color="white" :name="icon" />
      <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
        {{ statusLabel }}
      </q-tooltip>
    </div>
  </div>
</template>

<script setup lang="ts">
// import { DeviceStatus } from '@/models/Device';
import type { PropType } from 'vue';
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiAlertCircleOutline, mdiConnection, mdiWifi } from '@quasar/extras/mdi-v7';

type DeviceConnectionState = 'Online' | 'Degraded' | 'Offline';
type DeviceConnectionStateValue = DeviceConnectionState | 0 | 1 | 2;

const props = defineProps({
  connected: {
    type: Boolean as PropType<boolean>,
    required: false,
  },
  status: {
    type: [String, Number] as PropType<DeviceConnectionStateValue>,
    required: false,
  },
});

const { t } = useI18n();

const connectionState = computed<DeviceConnectionState>(() => {
  if (props.status !== undefined) {
    if (props.status === 0 || props.status === 'Online') {
      return 'Online';
    }
    if (props.status === 1 || props.status === 'Degraded') {
      return 'Degraded';
    }
    return 'Offline';
  }
  return props.connected ? 'Online' : 'Offline';
});

const statusColor = computed(() => {
  if (connectionState.value === 'Degraded') {
    return '#f0a028';
  }
  return connectionState.value === 'Online' ? '#67c040' : '#e02222';
});

const icon = computed(() => {
  if (connectionState.value === 'Degraded') {
    return mdiAlertCircleOutline;
  }
  return connectionState.value === 'Online' ? mdiWifi : mdiConnection;
});

const statusLabel = computed(() => {
  if (connectionState.value === 'Degraded') {
    return t('device.degraded');
  }
  return connectionState.value === 'Online' ? t('device.connected') : t('device.disconnected');
});
</script>

<style scoped>
.status-dot {
  width: 24px;
  height: 24px;
  border-radius: 100%;
}
</style>
