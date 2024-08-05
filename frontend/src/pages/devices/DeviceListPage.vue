<template>
  <PageLayout :breadcrumbs="[{ label: t('device.label', 2), to: '/devices' }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <AutoRefreshButton
        v-model="refreshInterval"
        :loading="isLoadingDevices"
        class="col-grow col-lg-auto"
        @on-refresh="getDevices(pagination)"
      />
      <q-btn
        v-if="authStore.isAdmin"
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
      <DevicesTable
        v-model:pagination="pagination"
        :devices="devicesPaginated"
        :loading="isLoadingDevices"
        :filter="filter"
        class="shadow"
        @on-change="getDevices(pagination)"
        @on-request="onRequest"
      />
    </template>
  </PageLayout>
  <CreateDeviceDialog v-model="isCreateDialogOpen" @on-create="getDevices(pagination)" />
</template>

<script setup lang="ts">
import DevicesTable from '@/components/devices/DevicesTable.vue';
import { useI18n } from 'vue-i18n';
import { useAuthStore } from '@/stores/auth-store';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { ref } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import { useStorage } from '@vueuse/core';
import AutoRefreshButton from '@/components/core/AutoRefreshButton.vue';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import DeviceService from '@/api/services/DeviceService';
import { handleError } from '@/utils/error-handler';
import { DevicesQueryParams, DevicesResponse } from '@/api/types/Device';
import CreateDeviceDialog from '@/components/devices/CreateDeviceDialog.vue';

const { t } = useI18n();
const authStore = useAuthStore();
const filter = ref('');

// Setup for automatic refresh
const refreshInterval = useStorage('auto_device_refresh', 30);

const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 10,
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
</script>
