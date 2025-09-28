<template>
  <q-select
    v-model="selectedRecipe"
    :label="t('recipe.label')"
    :options="recipeOptions"
    :loading="loadingRecipes"
    use-input
    option-label="name"
    option-value="id"
    :input-debounce="400"
    @filter="filterFn"
    @virtual-scroll="getOnScroll"
  />
</template>

<script setup lang="ts">
import { computed, nextTick, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import RecipeService from '@/api/services/RecipeService';
import { handleError } from '@/utils/error-handler';
import { QSelect } from 'quasar';
import type { RecipesQueryParams, RecipesResponse } from '@/api/services/RecipeService';

export interface RecipeSelectData {
  id?: string;
  name?: string;
}

const props = defineProps({
  deviceId: {
    type: String,
    required: false,
  },
});

const { t } = useI18n();

const selectedRecipe = defineModel<RecipeSelectData>({ required: false });

const recipes = ref<RecipesResponse['items']>([]);
const loadingRecipes = ref(false);
const filter = ref('');
const nextPage = ref(1);
const lastPage = ref(1);

async function getOnScroll({ to, ref }: { to: number; ref: QSelect | null }) {
  const lastIndex = (recipes.value.length ?? 0) - 1;

  if (loadingRecipes.value || nextPage.value > lastPage.value || lastIndex != to) return;

  const queryParams: RecipesQueryParams = {
    SortBy: 'name',
    Descending: false,
    SearchTerm: filter.value,
    PageNumber: nextPage.value,
    PageSize: 50,
    DeviceId: props.deviceId,
  };

  loadingRecipes.value = true;
  const { data, error } = await RecipeService.getRecipes(queryParams);
  loadingRecipes.value = false;

  if (error) {
    handleError(error, 'Loading recipes failed');
    return;
  }

  if (data) {
    recipes.value = [...(recipes.value ?? []), ...(data.items ?? [])];
  }

  nextPage.value++;
  lastPage.value = data.totalPages ?? 1;

  if (ref) {
    await nextTick();
    ref.refresh();
  }
}
getOnScroll({ to: -1, ref: null });

async function filterFn(
  val: string,
  doneFn: (callbackFn: () => void, afterFn?: ((ref: QSelect) => void)  ) => void,
) {
  if (val === filter.value) {
    doneFn(() => {});
    return;
  }

  filter.value = val;
  nextPage.value = 1;
  lastPage.value = 1;
  recipes.value = [];
  await getOnScroll({ to: -1, ref: null });
  doneFn(() => {});
}

const recipeOptions = computed(() => {
  return recipes.value?.map((recipe) => ({
    name: recipe.name,
    id: recipe.id,
  }));
});
</script>
