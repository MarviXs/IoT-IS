<template>
  <q-select
    v-model="selected"
    :label="t('supplier.label')"
    :options="options"
    use-input
    option-label="name"
    option-value="id"
    :input-debounce="400"
    :rules="selectRules"
  />
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { handleError } from '@/utils/error-handler';
import { QSelect } from 'quasar';
import VATCategoryService, { VATCategoryResponse } from '@/api/services/VATCategoryService';

export interface VATCategorySelectData {
  id: number;
  name: string;
  amnout: number;
}

const { t } = useI18n();

const selected = defineModel<VATCategorySelectData>({ required: false });

const items = ref<VATCategoryResponse>([]);

const options = computed(() => {
  return items.value?.map((i) => ({
    name: i.name,
    id: i.id,
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
