<template>
  <div class="actions">
    <div class="title">{{ t('command.label', 2) }}</div>
    <q-space />
    <SearchBar v-model="filter" class="col-grow col-lg-auto" />
    <q-btn
      class="shadow col-grow col-lg-auto"
      color="primary"
      :icon="mdiPlus"
      :label="t('command.create_command')"
      unelevated
      no-caps
      size="15px"
      @click="createDialogOpen = true"
    />
  </div>
  <div>
    <q-table
      v-model:pagination="pagination"
      :rows="commands"
      :columns="columns"
      :loading="isLoadingCommands"
      flat
      binary-state-sort
      :rows-per-page-options="[10, 20, 50]"
      class="shadow"
      :no-data-label="t('table.no_data_label')"
      :loading-label="t('table.loading_label')"
      :rows-per-page-label="t('table.rows_per_page_label')"
      @request="(requestProp) => getCommands(requestProp.pagination)"
    >
      <template #no-data="{ message }">
        <div class="full-width column flex-center q-pa-lg nothing-found-text">
          <q-icon :name="mdiCodeTags" class="q-mb-md" size="2rem"></q-icon>
          {{ message }}
        </div>
      </template>

      <template #body-cell-actions="props">
        <q-td auto-width :props="props">
          <q-btn :icon="mdiPencil" color="grey-color" flat round @click.stop="openEditDialog(props.row.id)"
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.edit') }}
            </q-tooltip>
          </q-btn>
          <q-btn :icon="mdiTrashCanOutline" color="grey-color" flat round @click.stop="openDeleteDialog(props.row.id)"
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.delete') }}
            </q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </q-table>
  </div>
  <create-command-dialog v-model="createDialogOpen" @on-create="getCommands(pagination)" />
  <edit-command-dialog
    v-if="commandToEdit"
    v-model="editDialogOpen"
    :command-id="commandToEdit"
    @on-update="getCommands(pagination)"
  />
  <delete-command-dialog
    v-if="commandToDelete"
    v-model="deleteDialogOpen"
    :command-id="commandToDelete"
    @on-deleted="getCommands(pagination)"
  />
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiCodeTags, mdiPencil, mdiTrashCanOutline } from '@quasar/extras/mdi-v7';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import CreateCommandDialog from '@/components/commands/CreateCommandDialog.vue';
import EditCommandDialog from '@/components/commands/EditCommandDialog.vue';
import DeleteCommandDialog from '@/components/commands/DeleteCommandDialog.vue';
import CommandService from '@/api/services/CommandService';
import SearchBar from '@/components/core/SearchBar.vue';
import { useAuthStore } from '@/stores/auth-store';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import { CommandsQueryParams, CommandsResponse } from '@/api/services/CommandService';
import { handleError } from '@/utils/error-handler';
import { useRoute } from 'vue-router';
import { watchDebounced } from '@vueuse/core';

const { t } = useI18n();
const authStore = useAuthStore();
const route = useRoute();
const deviceTemplateId = route.params.id as string;

const filter = ref('');
const pagination = ref<PaginationClient>({
  sortBy: 'displayName',
  descending: false,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});

const commandsPaginated = ref<CommandsResponse>();
const commands = computed(() => commandsPaginated.value?.items ?? []);
const isLoadingCommands = ref(false);
async function getCommands(paginationTable: PaginationTable) {
  const paginationQuery: CommandsQueryParams = {
    DeviceTemplateId: deviceTemplateId,
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  isLoadingCommands.value = true;
  const { data, error } = await CommandService.getCommands(paginationQuery);
  isLoadingCommands.value = false;

  if (error) {
    handleError(error, 'Loading commands failed');
    return;
  }

  commandsPaginated.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
}
getCommands(pagination.value);

const createDialogOpen = ref(false);

const deleteDialogOpen = ref(false);
const commandToDelete = ref<string>();
function openDeleteDialog(commandId: string) {
  commandToDelete.value = commandId;
  deleteDialogOpen.value = true;
}

const editDialogOpen = ref(false);
const commandToEdit = ref<string>();
function openEditDialog(commandId: string) {
  commandToEdit.value = commandId;
  editDialogOpen.value = true;
}

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'displayName',
    label: t('global.display_name'),
    field: 'displayName',
    sortable: true,
    align: 'left',
  },
  {
    name: 'name',
    label: t('global.name'),
    field: 'name',
    sortable: true,
    align: 'left',
  },
  {
    name: 'parameters',
    label: t('command.parameters'),
    field: 'params',
    sortable: true,
    format(val, row) {
      if (val.length === 0) {
        return '-';
      }
      return `(${val.map((param: string[]) => param).join(', ')})`;
    },
    align: 'left',
  },
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);

watchDebounced(filter, () => getCommands(pagination.value), { debounce: 400 });
</script>

<style lang="scss" scoped>
.actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 0.75rem 1rem;
  margin-bottom: 1rem;
}

.title {
  font-size: 1.4rem;
  font-weight: 600;
  margin: 0;
  color: $secondary;
}
</style>
