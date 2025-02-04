<template>
  <q-select
    v-model="selected"
    :label="t('product.label')"
    :options="options"
    :loading="isLoading"
    use-input
    clearable
    option-label="name"
    option-value="id"
    :input-debounce="400"
    @filter="filterFn"
    @virtual-scroll="onScroll"
  />
</template>

<script setup lang="ts">
import { computed, nextTick, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { handleError } from '@/utils/error-handler';
import { QSelect } from 'quasar';
import ProductCategoryService, {
  ProductCategoryParams,
  ProductCategoryResponse,
} from '@/api/services/ProductCategoryService';

export interface ProductCategorySelectData {
  id: string;
  name: string;
}

const { t } = useI18n();

const selected = defineModel<ProductCategorySelectData | null>({ required: false });

const items = ref<ProductCategoryResponse['items']>([]);
const isLoading = ref(false);
const filter = ref('');
const nextPage = ref(1);
const lastPage = ref(1);

const options = computed(() => {
  return items.value?.map((i) => ({
    name: i.name,
    id: i.id,
  }));
});

async function onScroll({ to, ref }: { to: number; ref: QSelect | null }) {
  const lastIndex = (items.value.length ?? 0) - 1;

  if (isLoading.value || nextPage.value > lastPage.value || lastIndex != to) return;

  const paginationQuery: ProductCategoryParams = {
    SortBy: 'CategoryName',
    Descending: false,
    SearchTerm: filter.value,
    PageNumber: nextPage.value,
    PageSize: 50,
  };

  isLoading.value = true;
  const { data, error } = await ProductCategoryService.getProductCategories(paginationQuery);
  isLoading.value = false;

  if (error) {
    handleError(error, 'Loading categories failed');
    return;
  }

  if (data) {
    items.value = [...(items.value ?? []), ...(data.items ?? [])];
  }

  nextPage.value++;
  lastPage.value = data.totalPages ?? 1;

  if (ref) {
    await nextTick();
    ref.refresh();
  }
}
onScroll({ to: -1, ref: null });

async function filterFn(
  val: string,
  doneFn: (callbackFn: () => void, afterFn?: ((ref: QSelect) => void) | undefined) => void,
) {
  if (val === filter.value) {
    doneFn(() => {});
    return;
  }

  filter.value = val;
  nextPage.value = 1;
  lastPage.value = 1;
  items.value = [];
  await onScroll({ to: -1, ref: null });
  doneFn(() => {});
}
</script>
