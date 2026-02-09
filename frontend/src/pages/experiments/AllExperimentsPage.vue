<template>
  <PageLayout :breadcrumbs="[{ label: t('experiment.label', 2) }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('experiment.create_experiment')"
        :icon="mdiPlus"
        @click="isCreateDialogOpen = true"
      />
    </template>

    <template #default>
      <ExperimentsTable
        v-model:pagination="pagination"
        :experiments="experimentsPaginated"
        :loading="isLoadingExperiments"
        class="shadow"
        @on-request="onRequest"
        @on-edit="openUpdateDialog"
        @on-delete="openDeleteDialog"
      />
    </template>
  </PageLayout>

  <CreateExperimentDialog v-model="isCreateDialogOpen" @on-create="getExperiments(pagination)" />
  <UpdateExperimentDialog
    v-if="experimentToUpdate"
    v-model="isUpdateDialogOpen"
    :experiment-id="experimentToUpdate"
    @on-update="getExperiments(pagination)"
  />
  <DeleteExperimentDialog
    v-if="experimentToDelete"
    v-model="isDeleteDialogOpen"
    :experiment-id="experimentToDelete"
    @on-deleted="getExperiments(pagination)"
  />
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import { watchDebounced } from '@vueuse/core';
import PageLayout from '@/layouts/PageLayout.vue';
import SearchBar from '@/components/core/SearchBar.vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import type { ExperimentsQueryParams, ExperimentsResponse } from '@/api/services/ExperimentService';
import ExperimentService from '@/api/services/ExperimentService';
import { handleError } from '@/utils/error-handler';
import ExperimentsTable from '@/components/experiments/ExperimentsTable.vue';
import CreateExperimentDialog from '@/components/experiments/CreateExperimentDialog.vue';
import UpdateExperimentDialog from '@/components/experiments/UpdateExperimentDialog.vue';
import DeleteExperimentDialog from '@/components/experiments/DeleteExperimentDialog.vue';

const { t } = useI18n();

const filter = ref('');
const isCreateDialogOpen = ref(false);
const isUpdateDialogOpen = ref(false);
const isDeleteDialogOpen = ref(false);
const experimentToUpdate = ref<string | null>(null);
const experimentToDelete = ref<string | null>(null);

const pagination = ref<PaginationClient>({
  sortBy: 'updatedAt',
  descending: true,
  page: 1,
  rowsPerPage: 20,
  rowsNumber: 0,
});

const experimentsPaginated = ref<ExperimentsResponse>();
const isLoadingExperiments = ref(false);

async function onRequest(props: { pagination: PaginationClient }) {
  await getExperiments(props.pagination);
}

function openUpdateDialog(experimentId: string) {
  experimentToUpdate.value = experimentId;
  isUpdateDialogOpen.value = true;
}

function openDeleteDialog(experimentId: string) {
  experimentToDelete.value = experimentId;
  isDeleteDialogOpen.value = true;
}

async function getExperiments(paginationTable: PaginationTable) {
  const paginationQuery: ExperimentsQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  isLoadingExperiments.value = true;
  const { data, error } = await ExperimentService.getExperiments(paginationQuery);
  isLoadingExperiments.value = false;

  if (error) {
    handleError(error, t('experiment.toasts.load_failed'));
    return;
  }

  experimentsPaginated.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.page = data.currentPage ?? paginationTable.page;
  pagination.value.rowsPerPage = data.pageSize ?? paginationTable.rowsPerPage;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
}

getExperiments(pagination.value);

watchDebounced(
  filter,
  async () => {
    pagination.value.page = 1;
    await getExperiments(pagination.value);
  },
  { debounce: 400 },
);
</script>
