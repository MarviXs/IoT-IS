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
        :companies="companyPaginated"
        :loading="isLoadingCompanies"
        @on-change="getCompanies(pagination)"
        @on-request="onRequest"
      />
    </template>
  </PageLayout>
  <CreateCompanyDialog v-model="isCreateDialogOpen" @on-create="getCompanies(pagination)" />
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import { ref } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import CompanyService from '@/api/services/CompanyService';
import { handleError } from '@/utils/error-handler';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import CreateCompanyDialog from '@/components/companies/CreateCompanyDialog.vue';
import CompanyTable from '@/components/companies/CompanyTable.vue';

const { t } = useI18n();
const filter = ref('');

const pagination = ref<PaginationClient>({
  sortBy: '',
  descending: false,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});

const companyPaginated = ref<any>([]);
const isLoadingCompanies = ref(false);

async function getCompanies(paginationTable: PaginationTable) {
  const paginationQuery = {
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
    handleError(error, 'Loading companies failed');
    return;
  }

  companyPaginated.value = data.items;
  pagination.value.rowsNumber = data.totalCount;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
}

getCompanies(pagination.value);

async function onRequest(props: { pagination: PaginationClient }) {
  await getCompanies(props.pagination);
}

const isCreateDialogOpen = ref(false);
</script>
