<template>
    <PageLayout :breadcrumbs="[{ label: t('company.label', 2) }]">
      <template #actions>
        <SearchBar v-model="filter" class="col-grow col-lg-auto" />
        <q-btn
          class="shadow col-grow col-lg-auto"
          color="primary"
          unelevated
          no-caps
          size="15px"
          :label="t('company.add_company')"
          :icon="mdiPlus"
          @click="isCreateDialogOpen = true"
        />
      </template>
      <template #default>
        <CompanyTable
          v-model:pagination="pagination"
          :devices="companyPaginated"
          :loading="isLoadingCompanies"
          @on-change="getCompanies(pagination)"
          @on-request="onRequest"
        />
      </template>
    </PageLayout>
    <CreateCompanyDialog v-model="isCreateDialogOpen" @on-create="getCompanies(pagination)" />
  </template>
  
  <script setup lang="ts">
  //import ProductsTable from '@/components/products/ProductsTable.vue';
  /*
  import { useI18n } from 'vue-i18n';
  import PageLayout from '@/layouts/PageLayout.vue';
  import { ref } from 'vue';
  import SearchBar from '@/components/core/SearchBar.vue';
  import { PaginationClient, PaginationTable } from '@/models/Pagination';
  import CompanyService, { ProductsQueryParams } from '@/api/services/ProductService';
  import { handleError } from '@/utils/error-handler';
  import { mdiPlus, mdiImport } from '@quasar/extras/mdi-v7';
  import CreateCompanyDialog from '@/components/companies/CreateCompanyDialog.vue';
  import DeleteCompanyDialog from '@/components/companies/DeleteCompanyDialog.vue';
  import CompanyTable from '@/components/companies/CompanyTable.vue';
  
  const { t } = useI18n();
  const filter = ref('');
  
  const pagination = ref<PaginationClient>({
    sortBy: 'name',
    descending: false,
    page: 1,
    rowsPerPage: 10,
    rowsNumber: 0,
  });
  const companyPaginated = ref<any>();
  const isLoadingCompanies = ref(false);
  
  async function onRequest(props: { pagination: PaginationClient }) {
    await getCompanies(props.pagination);
  }
  
  async function getCompanies(paginationTable: PaginationTable) {
    const paginationQuery: ProductsQueryParams = {
      SortBy: paginationTable.sortBy,
      Descending: paginationTable.descending,
      SearchTerm: filter.value,
      PageNumber: paginationTable.page,
      PageSize: paginationTable.rowsPerPage,
    };
  
    isLoadingCompanies.value = true;
    const { data, error } = await CompanyService.getCompanies(paginationQuery);
    isLoadingCompanies.value = false;
  
    if (error) {
      handleError(error, 'Loading recipes failed');
      return;
    }
  
    companyPaginated.value = data;
    pagination.value.rowsNumber = data.totalCount ?? 0;
    pagination.value.sortBy = paginationTable.sortBy;
    pagination.value.descending = paginationTable.descending;
    pagination.value.page = data.currentPage;
    pagination.value.rowsPerPage = data.pageSize;
  }
  getCompanies(pagination.value);
  
  const isCreateDialogOpen = ref(false);
  */

import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import { ref } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import CompanyService, {Company} from '@/api/services/CompanyService';  // Corrected import
import { handleError } from '@/utils/error-handler';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import CreateCompanyDialog from '@/components/companies/CreateCompanyDialog.vue';
import CompanyTable from '@/components/companies/CompanyTable.vue';

const { t } = useI18n();
const filter = ref('');

// Pagination state
const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});

const companyPaginated = ref<Company[]>([]);
const isLoadingCompanies = ref(false);

// Fetch companies with pagination
async function getCompanies(paginationTable: PaginationTable) {
  const paginationQuery = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  isLoadingCompanies.value = true;

  // Call CompanyService with the pagination parameters
  isLoadingCompanies.value = true;
  const { data, error } = await CompanyService.getCompanies();
  console.log(data);
  isLoadingCompanies.value = false;

  if (error) {
    handleError(error, 'Loading recipes failed');
    return;
  }

  // Update the paginated data
  companyPaginated.value = data;
  /*pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;*/
}

// Initial load
getCompanies(pagination.value);

// Triggered when the pagination or filter changes
async function onRequest(props: { pagination: PaginationClient }) {
  await getCompanies(props.pagination);
}

const isCreateDialogOpen = ref(false);
  </script>
  
  