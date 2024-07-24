<template>
  <q-table
    :rows="props.jobs"
    :columns="columns"
    :loading="props.loading"
    flat
    :rows-per-page-options="[10, 20, 50]"
    class="shadow"
    :no-data-label="t('table.no_data_label')"
    :loading-label="t('table.loading_label')"
    :rows-per-page-label="t('table.rows_per_page_label')"
  >
    <template #no-data="{ message }">
      <div class="full-width column flex-center q-pa-lg nothing-found-text">
        <q-icon :name="mdiListStatus" class="q-mb-md" size="50px"></q-icon>
        {{ message }}
      </div>
    </template>

    <template #body-cell-deviceName="tableProps">
      <q-td :props="tableProps">
        <router-link class="text-black" :to="`/devices/${tableProps.row.deviceId}`">{{
          tableProps.row.deviceName
        }}</router-link>
      </q-td>
    </template>

    <template #body-cell-status="jobProps">
      <q-td auto-width :props="jobProps">
        <JobStatusIcon :status="jobProps.row.status.code" />
      </q-td>
    </template>

    <template #body-cell-actions="tableProps">
      <q-td auto-width :props="tableProps">
        <q-btn :icon="mdiOpenInNew" color="grey-color" flat round :to="`/jobs/${tableProps.row.uid}`"
          ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.open') }}
          </q-tooltip>
        </q-btn>
      </q-td>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { Job } from '@/models/Job';
import { PropType, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiListStatus, mdiOpenInNew } from '@quasar/extras/mdi-v6';
import { RouterLink } from 'vue-router';
import JobStatusIcon from '@/components/jobs/JobStatusIcon.vue';
import { getLastJobStatus } from '@/models/Device';

const { t } = useI18n();

const props = defineProps({
  jobs: {
    type: Array as PropType<Job[]>,
    default: () => [],
  },
  showDeviceName: {
    type: Boolean,
    default: false,
  },
  loading: {
    type: Boolean,
    default: false,
  },
});

const columns = computed<QTableProps['columns']>(() => {
  const baseColumns: QTableProps['columns'] = [
    {
      name: 'name',
      label: t('job.label'),
      field: 'name',
      sortable: true,
      align: 'left',
    },
    {
      name: 'started_at',
      label: t('job.started_at'),
      field: 'createdAt',
      sortable: true,
      align: 'left',
      format: (val: string) => new Date(val).toLocaleString(),
    },
    {
      name: 'finished_at',
      label: t('job.finished_at'),
      field: 'finishedAt',
      sortable: true,
      align: 'left',
      format: (val: string) => (val ? new Date(val).toLocaleString() : t('job.not_finished')),
    },
    {
      name: 'step',
      label: t('job.step'),
      field: (row) => row.status?.currentStep ?? 1,
      sortable: true,
      align: 'left',
      format: (val: string, row: Job) => t('global.n_of_m', [val, row.noOfCmds]),
    },
    {
      name: 'cycle',
      label: t('job.cycle'),
      field: (row) => row.status?.currentCycle ?? 1,
      sortable: true,
      align: 'left',
      format: (val: number, row: Job) => t('global.n_of_m', [val, row.noOfReps]),
    },
    {
      name: 'status',
      label: t('job.status'),
      field(row) {
        return getLastJobStatus(row);
      },
      sortable: true,
      align: 'center',
    },
    {
      name: 'actions',
      label: '',
      field: '',
      align: 'center',
      sortable: false,
    },
  ];

  if (props.showDeviceName) {
    baseColumns.unshift({
      name: 'deviceName',
      label: t('device.label'),
      field: 'deviceName',
      sortable: true,
      align: 'left',
    });
  }

  return baseColumns;
});
</script>

<style lang="scss" scoped></style>
