<template>
  <!-- Obal pre horizontálne posúvanie -->
  <div class="scroll-container">
    <q-table
      v-model:pagination="pagination"
      :rows="orderItemsFiltered"
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
          <q-btn :icon="mdiPencil" color="grey-color" flat round @click.stop="openUpdateDialog(propsActions.row.id)">
            <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.edit') }}
            </q-tooltip>
          </q-btn>
          <q-btn :icon="mdiTrashCan" color="grey-color" flat round @click.stop="openDeleteDialog(propsActions.row.id)">
            <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.delete') }}
            </q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </q-table>
  </div>
  
  <!-- Dialogy pre úpravu a vymazanie položky -->
  <EditOrderItemDialog
    v-if="itemToUpdate"
    v-model="isUpdateDialogOpen"
    :item-id="itemToUpdate"
    @on-update="emit('onChange')"
  />
  <DeleteOrderItemDialog
    v-if="itemToDelete"
    v-model="isDeleteDialogOpen"
    :item-id="itemToDelete"
    @on-deleted="emit('onChange')"
  />
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { PropType, computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiFileSearchOutline, mdiPencil, mdiTrashCan } from '@quasar/extras/mdi-v7';
import { PaginationClient } from '@/models/Pagination';
import { OrderItemsResponse } from '@/api/services/OrderItemsService';
//import EditOrderItemDialog from './EditOrderItemDialog.vue';
//import DeleteOrderItemDialog from './DeleteOrderItemDialog.vue';

const props = defineProps({
  orderItems: {
    type: Object as PropType<OrderItemsResponse>, // Predpokladáme, že máte OrderItemsResponse definovaný vo vašich službách
    required: false,
    default: null,
  },
  loading: {
    type: Boolean,
    required: true,
  },
});

const pagination = defineModel<PaginationClient>('pagination');

const orderItemsFiltered = computed(() => props.orderItems?.items ?? []);

const emit = defineEmits(['onChange', 'onRequest']);
const { t } = useI18n();

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'id',
    label: t('order_item.id'),
    field: 'id',
    align: 'left',
    sortable: true,
  },
  {
    name: 'productNumber',
    label: t('order_item.product_number'),
    field: 'productNumber',
    align: 'left',
    sortable: true,
  },
  {
    name: 'varietyName',
    label: t('order_item.variety_name'),
    field: 'varietyName',
    align: 'left',
    sortable: true,
  },
  {
    name: 'quantity',
    label: t('order_item.quantity'),
    field: 'quantity',
    align: 'right',
    sortable: true,
  },
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);

// Premenné pre dialógy na úpravu a vymazanie položky
const isUpdateDialogOpen = ref(false);
const itemToUpdate = ref<string>();
function openUpdateDialog(itemId: string) {
  itemToUpdate.value = itemId;
  isUpdateDialogOpen.value = true;
}

const isDeleteDialogOpen = ref(false);
const itemToDelete = ref<string>();
function openDeleteDialog(itemId: string) {
  itemToDelete.value = itemId;
  isDeleteDialogOpen.value = true;
}
</script>

<style scoped>
.scroll-container {
  overflow-x: auto;
  white-space: nowrap;
  width: 100%;
}

.q-table__container {
  width: 100%;
  display: inline-block;
}
</style>
