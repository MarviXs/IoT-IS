<template>
  <div class="row">
    <q-badge :color="jobStatusColors[jobStatus]" class="q-pa-xs">
      {{ props.jobStatus }}
    </q-badge>
  </div>
</template>

<script setup lang="ts">
import { JobStatus } from '@/api/types/Job';
import { jobStatusColors } from '@/utils/job-status-look';
import { PropType } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps({
  jobStatus: {
    type: String as PropType<JobStatus>,
    required: true,
  },
  paused: {
    type: Boolean,
    default: false,
  },
});

const { t } = useI18n();
</script>
