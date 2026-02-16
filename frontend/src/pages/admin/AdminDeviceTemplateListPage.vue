<template>
  <PageLayout :breadcrumbs="[{ label: t('device_template.all_device_templates'), to: '/admin/device-templates' }]">
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
            <RouterLink :to="`/device-templates/${props.row.id}`">{{ props.row.name }}</RouterLink>
          </q-td>
        </template>

        <template #body-cell-owner="propsOwner">
          <q-td :props="propsOwner">
            {{ propsOwner.row.ownerEmail ?? '—' }}
          </q-td>
        </template>

        <template #body-cell-isGlobal="propsGlobal">
          <q-td :props="propsGlobal">
            <q-badge v-if="propsGlobal.row.isGlobal" color="primary" outline>{{ t('device_template.global') }}</q-badge>
            <span v-else>{{ t('global.no') }}</span>
          </q-td>
        </template>

        <template #body-cell-updatedAt="propsUpdated">
          <q-td :props="propsUpdated">
            {{ new Date(propsUpdated.row.updatedAt).toLocaleString(locale) }}
          </q-td>
        </template>

        <template #body-cell-actions="propsActions">
          <q-td auto-width :props="propsActions">
            <q-btn
              :icon="mdiDownload"
              color="grey-color"
              flat
              round
              class="q-mr-sm"
              :loading="exportingId === propsActions.row.id"
              @click.stop="exportTemplate(propsActions.row.id, propsActions.row.name)"
            >
              <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('device_template.export') }}
              </q-tooltip>
            </q-btn>
            <q-btn
              v-if="canEditTemplate(propsActions.row)"
              :icon="mdiPencil"
              color="grey-color"
              flat
              round
              :to="`/device-templates/${propsActions.row.id}`"
            >
              <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.edit') }}
              </q-tooltip>
            </q-btn>
            <q-btn v-if="canEditTemplate(propsActions.row)" :icon="mdiDotsVertical" color="grey-color" flat round>
              <q-menu anchor="bottom right" self="top right">
                <q-list>
                  <q-item clickable v-close-popup @click="openChangeOwnerDialog(propsActions.row)">
                    <div class="row items-center q-gutter-sm">
                      <q-icon color="grey-9" size="24px" :name="mdiAccountSwitch" />
                      <div>{{ t('device_template.change_owner') }}</div>
                    </div>
                  </q-item>
                  <q-item clickable v-close-popup @click="openDeleteDialog(propsActions.row.id)">
                    <div class="row items-center q-gutter-sm">
                      <q-icon color="grey-9" size="24px" :name="mdiTrashCanOutline" />
                      <div>{{ t('global.delete') }}</div>
                    </div>
                  </q-item>
                </q-list>
              </q-menu>
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
  <ImportDeviceTemplateDialog v-model="importTemplateDialogOpen" @on-imported="getTemplates(pagination)" />
  <ChangeDeviceTemplateOwnerDialog
    v-if="templateToChangeOwner"
    v-model="changeOwnerDialogOpen"
    :template-id="templateToChangeOwner.id"
    :current-owner-id="templateToChangeOwner.ownerId"
    :current-owner-email="templateToChangeOwner.ownerEmail"
    @on-changed="getTemplates(pagination)"
  />
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { mdiPlus, mdiCodeTags, mdiPencil, mdiDotsVertical, mdiAccountSwitch, mdiTrashCanOutline, mdiDownload } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { computed, ref, watch } from 'vue';
import SearchBar from '@/components/core/SearchBar.vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import { handleError } from '@/utils/error-handler';
import DeleteDeviceTemplateDialog from '@/components/device-templates/DeleteDeviceTemplateDialog.vue';
import ImportDeviceTemplateDialog from '@/components/device-templates/ImportDeviceTemplateDialog.vue';
import ChangeDeviceTemplateOwnerDialog from '@/components/device-templates/ChangeDeviceTemplateOwnerDialog.vue';
import AdminDeviceTemplateService, {
  type AdminDeviceTemplatesQueryParams,
  type AdminDeviceTemplatesResponse,
} from '@/api/services/AdminDeviceTemplateService';
import type { QTableProps } from 'quasar';
import { watchDebounced } from '@vueuse/core';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';
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

const templatesPaginated = ref<AdminDeviceTemplatesResponse>();
const templates = computed(() => templatesPaginated.value?.items ?? []);

const importTemplateDialogOpen = ref(false);

const loadingTemplates = ref(false);
const deleteDialogOpen = ref(false);
const deleteTemplateId = ref<string>();
const exportingId = ref<string | null>(null);

const changeOwnerDialogOpen = ref(false);
const templateToChangeOwner = ref<
  | {
      id: string;
      ownerId: string;
      ownerEmail?: string | null;
    }
  | null
>(null);

async function getTemplates(paginationTable: PaginationTable) {
  const paginationQuery: AdminDeviceTemplatesQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  loadingTemplates.value = true;
  const { data, error } = await AdminDeviceTemplateService.getDeviceTemplates(paginationQuery);
  loadingTemplates.value = false;

  if (error) {
    handleError(error, t('device_template.toasts.loading_failed'));
    return;
  }

  if (!data) {
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

function openChangeOwnerDialog(template: NonNullable<AdminDeviceTemplatesResponse['items']>[number]) {
  if (!canEditTemplate(template)) {
    return;
  }

  templateToChangeOwner.value = {
    id: template.id,
    ownerId: template.ownerId,
    ownerEmail: template.ownerEmail,
  };
  changeOwnerDialogOpen.value = true;
}

watch(changeOwnerDialogOpen, (open) => {
  if (!open) {
    templateToChangeOwner.value = null;
  }
});

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

function canEditTemplate(template: NonNullable<AdminDeviceTemplatesResponse['items']>[number]): boolean {
  return template.canEdit !== false;
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
    name: 'owner',
    label: t('global.owner'),
    field: 'ownerEmail',
    sortable: false,
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
