<template>
  <q-table
    v-model:pagination.sync="pagination"
    :rows="commands"
    :columns="availableCommandsColumns"
    :loading="isLoadingCommands"
    flat
    binary-state-sort
    hide-header
    :no-data-label="t('recipe.no_commands_for_device_type')"
    :loading-label="t('table.loading_label')"
    :rows-per-page-label="t('table.rows_per_page_label')"
    :rows-per-page-options="[20, 50, 100, 0]"
    class="shadow"
  >
    <template #body-cell-actions="propsActions">
      <q-td auto-width :props="propsActions">
        <q-btn dense :icon="mdiPlus" color="primary" flat round @click="addCommandToSteps(propsActions.row)" />
      </q-td>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import CommandService from '@/api/services/CommandService';
import { RecipeResponse } from '@/api/types/Recipe';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import { useRoute } from 'vue-router';
import { CommandResponse, CommandsQueryParams, CommandsResponse } from '@/api/types/Command';
import { handleError } from '@/utils/error-handler';

const { t } = useI18n();
const route = useRoute();

const props = defineProps({
  deviceTemplateId: {
    type: String,
    required: true,
  },
});

const recipe = defineModel<RecipeResponse>({ required: true });

const pagination = ref<PaginationClient>({
  sortBy: 'updatedAt',
  descending: true,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});

const commandsPaginated = ref<CommandsResponse>();
const commands = computed(() => commandsPaginated.value?.items ?? []);
const isLoadingCommands = ref(false);
async function getCommands(paginationReq: PaginationTable) {
  const query: CommandsQueryParams = {
    DeviceTemplateId: props.deviceTemplateId,
    SearchTerm: '',
    SortBy: paginationReq.sortBy,
    Descending: paginationReq.descending,
    PageNumber: paginationReq.page,
    PageSize: paginationReq.rowsPerPage,
  };

  isLoadingCommands.value = true;
  const { data, error } = await CommandService.getCommands(query);
  isLoadingCommands.value = false;

  if (error) {
    handleError(error, 'Loading commands failed');
    return;
  }

  commandsPaginated.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.sortBy = paginationReq.sortBy;
  pagination.value.descending = paginationReq.descending;
  pagination.value.page = paginationReq.page;
  pagination.value.rowsPerPage = paginationReq.rowsPerPage;
}
getCommands(pagination.value);

function addCommandToSteps(command: CommandResponse) {
  if (!recipe.value.steps) recipe.value.steps = [];

  recipe.value.steps.push({
    id: null!,
    command: command,
    subrecipe: null!,
    cycles: 1,
    order: recipe.value.steps.length,
  });
}

const availableCommandsColumns = computed<QTableProps['columns']>(() => [
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
  {
    name: 'name',
    label: t('global.name'),
    field: 'displayName',
    sortable: true,
    align: 'left',
  },
]);
</script>
