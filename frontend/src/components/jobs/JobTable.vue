<template>
  <div>
    <div v-if="isMobile">
      <q-infinite-scroll
        v-if="mobileJobs.length"
        :offset="120"
        @load="onLoadMore"
      >
        <div class="jobs-grid">
          <q-card
            v-for="job in mobileJobs"
            :key="job.id"
            class="job-card shadow"
            v-ripple
            @click="goToJob(job)"
          >
            <q-card-section class="job-card__header">
              <div class="job-card__title">
                <div class="text-subtitle1 text-weight-medium ellipsis">{{ job.name }}</div>
                <router-link
                  v-if="!props.deviceId"
                  class="job-card__device text-caption text-primary"
                  :to="`/devices/${job.device.id}`"
                  @click.stop
                >
                  {{ job.device.name }}
                </router-link>
              </div>
              <JobStatusIcon :status="job.status" />
            </q-card-section>
            <q-card-section class="job-card__meta">
              <div class="job-card__meta-item">
                <div class="job-card__meta-label text-caption text-grey-7">{{ t('job.started_at') }}</div>
                <div class="text-body2">{{ formatDateTime(job.createdAt) }}</div>
              </div>
              <div class="job-card__meta-item">
                <div class="job-card__meta-label text-caption text-grey-7">{{ t('job.finished_at') }}</div>
                <div class="text-body2">{{ formatFinishedAt(job.finishedAt) }}</div>
              </div>
              <div class="job-card__meta-grid">
                <div class="job-card__meta-item">
                  <div class="job-card__meta-label text-caption text-grey-7">{{ t('job.step') }}</div>
                  <div class="text-body2">{{ formatStep(job) }}</div>
                </div>
                <div class="job-card__meta-item">
                  <div class="job-card__meta-label text-caption text-grey-7">{{ t('job.cycle') }}</div>
                  <div class="text-body2">{{ formatCycle(job) }}</div>
                </div>
              </div>
            </q-card-section>
            <q-card-actions class="job-card__actions" align="right">
              <q-btn
                :icon="mdiOpenInNew"
                color="grey-color"
                flat
                round
                @click.stop="goToJob(job)"
              >
                <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                  {{ t('global.open') }}
                </q-tooltip>
              </q-btn>
              <q-btn
                class="q-ml-sm"
                :disable="!job.finishedAt"
                :icon="mdiChartLine"
                color="grey-color"
                flat
                round
                @click.stop="openGraph(job)"
              >
                <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                  <span v-if="job.finishedAt">{{ t('job.view_graph') }}</span>
                  <span v-else>{{ t('job.graph_unavailable') }}</span>
                </q-tooltip>
              </q-btn>
            </q-card-actions>
          </q-card>
        </div>

        <template #loading>
          <div class="row justify-center q-my-md">
            <q-spinner-dots color="primary" size="32px" />
          </div>
        </template>
      </q-infinite-scroll>
      <div v-else class="full-width column flex-center q-pa-lg nothing-found-text">
        <q-icon :name="mdiListStatus" class="q-mb-md" size="50px"></q-icon>
        {{ isLoadingJobs ? t('table.loading_label') : t('table.no_data_label') }}
      </div>
    </div>
    <q-table
      v-else
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
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiListStatus, mdiOpenInNew, mdiChartLine } from '@quasar/extras/mdi-v7';
import { RouterLink, useRouter } from 'vue-router';
import JobStatusIcon from '@/components/jobs/JobStatusIcon.vue';
import type { JobsQueryParams, JobsResponse } from '@/api/services/JobService';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import JobService from '@/api/services/JobService';
import { handleError } from '@/utils/error-handler';
import { useQuasar, type QTableProps } from 'quasar';

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
  rowsPerPage: 20,
  rowsNumber: 0,
});
const jobs = ref<JobsResponse>();
const isLoadingJobs = ref(false);

type JobTableRow = NonNullable<JobsResponse['items']>[number];

const mobileJobs = ref<JobTableRow[]>([]);
const totalJobs = ref(0);
const mobilePage = ref(1);

const $q = useQuasar();
const router = useRouter();
const isMobile = computed(() => $q.platform.is.mobile);
const hasMoreJobs = computed(() => mobileJobs.value.length < totalJobs.value);

watch(
  () => props.deviceId,
  () => {
    pagination.value.page = 1;
    mobileJobs.value = [];
    mobilePage.value = 1;
    totalJobs.value = 0;

    const paginationTable: PaginationTable = {
      ...pagination.value,
      page: 1,
    };

    getJobs(paginationTable);
  },
);

async function onRequest(props: { pagination: PaginationTable }) {
  await getJobs(props.pagination);
}

async function getJobs(paginationTable: PaginationTable, options: { append?: boolean } = {}) {
  if (!options.append) {
    mobilePage.value = paginationTable.page;
  }

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
  if (error || !data) {
    handleError(error, t('device.toasts.loading_failed'));
    return;
  }

  jobs.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;

  totalJobs.value = data.totalCount ?? 0;
  const items = data.items ?? [];

  if (options.append) {
    if (items.length === 0) {
      return;
    }

    mobileJobs.value = [...mobileJobs.value, ...items];
    mobilePage.value = data.currentPage ?? paginationTable.page ?? mobilePage.value;
    return;
  }

  mobileJobs.value = items;
  mobilePage.value = data.currentPage ?? paginationTable.page ?? 1;
}
getJobs(pagination.value);

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

function goToJob(job: JobTableRow) {
  router.push({
    path: `/jobs/${job.id}`,
    query: { device: job.device.id },
  });
}

function openGraph(job: JobTableRow) {
  const route = getGraphRoute(job);
  if (!route) {
    return;
  }

  router.push(route);
}

function formatDateTime(value?: string | null) {
  if (!value) {
    return '—';
  }

  return new Date(value).toLocaleString();
}

function formatFinishedAt(value?: string | null) {
  if (!value) {
    return t('job.not_finished');
  }

  return formatDateTime(value);
}

function formatStep(job: JobTableRow) {
  return t('global.n_of_m', [job.currentStep ?? 1, job.totalSteps ?? job.currentStep ?? 1]);
}

function formatCycle(job: JobTableRow) {
  return t('global.n_of_m', [job.currentCycle ?? 1, job.totalCycles ?? job.currentCycle ?? 1]);
}

async function onLoadMore(_index: number, done: (stop?: boolean) => void) {
  if (isLoadingJobs.value) {
    done();
    return;
  }

  if (!hasMoreJobs.value) {
    done(true);
    return;
  }

  const nextPage = mobilePage.value + 1;
  await getJobs(
    {
      ...pagination.value,
      page: nextPage,
    } as PaginationTable,
    { append: true },
  );

  if (!hasMoreJobs.value) {
    done(true);
    return;
  }

  done();
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

<style lang="scss" scoped>
.jobs-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.job-card {
  cursor: pointer;

  &__header {
    display: flex;
    justify-content: space-between;
    gap: 12px;
  }

  &__title {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  &__meta {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  &__meta-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 12px;
  }

  &__meta-item {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  &__meta-label {
    letter-spacing: 0.5px;
    text-transform: uppercase;
  }

  &__actions {
    padding: 0 16px 16px;
  }
}
</style>
