<template>
  <PageLayout :breadcrumbs="[{ label: t('global.account') }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
    </template>
    <template #default>
      <q-table
        v-model:pagination="pagination"
        :rows="usersPaginated?.items ?? []"
        :columns="columns"
        :loading="isLoading"
        flat
        binary-state-sort
        :rows-per-page-options="[10, 20, 50]"
        @request="(requestProp) => getUsers(requestProp.pagination)"
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiAccountGroup" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>
        <template #body-cell-actions="props">
          <q-td auto-width :props="props">
            <q-btn :to="`/user-management/${props.row.id}`" :icon="mdiPencil" color="grey-color" flat round
              ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.edit') }}
              </q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiAccountGroup, mdiPencil } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import SearchBar from '@/components/core/SearchBar.vue';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import { UsersQueryParams, UsersResponse } from '@/api/services/UserManagementService';
import UserManagementService from '@/api/services/UserManagementService';
import { handleError } from '@/utils/error-handler';
import { watchDebounced } from '@vueuse/core';

const { t, locale } = useI18n();

const filter = ref('');

const pagination = ref<PaginationClient>({
  sortBy: 'email',
  descending: false,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});
const usersPaginated = ref<UsersResponse>();
const isLoading = ref(false);

async function getUsers(paginationTable: PaginationTable) {
  const paginationQuery: UsersQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  isLoading.value = true;
  const { data, error } = await UserManagementService.getUsers(paginationQuery);
  isLoading.value = false;

  if (error) {
    handleError(error, 'Loading recipes failed');
    return;
  }

  usersPaginated.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
}
getUsers(pagination.value);

watchDebounced(filter, () => getUsers(pagination.value), { debounce: 400 });

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'email',
    label: 'E-mail',
    field: 'email',
    sortable: true,
    align: 'left',
  },
  {
    name: 'roles',
    label: 'Role',
    field: 'roles',
    sortable: false,
    align: 'left',
    format(val) {
      if (!val || val.length === 0) return 'User';
      return val.join(', ');
    },
  },

  {
    name: 'registrationDate',
    label: 'Registration date',
    field: 'registrationDate',
    sortable: true,
    align: 'right',
    format(val) {
      return new Date(val).toLocaleDateString(locale.value);
    },
  },
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);
</script>
