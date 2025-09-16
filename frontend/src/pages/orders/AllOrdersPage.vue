<template>
  <PageLayout :breadcrumbs="[{ label: t('order.label', 2) }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('order.add_order')"
        :icon="mdiPlus"
        @click="isCreateDialogOpen = true"
      />
    </template>
    <template #default>
      <OrdersTable
        v-model:pagination="pagination"
        :orders="ordersPaginated"
        :loading="isLoadingOrders"
        :filter="filter"
        class="shadow"
        @on-change="getOrders(pagination)"
        @on-request="onRequest"
      />
    </template>
  </PageLayout>
  <CreateOrderDialog v-model="isCreateDialogOpen" @on-create="getOrders(pagination)" />
</template>

<script setup lang="ts">
import OrdersTable from '@/components/orders/OrdersTable.vue';
import { useI18n } from 'vue-i18n';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { ref } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import { useStorage } from '@vueuse/core';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import { DevicesResponse } from '@/api/services/DeviceService';
import type { OrdersQueryParams } from '@/api/services/OrdersService';
import OrdersService from '@/api/services/OrdersService';
import { handleError } from '@/utils/error-handler';
import CreateOrderDialog from '@/components/orders/CreateOrderDialog.vue';

const { t } = useI18n();
const filter = ref('');
const refreshInterval = useStorage('auto_device_refresh', 30);
const isCreateDialogOpen = ref(false);
const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});
const ordersPaginated = ref<any>();
const isLoadingOrders = ref(false);

async function onRequest(props: { pagination: PaginationClient }) {
  await getOrders(props.pagination);
}

async function getOrders(paginationTable: PaginationTable) {
  const paginationQuery: OrdersQueryParams = {
      SortBy: paginationTable.sortBy,
      Descending: paginationTable.descending,
      SearchTerm: filter.value,
      PageNumber: paginationTable.page,
      PageSize: paginationTable.rowsPerPage,
    };
  
    isLoadingOrders.value = true;
    const { data, error } = await OrdersService.getOrders(paginationQuery);
    isLoadingOrders.value = false;
  
    if (error) {
      handleError(error, 'Loading recipes failed');
      return;
    } 


  ordersPaginated.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
}
getOrders(pagination.value);


</script>