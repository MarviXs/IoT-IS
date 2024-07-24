<template>
  <PageLayout :title="t('job.label', 2)">
    <JobStable :jobs="store.jobs.data" :show-device-name="true" :loading="store.jobs.isLoading" />
  </PageLayout>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import { useJobStore } from '@/stores/job-store';
import JobStable from '@/components/jobs/JobTable.vue';

const { t } = useI18n();

const store = useJobStore();
store.jobs.refresh();
</script>

<style lang="scss" scoped></style>
