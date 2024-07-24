<template>
  <div class="row justify-center items-center">
    <div class="status-dot row items-center justify-center" :style="{ 'background-color': statusColor }">
      <q-icon :color="statusColor" :name="icon" size="1.75rem" />
      <q-tooltip content-style="font-size: 11px" :offset="[0, 4]"> {{ status }} </q-tooltip>
    </div>
  </div>
</template>

<script setup lang="ts">
import { JobStatusEnum } from '@/models/JobStatusEnum';
import { jobStatusColors, jobStatusIcon } from '@/utils/job-status-look';

const props = defineProps({
  status: {
    type: String,
    default: '',
  },
});

const statusColor = jobStatusColors[props.status as JobStatusEnum];
const icon = jobStatusIcon[props.status as JobStatusEnum];
</script>
@/utils/job-status-look
