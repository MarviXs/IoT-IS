<template>
  <div class="row">
    <q-badge :color="jobStatusColors[job.currentStatus]" class="q-pa-xs">
      {{ props.job.currentStatus }}
    </q-badge>
    <q-badge v-if="props.job.toCancel" color="red" class="q-pa-xs q-ml-sm">
      {{ t('job.stopping').toUpperCase() }}
    </q-badge>
    <q-badge v-if="props.job.paused" color="accent" class="q-pa-xs q-ml-sm">
      {{ t('job.paused').toUpperCase() }}
    </q-badge>
  </div>
</template>

<script setup lang="ts">
import { Job } from '@/models/Job';
import { jobStatusColors } from '@/utils/job-status-look';
import { PropType } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps({
  job: {
    type: Object as PropType<Job>,
    required: true,
  },
});

const { t } = useI18n();
</script>
