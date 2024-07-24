<template>
  <PageLayout :breadcrumbs="[{ label: t('device_template.label', 2), to: '/device-templates' }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <q-btn
        v-if="authStore.isAdmin"
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        label="Create Template"
        :icon="mdiPlus"
        to="/device-templates/create"
      />
    </template>
    <template #default>
      <q-table
        v-model:pagination="pagination"
        :rows="templates"
        :columns="columns"
        :loading="loadingTemplates"
        flat
        binary-state-sort
        :rows-per-page-options="[10, 20, 50]"
        class="shadow"
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
        :rows-per-page-label="t('table.rows_per_page_label')"
        @request="(requestProp) => getTemplates(requestProp.pagination)"
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiCodeTags" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>

        <template #body-cell-name="props">
          <q-td :props="props">
            <RouterLink :to="`/device-templates/${props.row.id}`">{{ props.row.name }}</RouterLink>
          </q-td>
        </template>

        <template #body-cell-actions="props">
          <q-td auto-width :props="props">
            <q-btn :icon="mdiPencil" color="grey-color" flat round :to="`/device-templates/${props.row.id}`">
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
  <DeleteDeviceTemplateDialog
    v-model="deleteDialogOpen"
    :template-id="deleteTemplateId"
    @on-deleted="getTemplates(pagination)"
  />
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useAuthStore } from '@/stores/auth-store';
import { mdiPlus, mdiCodeTags, mdiPencil, mdiTrashCanOutline } from '@quasar/extras/mdi-v6';
import PageLayout from '@/layouts/PageLayout.vue';
import { computed, ref } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';
import { handleError } from '@/utils/error-handler';
import { DeviceTemplatesQueryParams, DeviceTemplatesResponse } from '@/api/types/DeviceTemplate';
import { QTableProps } from 'quasar';
import DeleteDeviceTemplateDialog from '@/components/device-templates/DeleteDeviceTemplateDialog.vue';
import { watchDebounced } from '@vueuse/core';

const { t, locale } = useI18n();
const authStore = useAuthStore();
const filter = ref('');

const pagination = ref<PaginationClient>({
  sortBy: 'updatedAt',
  descending: true,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});
const templatesPaginated = ref<DeviceTemplatesResponse>();
const templates = computed(() => templatesPaginated.value?.items ?? []);

const loadingTemplates = ref(false);
const deleteDialogOpen = ref(false);

const deleteTemplateId = ref<string>();

async function getTemplates(paginationReq: PaginationTable) {
  const paginationQuery: DeviceTemplatesQueryParams = {
    SortBy: paginationReq.sortBy,
    Descending: paginationReq.descending,
    SearchTerm: filter.value,
    PageNumber: paginationReq.page,
    PageSize: paginationReq.rowsPerPage,
  };

  loadingTemplates.value = true;
  const { data, error } = await DeviceTemplateService.getDeviceTemplates(paginationQuery);
  loadingTemplates.value = false;

  if (error) {
    handleError(error, 'Loading templates failed');
    return;
  }

  templatesPaginated.value = data;
  pagination.value.sortBy = paginationReq.sortBy;
  pagination.value.descending = paginationReq.descending;
  pagination.value.page = paginationReq.page;
  pagination.value.rowsPerPage = paginationReq.rowsPerPage;
  pagination.value.rowsNumber = data.totalCount ?? 0;
}
getTemplates(pagination.value);

function openDeleteDialog(id: string) {
  deleteTemplateId.value = id;
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
    name: 'modelId',
    label: 'Model ID',
    field: 'modelId',
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

watchDebounced(filter, () => getTemplates(pagination.value), { debounce: 400 });
</script>
