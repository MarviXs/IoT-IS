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
    <template #body-cell-actions="propsActions">
      <q-td auto-width :props="propsActions">
        <q-btn :icon="mdiPencil" color="grey-color" flat round @click.stop="openUpdateDialog(propsActions.row.id)"
          ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.edit') }}
          </q-tooltip>
        </q-btn>
        <q-btn :icon="mdiTrashCan" color="grey-color" flat round @click.stop="openDeleteDialog(propsActions.row.id)"
          ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.delete') }}
          </q-tooltip>
        </q-btn>
      </q-td>
    </template>
  </q-table>
  <EditProductDialog
    v-if="productToUpdate"
    v-model="isUpdateDialogOpen"
    :product-id="productToUpdate"
    @on-update="emit('onChange')"
  />
  <DeleteProductDialog
    v-if="productToDelete"
    v-model="isDeleteDialogOpen"
    :product-id="productToDelete"
    @on-deleted="emit('onChange')"
  />
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { PropType, computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiFileSearchOutline, mdiPencil, mdiTrashCan } from '@quasar/extras/mdi-v7';
import { PaginationClient } from '@/models/Pagination';
import { ProductsResponse } from '@/api/services/ProductService';
import EditProductDialog from './EditProductDialog.vue';
import DeleteProductDialog from './DeleteProductDialog.vue';

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
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);

const isUpdateDialogOpen = ref(false);
const productToUpdate = ref<string>();
function openUpdateDialog(productId: string) {
  productToUpdate.value = productId;
  isUpdateDialogOpen.value = true;
}

const isDeleteDialogOpen = ref(false);
const productToDelete = ref<string>();
function openDeleteDialog(productId: string) {
  isDeleteDialogOpen.value = true;
  productToDelete.value = productId;
}
</script>
