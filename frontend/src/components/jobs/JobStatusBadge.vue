<template>
  <q-badge v-if="status" :color="jobStatusColors[status as JobStatusEnum]">
    {{ status }}
  </q-badge>
</template>

<script setup lang="ts">
import { JobStatusEnum } from '@/models/JobStatusEnum';
import { jobStatusColors } from '@/utils/job-status-look';

defineProps({
  status: {
    type: String,
    default: '',
  },
});
</script>
