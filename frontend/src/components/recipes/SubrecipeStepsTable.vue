<template>
  <q-table
    v-model:pagination="pagination"
    :rows="recipes"
    :columns="availableSubrecipesColumns"
    :loading="isLoadingRecipes"
    flat
    hide-header
    :no-data-label="t('recipe.no_subrecipes_for_device_type')"
    :loading-label="t('table.loading_label')"
    :rows-per-page-label="t('table.rows_per_page_label')"
    :rows-per-page-options="[20, 50, 100, 0]"
    class="shadow"
  >
    <template #body-cell-actions="propsActions">
      <q-td auto-width :props="propsActions">
        <q-btn dense :icon="mdiPlus" color="primary" flat round @click="addSubrecipeToSteps(propsActions.row)" />
      </q-td>
    </template>
    <template #body-cell-name="propsName">
      <q-td :props="propsName">
        <q-tree
          :nodes="[
            subrecipeToNodes(
              {
                id: propsName.row.id,
                subrecipe: propsName.row,
                cycles: 0,
                order: 0,
              },
              true,
            ),
          ]"
          node-key="id"
          @lazy-load="lazyLoadSubrecipe"
        >
          <template #default-header="treeProp">
            <span>{{ treeProp.node.label }}</span>
            <template v-if="treeProp.node.cycles > 1 && !treeProp.node.root">
              <span class="text-grey-8">&nbsp;({{ treeProp.node.cycles }}x)</span>
            </template>
          </template>
        </q-tree>
      </q-td>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiPlus } from '@quasar/extras/mdi-v6';
import { RecipeResponse, RecipesQueryParams, RecipesResponse } from '@/api/types/Recipe';
import { subrecipeToNodes, lazyLoadSubrecipe } from '@/utils/subrecipe-nodes';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import { useRoute } from 'vue-router';
import RecipeService from '@/api/services/RecipeService';
import { handleError } from '@/utils/error-handler';

const { t } = useI18n();
const route = useRoute();

const recipe = defineModel<RecipeResponse>({ required: true });
const deviceTemplateId = route.params.id as string;

const pagination = ref<PaginationClient>({
  sortBy: 'updatedAt',
  descending: true,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});

const recipesPaginated = ref<RecipesResponse>();
const recipes = computed(() => recipesPaginated.value?.items ?? []);
const isLoadingRecipes = ref(false);
async function getRecipes(paginationReq: PaginationTable) {
  const paginationQuery: RecipesQueryParams = {
    DeviceTemplateId: deviceTemplateId,
    SortBy: paginationReq.sortBy,
    Descending: paginationReq.descending,
    PageNumber: paginationReq.page,
    PageSize: paginationReq.rowsPerPage,
  };

  isLoadingRecipes.value = true;
  const { data, error } = await RecipeService.getRecipes(paginationQuery);
  isLoadingRecipes.value = false;

  if (error) {
    handleError(error, 'Loading recipes failed');
    return;
  }

  recipesPaginated.value = data;
  pagination.value.rowsNumber = data.totalCount ?? 0;
  pagination.value.sortBy = paginationReq.sortBy;
  pagination.value.descending = paginationReq.descending;
  pagination.value.page = paginationReq.page;
  pagination.value.rowsPerPage = paginationReq.rowsPerPage;
}
getRecipes(pagination.value);

function addSubrecipeToSteps(subrecipe: RecipeResponse) {
  if (!recipe.value.steps) recipe.value.steps = [];

  recipe.value.steps.push({
    subrecipe: subrecipe,
    cycles: 1,
    order: recipe.value.steps.length,
  });
}

const availableSubrecipesColumns = computed<QTableProps['columns']>(() => [
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
    field: 'name',
    sortable: true,
    align: 'left',
  },
]);
</script>
