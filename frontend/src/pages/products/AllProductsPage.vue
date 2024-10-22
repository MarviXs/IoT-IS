<template>
  <PageLayout :breadcrumbs="[{ label: t('products.label', 2) }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
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
</template>

<script setup lang="ts">
import DevicesTable from '@/components/devices/DevicesTable.vue';
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import { ref } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import { useStorage } from '@vueuse/core';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import { DevicesResponse } from '@/api/services/DeviceService';

const { t } = useI18n();
const filter = ref('');

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
  /* const paginationQuery: DevicesQueryParams = {
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
    } */

  const data = {
    currentPage: paginationTable.page,
    totalPages: 5,
    pageSize: paginationTable.rowsPerPage,
    totalCount: 50,
    hasPrevious: paginationTable.page > 1,
    hasNext: paginationTable.page < 5,
    items: Array.from({ length: paginationTable.rowsPerPage }, (_, index) => ({
      id: (index + 1 + (paginationTable.page - 1) * paginationTable.rowsPerPage).toString(),
      name: `Device ${index + 1 + (paginationTable.page - 1) * paginationTable.rowsPerPage}`,
      connected: true,
      lastSeen: new Date().toISOString(),
    })),
  };

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
