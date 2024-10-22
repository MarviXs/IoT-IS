<template>
  <PageLayout :breadcrumbs="[{ label: t('products.label', 2) }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="secondary"
        unelevated
        no-caps
        size="15px"
        :label="t('product.import_products')"
        :icon="mdiImport"
      />
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('product.add_product')"
        :icon="mdiPlus"
      />
    </template>
    <template #default>
      <ProductsTable
        v-model:pagination="pagination"
        :devices="productsPaginated"
        :loading="isLoadingProducts"
        :filter="filter"
        class="shadow"
        @on-change="getDevices(pagination)"
        @on-request="onRequest"
      />
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import ProductsTable from '@/components/products/ProductsTable.vue';
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import { ref } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import ProductsService, { ProductsQueryParams } from '@/api/services/ProductsService';
import { handleError } from '@/utils/error-handler';
import { mdiPlus, mdiImport } from '@quasar/extras/mdi-v7';

const { t } = useI18n();
const filter = ref('');

const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});
const productsPaginated = ref<any>();
const isLoadingProducts = ref(false);

async function onRequest(props: { pagination: PaginationClient }) {
  await getDevices(props.pagination);
}

async function getDevices(paginationTable: PaginationTable) {
  const paginationQuery: ProductsQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  isLoadingProducts.value = true;
  const { data, error } = await ProductsService.getProducts(paginationQuery);
  isLoadingProducts.value = false;

  if (error) {
    handleError(error, 'Loading recipes failed');
    return;
  }

  productsPaginated.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
}
getDevices(pagination.value);
</script>
