<template>
  <PageLayout :breadcrumbs="[{ label: t('device.all_devices'), to: '/admin/devices' }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <AutoRefreshButton
        v-if="!isMobile"
        v-model="refreshInterval"
        :loading="isLoadingDevices"
        class="col-grow col-lg-auto"
        @on-refresh="getDevices(pagination)"
      />
      <q-btn
        v-if="!isMobile"
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('device.add_device')"
        :icon="mdiPlus"
        @click="isCreateDialogOpen = true"
      />
    </template>
    <template #default>
      <q-pull-to-refresh v-if="isMobile" @refresh="onPullToRefresh">
        <DevicesTable
          v-model:pagination="pagination"
          :devices="devicesPaginated"
          :loading="isLoadingDevices"
          :filter="filter"
          :mobile-devices="mobileDevices"
          :has-more="hasMoreDevices"
          :reset-key="mobileResetKey"
          admin-view
          @on-change="getDevices(pagination)"
          @on-request="onRequest"
          @on-load-more="loadMoreDevices"
        />
      </q-pull-to-refresh>
      <DevicesTable
        v-else
        v-model:pagination="pagination"
        :devices="devicesPaginated"
        :loading="isLoadingDevices"
        :filter="filter"
        admin-view
        @on-change="getDevices(pagination)"
        @on-request="onRequest"
      />
    </template>
  </PageLayout>
  <CreateDeviceDialog v-model="isCreateDialogOpen" @on-create="getDevices(pagination)" />
  <q-page-sticky v-if="isMobile" position="bottom-right" :offset="[18, 18]">
    <q-btn fab color="primary" :icon="mdiPlus" @click="isCreateDialogOpen = true" />
  </q-page-sticky>
</template>

<script setup lang="ts">
import DevicesTable from '@/components/devices/DevicesTable.vue';
import { useI18n } from 'vue-i18n';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { computed, ref } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import { useStorage, watchDebounced } from '@vueuse/core';
import AutoRefreshButton from '@/components/core/AutoRefreshButton.vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import AdminDeviceService from '@/api/services/AdminDeviceService';
import { handleError } from '@/utils/error-handler';
import type { AdminDevicesQueryParams, AdminDevicesResponse } from '@/api/services/AdminDeviceService';
import CreateDeviceDialog from '@/components/devices/CreateDeviceDialog.vue';
import { useQuasar } from 'quasar';

const { t } = useI18n();
const filter = ref('');

const $q = useQuasar();
const isMobile = computed(() => $q.platform.is.mobile);

const refreshInterval = useStorage('auto_admin_device_refresh', 30);

const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 20,
  rowsNumber: 0,
});
type DeviceList = NonNullable<AdminDevicesResponse['items']>;

const devicesPaginated = ref<AdminDevicesResponse>();
const isLoadingDevices = ref(false);
const mobileDevices = ref<DeviceList>([] as DeviceList);
const totalDevices = ref(0);
const mobileResetKey = ref(0);

const hasMoreDevices = computed(() => mobileDevices.value.length < totalDevices.value);

async function onRequest(props: { pagination: PaginationClient }) {
  await getDevices(props.pagination);
}

async function getDevices(paginationTable: PaginationTable, options: { append?: boolean } = {}): Promise<boolean> {
  const { append = false } = options;
  const paginationQuery: AdminDevicesQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  isLoadingDevices.value = true;
  const { data, error } = await AdminDeviceService.getDevices(paginationQuery);
  isLoadingDevices.value = false;

  if (error) {
    handleError(error, 'Loading devices failed');
    return false;
  }

  if (!data) {
    return false;
  }

  devicesPaginated.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;

  totalDevices.value = data.totalCount ?? 0;

  const items = data.items ?? [];

  if (append) {
    mobileDevices.value = [...mobileDevices.value, ...items];
  } else {
    mobileDevices.value = items;
    mobileResetKey.value++;
  }

  return true;
}
getDevices(pagination.value);

const isCreateDialogOpen = ref(false);

async function onPullToRefresh(done: () => void) {
  try {
    pagination.value.page = 1;
    await getDevices(pagination.value);
  } finally {
    done();
  }
}

async function loadMoreDevices(done: (stop?: boolean) => void) {
  if (isLoadingDevices.value) {
    done();
    return;
  }

  if (!hasMoreDevices.value) {
    done(true);
    return;
  }

  const nextPage = {
    ...pagination.value,
    page: pagination.value.page + 1,
  };

  const success = await getDevices(nextPage, { append: true });

  if (!success) {
    done();
    return;
  }

  if (!hasMoreDevices.value) {
    done(true);
    return;
  }

  done();
}

watchDebounced(
  filter,
  async () => {
    pagination.value.page = 1;
    await getDevices(pagination.value);
  },
  { debounce: 400 },
);
</script>
