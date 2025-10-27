<template>
  <PageLayout :breadcrumbs="[{ label: t('device.label', 2), to: '/devices' }]">
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
          @on-change="getDevices(pagination)"
          @on-request="onRequest"
        />
      </q-pull-to-refresh>
      <DevicesTable
        v-else
        v-model:pagination="pagination"
        :devices="devicesPaginated"
        :loading="isLoadingDevices"
        :filter="filter"
        @on-change="getDevices(pagination)"
        @on-request="onRequest"
      />
    </template>
  </PageLayout>
  <CreateDeviceDialog v-model="isCreateDialogOpen" @on-create="getDevices(pagination)" />
  <q-page-sticky v-if="isMobile" position="bottom-right" :offset="[18, 18]">
    <q-btn
      fab
      color="primary"
      :icon="mdiPlus"
      @click="isCreateDialogOpen = true"
    />
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
import DeviceService from '@/api/services/DeviceService';
import { handleError } from '@/utils/error-handler';
import type { DevicesQueryParams, DevicesResponse } from '@/api/services/DeviceService';
import CreateDeviceDialog from '@/components/devices/CreateDeviceDialog.vue';
import { useQuasar } from 'quasar';

const { t } = useI18n();
const filter = ref('');

const $q = useQuasar();
const isMobile = computed(() => $q.screen.lt.md);

// Setup for automatic refresh
const refreshInterval = useStorage('auto_device_refresh', 30);

const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 20,
  rowsNumber: 0,
});
const devicesPaginated = ref<DevicesResponse>();
const isLoadingDevices = ref(false);

async function onRequest(props: { pagination: PaginationClient }) {
  await getDevices(props.pagination);
}

async function getDevices(paginationTable: PaginationTable) {
  const paginationQuery: DevicesQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  isLoadingDevices.value = true;
  const { data, error } = await DeviceService.getDevices(paginationQuery);
  isLoadingDevices.value = false;

  if (error) {
    handleError(error, 'Loading recipes failed');
    return;
  }

  devicesPaginated.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
}
getDevices(pagination.value);

const isCreateDialogOpen = ref(false);

async function onPullToRefresh(done: () => void) {
  try {
    await getDevices(pagination.value);
  } finally {
    done();
  }
}

watchDebounced(filter, () => getDevices(pagination.value), { debounce: 400 });
</script>
