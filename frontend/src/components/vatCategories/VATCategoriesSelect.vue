<template>
  <q-select
    v-model="selected"
    :label="t('vat_category.label')"
    :options="options"
    use-input
    option-label="name"
    option-value="id"
    :input-debounce="400"
    :rules="selectRules"
    :display-value="selected ? `${selected.name} (${selected.rate}%)` : ''"
  />
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { handleError } from '@/utils/error-handler';
import { QSelect } from 'quasar';
import type { VATCategoryResponse } from '@/api/services/VATCategoryService';
import VATCategoryService from '@/api/services/VATCategoryService';

export interface VATCategorySelectData {
  id: string;
  name: string;
  rate: number;
}

const { t } = useI18n();

const selected = defineModel<VATCategorySelectData | null>({ required: false });

const items = ref<VATCategoryResponse>([]);

const options = computed(() => {
  return items.value?.map((i) => ({
    name: i.name,
    id: i.id,
    rate: i.rate,
  }));
});

loadItems();

function loadItems() {
  VATCategoryService.getVATCategories()
    .then((response) => {
      items.value = response.data!;
    })
    .catch((error) => {
      handleError(error, 'Loading VATCategories failed');
    });
}

const selectRules = [(val: VATCategorySelectData) => !!val || t('global.rules.required')];
</script>
