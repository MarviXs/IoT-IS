<template>
  <div class="row">
    <q-badge :color="jobStatusColors[jobStatus]" class="q-pa-xs">
      {{ props.jobStatus }}
    </q-badge>
    <q-badge v-if="paused" color="accent" class="q-pa-xs q-ml-sm">
      {{ t('job.paused').toUpperCase() }}
    </q-badge>
  </div>
</template>

<script setup lang="ts">
import { jobStatusColors } from '@/utils/job-status-look';
import { PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import { JobStatusEnum } from '@/api/types/Job';

const props = defineProps({
  jobStatus: {
    type: Object as PropType<JobStatusEnum>,
    required: true,
  },
  paused: {
    type: Boolean,
    default: false,
  },
});

const { t } = useI18n();
</script>
