<template>
  <PageLayout :breadcrumbs="[{ label: t('scene.label', 2), to: '/scenes' }]">
    <template #actions>
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('scene.create_scene')"
        :icon="mdiPlus"
        to="/scenes/create"
      />
    </template>
    <template #default>
      <q-table
        v-model:pagination="pagination"
        :rows="[]"
        :columns="columns"
        :loading="loadingScenes"
        flat
        binary-state-sort
        :rows-per-page-options="[10, 20, 50]"
        class="shadow"
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
        :rows-per-page-label="t('table.rows_per_page_label')"
        @request="(requestProp) => getScenes(requestProp.pagination)"
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiCodeTags" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>

        <template #body-cell-name="props">
          <q-td :props="props">
            <RouterLink :to="`/scenes/${props.row.id}`">{{ props.row.name }}</RouterLink>
          </q-td>
        </template>

        <template #body-cell-actions="props">
          <q-td auto-width :props="props">
            <q-btn :icon="mdiPencil" color="grey-color" flat round :to="`/scenes/${props.row.id}`">
              <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.edit') }}
              </q-tooltip>
            </q-btn>
            <q-btn :icon="mdiTrashCanOutline" color="grey-color" flat round @click="openDeleteDialog(props.row.id)"
              ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.delete') }}
              </q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { mdiPlus, mdiCodeTags, mdiPencil, mdiTrashCanOutline } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { computed, ref } from 'vue';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import { handleError } from '@/utils/error-handler';
import { DeviceTemplatesQueryParams, DeviceTemplatesResponse } from '@/api/services/DeviceTemplateService';
import { QTableProps } from 'quasar';
import { watchDebounced } from '@vueuse/core';
import SceneService from '@/api/services/SceneService';

const { t, locale } = useI18n();
const filter = ref('');

const pagination = ref<PaginationClient>({
  sortBy: 'updatedAt',
  descending: true,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});
const scenesPaginated = ref<DeviceTemplatesResponse>();
const scenes = computed(() => scenesPaginated.value?.items ?? []);

const loadingScenes = ref(false);
const deleteDialogOpen = ref(false);
const deleteSceneId = ref<string>();

async function getScenes(paginationTable: PaginationTable) {
  const paginationQuery: DeviceTemplatesQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  loadingScenes.value = true;
  const { data, error } = await SceneService.getScenes(paginationQuery);
  loadingScenes.value = false;

  if (error) {
    handleError(error, 'Loading templates failed');
    return;
  }

  scenesPaginated.value = data;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
  pagination.value.rowsNumber = data.totalCount ?? 0;
}
getScenes(pagination.value);

function openDeleteDialog(id: string) {
  deleteSceneId.value = id;
  deleteDialogOpen.value = true;
}

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'name',
    label: t('global.name'),
    field: 'name',
    sortable: true,
    align: 'left',
  },

  {
    name: 'updatedAt',
    label: 'Updated At',
    field: 'updatedAt',
    sortable: true,
    format(val, row) {
      return new Date(val).toLocaleString(locale.value);
    },
    align: 'right',
  },

  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);

watchDebounced(filter, () => getScenes(pagination.value), { debounce: 400 });
</script>
