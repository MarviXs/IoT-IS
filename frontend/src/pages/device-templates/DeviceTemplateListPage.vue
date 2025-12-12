<template>
  <PageLayout :breadcrumbs="[{ label: t('device_template.label', 2), to: '/device-templates' }]">
    <template #actions>
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        label="Import Template"
        :icon="mdiPlus"
        @click="importTemplateDialogOpen = true"
      />
      <q-btn
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
            <RouterLink v-if="props.row.canEdit" :to="`/device-templates/${props.row.id}`">
              {{ props.row.name }}
            </RouterLink>
            <span v-else>{{ props.row.name }}</span>
          </q-td>
        </template>

        <template #body-cell-isGlobal="props">
          <q-td :props="props">
            <q-badge v-if="props.row.isGlobal" color="primary" outline>{{ t('device_template.global') }}</q-badge>
            <span v-else>{{ t('global.no') }}</span>
          </q-td>
        </template>

        <template #body-cell-actions="props">
          <q-td auto-width :props="props">
            <q-btn
              :icon="mdiDownload"
              color="grey-color"
              flat
              round
              class="q-mr-sm"
              :loading="exportingId === props.row.id"
              @click.stop="exportTemplate(props.row.id, props.row.name)"
            >
              <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('device_template.export') }}
              </q-tooltip>
            </q-btn>
            <template v-if="props.row.canEdit">
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
            </template>
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
  <ImportDeviceTemplateDialog v-model="importTemplateDialogOpen" @on-imported="getTemplates(pagination)" />
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { mdiPlus, mdiCodeTags, mdiPencil, mdiTrashCanOutline, mdiDownload } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { computed, ref } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';
import { handleError } from '@/utils/error-handler';
import type { DeviceTemplatesQueryParams, DeviceTemplatesResponse } from '@/api/services/DeviceTemplateService';
import type { QTableProps } from 'quasar';
import DeleteDeviceTemplateDialog from '@/components/device-templates/DeleteDeviceTemplateDialog.vue';
import ImportDeviceTemplateDialog from '@/components/device-templates/ImportDeviceTemplateDialog.vue';
import { watchDebounced } from '@vueuse/core';
import type { ProblemDetails } from '@/api/types/ProblemDetails';

const { t, locale } = useI18n();
const filter = ref('');

const pagination = ref<PaginationClient>({
  sortBy: 'updatedAt',
  descending: true,
  page: 1,
  rowsPerPage: 20,
  rowsNumber: 0,
});
const templatesPaginated = ref<DeviceTemplatesResponse>();
const templates = computed(() => templatesPaginated.value?.items ?? []);

const importTemplateDialogOpen = ref(false);

const loadingTemplates = ref(false);
const deleteDialogOpen = ref(false);
const deleteTemplateId = ref<string>();
const exportingId = ref<string | null>(null);

async function getTemplates(paginationTable: PaginationTable) {
  const paginationQuery: DeviceTemplatesQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  loadingTemplates.value = true;
  const { data, error } = await DeviceTemplateService.getDeviceTemplates(paginationQuery);
  loadingTemplates.value = false;

  if (error) {
    handleError(error, 'Loading templates failed');
    return;
  }

  templatesPaginated.value = data;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
  pagination.value.rowsNumber = data.totalCount ?? 0;
}
getTemplates(pagination.value);

function openDeleteDialog(id: string) {
  deleteTemplateId.value = id;
  deleteDialogOpen.value = true;
}

async function exportTemplate(templateId: string, templateName: string) {
  exportingId.value = templateId;
  try {
    const { data, error } = await DeviceTemplateService.exportDeviceTemplate(templateId);

    if (!data || error) {
      const problem = error ?? ({ title: t('device_template.toasts.export_failed') } as ProblemDetails);
      handleError(problem, t('device_template.toasts.export_failed'));
      return;
    }

    const filename = `${slugify(templateName)}.json`;
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
  } finally {
    exportingId.value = null;
  }
}

function slugify(value: string) {
  return value
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '') || 'device-template';
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
    name: 'isGlobal',
    label: t('device_template.global'),
    field: 'isGlobal',
    sortable: true,
    align: 'left',
  },

  {
    name: 'updatedAt',
    label: 'Updated At',
    field: 'updatedAt',
    sortable: true,
    format(val) {
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
