<template>
  <div class="actions">
    <div class="title">{{ t('recipe.label', 2) }}</div>
    <q-badge v-if="!templateCanEdit" color="warning" text-color="black">
      {{ t('device_template.read_only_synced') }}
    </q-badge>
    <q-space />
    <SearchBar v-model="filter" />
    <q-btn
      v-if="templateCanEdit"
      class="shadow"
      color="primary"
      :icon="mdiPlus"
      :label="t('recipe.create_recipe')"
      :to="`/device-templates/${deviceTemplateId}/recipes/create`"
      unelevated
      no-caps
      size="15px"
    />
  </div>
  <div>
    <q-table
      v-model:pagination="pagination"
      :rows="recipes"
      :columns="columns"
      :loading="isLoadingRecipes"
      flat
      binary-state-sort
      :rows-per-page-options="[10, 20, 50]"
      class="shadow"
      :no-data-label="t('table.no_data_label')"
      :loading-label="t('table.loading_label')"
      :rows-per-page-label="t('table.rows_per_page_label')"
      @request="(requestProp) => getRecipes(requestProp.pagination)"
    >
      <template #no-data="{ message }">
        <div class="full-width column flex-center q-pa-lg nothing-found-text">
          <q-icon :name="mdiBookOutline" class="q-mb-md" size="2rem"></q-icon>
          {{ message }}
        </div>
      </template>

      <template #body-cell-name="props">
        <q-td :props="props">
          <RouterLink
            v-if="templateCanEdit"
            class="text-black"
            :to="`/device-templates/${deviceTemplateId}/recipes/${props.row.id}/edit`"
          >
            {{ props.row.name }}
          </RouterLink>
          <span v-else>{{ props.row.name }}</span>
        </q-td>
      </template>

      <template #body-cell-actions="props">
        <q-td auto-width :props="props">
          <q-btn
            v-if="templateCanEdit"
            :to="`/device-templates/${deviceTemplateId}/recipes/${props.row.id}/edit`"
            :icon="mdiPencil"
            color="grey-color"
            flat
            round
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.edit') }}
            </q-tooltip>
          </q-btn>
          <q-btn
            v-if="templateCanEdit"
            :icon="mdiTrashCanOutline"
            color="grey-color"
            flat
            round
            @click.stop="openDeleteDialog(props.row.id)"
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.delete') }}
            </q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </q-table>
  </div>
  <DeleteRecipeDialog
    v-if="recipeToDelete"
    v-model="deleteDialogOpen"
    :recipe-id="recipeToDelete"
    @on-deleted="getRecipes(pagination)"
  />
</template>

<script setup lang="ts">
import type { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiBookOutline, mdiPencil, mdiTrashCanOutline } from '@quasar/extras/mdi-v7';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import DeleteRecipeDialog from '@/components/recipes/DeleteRecipeDialog.vue';
import SearchBar from '@/components/core/SearchBar.vue';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import type { RecipesQueryParams, RecipesResponse } from '@/api/services/RecipeService';
import { useRoute } from 'vue-router';
import RecipeService from '@/api/services/RecipeService';
import { handleError } from '@/utils/error-handler';
import { watchDebounced } from '@vueuse/core';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';

const { t } = useI18n();
const route = useRoute();
const deviceTemplateId = route.params.id as string;
const templateCanEdit = ref(true);

const filter = ref('');
const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 20,
  rowsNumber: 0,
});

const recipesPaginated = ref<RecipesResponse>();
const recipes = computed(() => recipesPaginated.value?.items ?? []);
const isLoadingRecipes = ref(false);
async function getRecipes(paginationTable: PaginationTable) {
  const paginationQuery: RecipesQueryParams = {
    DeviceTemplateId: deviceTemplateId,
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    SearchTerm: filter.value,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
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
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
}
getRecipes(pagination.value);
void loadTemplatePermissions();

async function loadTemplatePermissions() {
  const { data, error } = await DeviceTemplateService.getDeviceTemplate(deviceTemplateId);
  if (error || !data) {
    return;
  }

  templateCanEdit.value = data.canEdit ?? true;
}

const recipeToDelete = ref<string>();
const deleteDialogOpen = ref(false);
function openDeleteDialog(commandId: string) {
  if (!templateCanEdit.value) {
    return;
  }

  recipeToDelete.value = commandId;
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
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);

watchDebounced(filter, () => getRecipes(pagination.value), { debounce: 400 });
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
