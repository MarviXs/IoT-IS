<template>
  <q-table
    v-model:pagination="pagination"
    binary-state-sort
    :rows="jobs?.items ?? []"
    :columns="columns"
    :loading="isLoadingJobs"
    flat
    :rows-per-page-options="[10, 20, 50]"
    class="shadow"
    :no-data-label="t('table.no_data_label')"
    :loading-label="t('table.loading_label')"
    :rows-per-page-label="t('table.rows_per_page_label')"
    @request="onRequest"
  >
    <template #no-data="{ message }">
      <div class="full-width column flex-center q-pa-lg nothing-found-text">
        <q-icon :name="mdiListStatus" class="q-mb-md" size="50px"></q-icon>
        {{ message }}
      </div>
    </template>

    <template #body-cell-deviceName="tableProps">
      <q-td :props="tableProps">
        <router-link class="text-primary" :to="`/devices/${tableProps.row.device.id}`">
          {{ tableProps.row.device.name }}</router-link
        >
      </q-td>
    </template>

    <template #body-cell-status="jobProps">
      <q-td auto-width :props="jobProps">
        <JobStatusIcon :status="jobProps.row.status" />
      </q-td>
    </template>

    <template #body-cell-actions="tableProps">
      <q-td auto-width :props="tableProps">
        <q-btn
          :icon="mdiOpenInNew"
          color="grey-color"
          flat
          round
          :to="`/jobs/${tableProps.row.id}?device=${tableProps.row.device.id}`"
          ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.open') }}
          </q-tooltip>
        </q-btn>
        <q-btn
          class="q-ml-sm"
          :disable="!tableProps.row.finishedAt"
          :icon="mdiChartLine"
          color="grey-color"
          flat
          round
          :to="getGraphRoute(tableProps.row)"
        >
          <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            <span v-if="tableProps.row.finishedAt">{{ t('job.view_graph') }}</span>
            <span v-else>{{ t('job.graph_unavailable') }}</span>
          </q-tooltip>
        </q-btn>
      </q-td>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import type { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiListStatus, mdiOpenInNew, mdiChartLine } from '@quasar/extras/mdi-v7';
import { RouterLink } from 'vue-router';
import JobStatusIcon from '@/components/jobs/JobStatusIcon.vue';
import type { JobsQueryParams, JobsResponse } from '@/api/services/JobService';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import JobService from '@/api/services/JobService';
import { handleError } from '@/utils/error-handler';

const { t } = useI18n();

const props = defineProps({
  deviceId: {
    type: String,
    required: false,
    default: '',
  },
});

const pagination = ref<PaginationClient>({
  sortBy: 'CreatedAt',
  descending: true,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});
const jobs = ref<JobsResponse>();
const isLoadingJobs = ref(false);

async function onRequest(props: { pagination: PaginationTable }) {
  await getJobs(props.pagination);
}

async function getJobs(paginationTable: PaginationTable) {
  isLoadingJobs.value = true;

  const query: JobsQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: '',
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  if (props.deviceId) {
    query.DeviceId = props.deviceId;
  }

  const { data, error } = await JobService.getJobs(query);
  isLoadingJobs.value = false;
  if (error) {
    handleError(error, t('device.toasts.loading_failed'));
    return;
  }
  jobs.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
}
getJobs(pagination.value);

type JobTableRow = NonNullable<JobsResponse['items']>[number];

function getGraphRoute(job: JobTableRow) {
  if (!job.finishedAt) {
    return undefined;
  }

  return {
    path: `/jobs/${job.id}/graph`,
    query: {
      device: job.device.id,
      from: job.createdAt,
      to: job.finishedAt,
    },
  };
}

const columns = computed<QTableProps['columns']>(() => {
  const baseColumns: QTableProps['columns'] = [
    {
      name: 'Name',
      label: t('job.label'),
      field: 'name',
      sortable: true,
      align: 'left',
    },
    {
      name: 'CreatedAt',
      label: t('job.started_at'),
      field: 'createdAt',
      sortable: true,
      align: 'left',
      format: (val: string) => new Date(val).toLocaleString(),
    },
    {
      name: 'FinishedAt',
      label: t('job.finished_at'),
      field: 'finishedAt',
      sortable: true,
      align: 'left',
      format: (val: string) => (val ? new Date(val).toLocaleString() : t('job.not_finished')),
    },
    {
      name: 'step',
      label: t('job.step'),
      field: (row) => row.currentStep ?? 1,
      sortable: false,
      align: 'left',
      format: (val: string, row) => t('global.n_of_m', [val, row.totalSteps]),
    },
    {
      name: 'cycle',
      label: t('job.cycle'),
      field: (row) => row.currentCycle ?? 1,
      sortable: false,
      align: 'left',
      format: (val: number, row) => t('global.n_of_m', [val, row.totalCycles]),
    },
    {
      name: 'status',
      label: t('job.status'),
      field: 'status',
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

  if (!props.deviceId) {
    baseColumns.unshift({
      name: 'deviceName',
      label: t('device.label'),
      field: 'device.name',
      sortable: true,
      align: 'left',
    });
  }

  return baseColumns;
});
</script>

<style lang="scss" scoped></style>
