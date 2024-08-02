<template>
  <PageLayout :breadcrumbs="[{ label: t('job.label', 2), to: '/jobs' }]">
    <JobTable />
  </PageLayout>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import JobTable from '@/components/jobs/JobTable.vue';

const { t } = useI18n();
</script>

<style lang="scss" scoped></style>
