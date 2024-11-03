<template>
  <q-table
    v-model:pagination="pagination"
    :rows="devicesFiltered"
    :columns="columns"
    :loading="props.loading"
    flat
    :rows-per-page-options="[10, 20, 50]"
    :no-data-label="t('table.no_data_label')"
    :loading-label="t('table.loading_label')"
    :rows-per-page-label="t('table.rows_per_page_label')"
    binary-state-sort
    @request="emit('onRequest', $event)"
  >
    <template #no-data="{ message }">
      <div class="full-width column flex-center q-pa-lg nothing-found-text">
        <q-icon :name="mdiFileSearchOutline" class="q-mb-md" size="50px"></q-icon>
        {{ message }}
      </div>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { PropType, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiFileSearchOutline } from '@quasar/extras/mdi-v7';
import { PaginationClient } from '@/models/Pagination';
import { ProductsResponse } from '@/api/services/ProductService';

const props = defineProps({
  devices: {
    type: Object as PropType<ProductsResponse>,
    required: false,
    default: null,
  },
  loading: {
    type: Boolean,
    required: true,
  },
});

const pagination = defineModel<PaginationClient>('pagination');

const devicesFiltered = computed(() => props.devices?.items ?? []);

const emit = defineEmits(['onChange', 'onRequest']);

const { t } = useI18n();

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'pluCode',
    label: t('product.plu_code'),
    field: 'pluCode',
    align: 'left',
    sortable: true,
  },
  {
    name: 'czechName',
    label: t('product.czech_name'),
    field: 'czechName',
    sortable: true,
    align: 'left',
  },
  {
    name: 'retailPrice',
    label: t('product.retail_price'),
    field: 'retailPrice',
    align: 'left',
    sortable: false,
  },
]);
</script>
