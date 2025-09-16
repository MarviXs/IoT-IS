<template>
  <PageLayout :breadcrumbs="[{ label: 'Notifications', to: '/scenes' }]">
    <template #actions>
      <AutoRefreshButton
        v-model="refreshInterval"
        :loading="loadingNotifications"
        class="col-grow col-lg-auto"
        @on-refresh="getNotifications(pagination)"
      />
    </template>
    <template #default>
      <q-table
        v-model:pagination="pagination"
        :rows="notifications"
        :columns="columns"
        :loading="loadingNotifications"
        flat
        binary-state-sort
        :rows-per-page-options="[10, 20, 50]"
        class="shadow"
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
        :rows-per-page-label="t('table.rows_per_page_label')"
        @request="(requestProp) => getNotifications(requestProp.pagination)"
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiBellOutline" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>

        <template #body-cell-severity="props">
          <q-td auto-width :props="props">
            <q-chip
              :label="props.row.severity"
              :color="notificationColors[props.row.severity]"
              class="text-white"
            ></q-chip>
          </q-td>
        </template>

        <template #body-cell-name="props">
          <q-td :props="props">
            <RouterLink :to="`/scenes/${props.row.sceneId}`">{{ props.row.sceneName }}</RouterLink>
          </q-td>
        </template>

        <template #body-cell-createdAt="propsContact">
          <q-td auto-width :props="propsContact">
            {{ formatTimeToDistance(propsContact.row.createdAt) }}
            <q-tooltip v-if="propsContact.row.createdAt" content-style="font-size: 11px" :offset="[0, 4]">
              {{ new Date(propsContact.row.createdAt).toLocaleString() }}
            </q-tooltip>
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { mdiBellOutline } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { computed, ref } from 'vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import { handleError } from '@/utils/error-handler';
import type { QTableProps } from 'quasar';
import type {
  NotificationQueryParams,
  NotificationsPaginated,
} from '@/api/services/NotificationService';
import NotificationService from '@/api/services/NotificationService';
import { formatTimeToDistance } from '@/utils/date-utils';
import { useStorage } from '@vueuse/core';
import AutoRefreshButton from '@/components/core/AutoRefreshButton.vue';
import { notificationColors } from '@/utils/colors';

const { t, locale } = useI18n();
const filter = ref('');
const refreshInterval = useStorage('auto_notifications_refresh', 30);

const pagination = ref<PaginationClient>({
  sortBy: 'createdAt',
  descending: true,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});
const notificationsPaginated = ref<NotificationsPaginated>();
const notifications = computed(() => notificationsPaginated.value?.items ?? []);

const loadingNotifications = ref(false);
async function getNotifications(paginationTable: PaginationTable) {
  const paginationQuery: NotificationQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  loadingNotifications.value = true;
  const { data, error } = await NotificationService.getNotifications(paginationQuery);
  loadingNotifications.value = false;

  if (error) {
    handleError(error, 'Loading templates failed');
    return;
  }

  notificationsPaginated.value = data;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
  pagination.value.rowsNumber = data.totalCount ?? 0;
}
getNotifications(pagination.value);

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'severity',
    label: 'Severity',
    field: 'severity',
    sortable: true,
    align: 'left',
  },
  {
    name: 'name',
    label: 'Scene',
    field: 'name',
    sortable: false,
    align: 'left',
  },
  {
    name: 'message',
    label: 'Message',
    field: 'message',
    sortable: true,
    align: 'left',
  },
  {
    name: 'createdAt',
    label: 'Created At',
    field: 'createdAt',
    sortable: true,
    format(val) {
      return new Date(val).toLocaleString(locale.value);
    },
    align: 'right',
  },
  // {
  //   name: 'actions',
  //   label: '',
  //   field: '',
  //   align: 'center',
  //   sortable: false,
  // },
]);
</script>
