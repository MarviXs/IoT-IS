<template>
  <PageLayout :breadcrumbs="[{ label: 'Notifications', to: '/scenes' }]">
    <template #actions>
      <AutoRefreshButton
        v-if="!isMobile"
        v-model="refreshInterval"
        :loading="loadingNotifications"
        class="col-grow col-lg-auto"
        @on-refresh="getNotifications(pagination)"
      />
    </template>
    <template #default>
      <q-pull-to-refresh v-if="isMobile" @refresh="onPullToRefresh">
        <div class="notifications-mobile">
          <q-infinite-scroll
            v-if="mobileNotifications.length"
            ref="infiniteScrollRef"
            :offset="120"
            @load="loadMoreNotifications"
          >
            <div class="notifications-mobile__list">
              <q-card
                v-for="notification in mobileNotifications"
                :key="notification.id"
                class="notification-card shadow"
              >
                <q-card-section class="notification-card__section">
                  <div class="notification-card__header">
                    <q-chip
                      square
                      dense
                      class="notification-card__severity text-white"
                      :color="notificationColors[notification.severity]"
                      :label="notification.severity"
                    />
                    <RouterLink
                      :to="`/scenes/${notification.sceneId}`"
                      class="notification-card__scene text-primary text-weight-medium"
                    >
                      {{ notification.sceneName }}
                    </RouterLink>
                  </div>
                  <div class="notification-card__message text-body2">
                    {{ notification.message }}
                  </div>
                  <div class="notification-card__meta text-caption text-grey-7">
                    {{ formatTimeToDistance(notification.createdAt) }}
                    <q-tooltip v-if="notification.createdAt" content-style="font-size: 11px" :offset="[0, 4]">
                      {{ new Date(notification.createdAt).toLocaleString() }}
                    </q-tooltip>
                  </div>
                </q-card-section>
              </q-card>
            </div>

            <template #loading>
              <div class="row justify-center q-my-md">
                <q-spinner-dots color="primary" size="32px" />
              </div>
            </template>
          </q-infinite-scroll>
          <div v-else class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiBellOutline" class="q-mb-md" size="50px"></q-icon>
            {{ loadingNotifications ? t('table.loading_label') : t('table.no_data_label') }}
          </div>
        </div>
      </q-pull-to-refresh>
      <q-table
        v-else
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
import { QInfiniteScroll, useQuasar, type QTableProps } from 'quasar';
import type { NotificationQueryParams, NotificationsPaginated } from '@/api/services/NotificationService';
import NotificationService from '@/api/services/NotificationService';
import { formatTimeToDistance } from '@/utils/date-utils';
import { useStorage } from '@vueuse/core';
import AutoRefreshButton from '@/components/core/AutoRefreshButton.vue';
import { notificationColors } from '@/utils/colors';

const { t, locale } = useI18n();
const filter = ref('');
const refreshInterval = useStorage('auto_notifications_refresh', 30);

const $q = useQuasar();
const isMobile = computed(() => $q.screen.lt.md);

const pagination = ref<PaginationClient>({
  sortBy: 'createdAt',
  descending: true,
  page: 1,
  rowsPerPage: 20,
  rowsNumber: 0,
});
const notificationsPaginated = ref<NotificationsPaginated>();
const notifications = computed(() => notificationsPaginated.value?.items ?? []);

type NotificationList = NonNullable<NotificationsPaginated['items']>;

const mobileNotifications = ref<NotificationList>([] as NotificationList);
const totalNotifications = ref(0);
const infiniteScrollRef = ref<InstanceType<typeof QInfiniteScroll>>();
const hasMoreNotifications = computed(() => mobileNotifications.value.length < totalNotifications.value);

const loadingNotifications = ref(false);
async function getNotifications(
  paginationTable: PaginationTable,
  options: { append?: boolean } = {},
): Promise<boolean> {
  const { append = false } = options;

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
    handleError(error, 'Loading notifications failed');
    return false;
  }

  if (!data) {
    return false;
  }

  notificationsPaginated.value = data;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
  pagination.value.rowsNumber = data.totalCount ?? 0;

  totalNotifications.value = data.totalCount ?? 0;

  const items = data.items ?? [];

  if (append) {
    mobileNotifications.value = [...mobileNotifications.value, ...items];
  } else {
    mobileNotifications.value = items;
    infiniteScrollRef.value?.reset();
  }

  return true;
}
void getNotifications(pagination.value);

async function onPullToRefresh(done: () => void) {
  try {
    pagination.value.page = 1;
    await getNotifications(pagination.value);
  } finally {
    done();
  }
}

async function loadMoreNotifications(_index: number, done: (stop?: boolean) => void) {
  if (loadingNotifications.value) {
    done();
    return;
  }

  if (!hasMoreNotifications.value) {
    done(true);
    return;
  }

  const nextPage: PaginationClient = {
    ...pagination.value,
    page: pagination.value.page + 1,
  };

  const success = await getNotifications(nextPage, { append: true });

  if (!success) {
    done();
    return;
  }

  if (!hasMoreNotifications.value) {
    done(true);
    return;
  }

  done();
}

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

<style lang="scss" scoped>
.notifications-mobile__list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.notification-card {
  &__section {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
  }

  &__scene {
    display: inline-flex;
    align-items: center;
  }

  &__meta {
    display: flex;
    align-items: center;
    gap: 4px;
  }
}
</style>
