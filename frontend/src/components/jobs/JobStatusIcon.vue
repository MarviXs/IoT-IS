<template>
  <div class="row justify-center items-center">
    <div class="status-dot row items-center justify-center" :style="{ 'background-color': statusColor }">
      <q-icon :color="statusColor" :name="icon" size="1.75rem" />
      <q-tooltip content-style="font-size: 11px" :offset="[0, 4]"> {{ status }} </q-tooltip>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { JobStatus } from '@/api/services/JobService';
import { jobStatusColors, jobStatusIcon } from '@/utils/job-status-look';
import type { PropType } from 'vue';

const props = defineProps({
  status: {
    type: String as PropType<JobStatus>,
    default: '',
  },
});

const statusColor = jobStatusColors[props.status];
const icon = jobStatusIcon[props.status];
</script>
@/utils/job-status-look
