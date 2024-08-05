<template>
  <div class="row justify-center items-center">
    <div class="status-dot row items-center justify-center" :style="{ 'background-color': statusColor }">
      <q-icon color="white" :name="icon" />
      <q-tooltip content-style="font-size: 11px" :offset="[0, 4]"> {{ status }} </q-tooltip>
    </div>
  </div>
</template>

<script setup lang="ts">
// import { DeviceStatus } from '@/models/Device';
import { PropType, computed } from 'vue';
import { mdiConnection, mdiWifi } from '@quasar/extras/mdi-v7';

const props = defineProps({
  status: {
    type: String as PropType<DeviceStatus>,
    required: true,
  },
});

const statusColor = computed(() => {
  return props.status === DeviceStatus.ONLINE ? '#67c040' : '#e02222';
});

const icon = computed(() => {
  return props.status === DeviceStatus.ONLINE ? mdiWifi : mdiConnection;
});
</script>

<style scoped>
.status-dot {
  width: 24px;
  height: 24px;
  border-radius: 100%;
}
</style>
