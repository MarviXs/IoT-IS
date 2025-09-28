<template>
  <!-- Obal pre horizontálne posúvanie -->
  <q-table
    v-model:pagination="pagination"
    class="tw-overflow-x-scroll tw-w-full"
    :rows="ordersFiltered"
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
    <template #body-cell-customer="props">
      <q-td :props="props">
        <q-btn flat @click="goToCustomerPage(props.row.id)" color="primary" class="text-decoration-none">
          {{ props.row.customerName }}
        </q-btn>
      </q-td>
    </template>

    <template #no-data="{ message }">
      <div class="full-width column flex-center q-pa-lg nothing-found-text">
        <q-icon :name="mdiFileSearchOutline" class="q-mb-md" size="50px"></q-icon>
        {{ message }}
      </div>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import type { QTableProps} from 'quasar';
import { date } from 'quasar';
import type { PropType} from 'vue';
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiFileSearchOutline } from '@quasar/extras/mdi-v7';
import type { PaginationClient } from '@/models/Pagination';
import type { OrdersResponse } from '@/api/services/OrdersService';
import { useRouter } from 'vue-router';

const props = defineProps({
  orders: {
    type: Object as PropType<OrdersResponse>,
    required: false,
    default: null,
  },
  loading: {
    type: Boolean,
    required: true,
  },
});

const pagination = defineModel<PaginationClient>('pagination');

const ordersFiltered = computed(() => props.orders?.items ?? []);

const emit = defineEmits(['onChange', 'onRequest']);

const { t } = useI18n();
const router = useRouter();

// Funkcia na formátovanie dátumu a času pomocou Quasar date utility
function formatDateTime(dateString: string | Date): string {
  if (!dateString) {
    return '';
  }
  return date.formatDate(dateString, 'YYYY-MM-DD HH:mm:ss');
}

// Funkcia na navigáciu na stránku s podrobnosťami o zákazníkovi
function goToCustomerPage(orderId: number) {
  router.push({ name: 'OrderDetails', params: { id: orderId } });
}

const columns = computed<QTableProps['columns']>(() => [
  /*
  {
    name: 'id',
    label: t('order.id'),
    field: 'id',
    align: 'left',
    sortable: true,
  },
  */
  {
    name: 'customer',
    label: t('order.customer'),
    field: 'customerName',
    align: 'left',
    sortable: true,
  },
  {
    name: 'orderDate',
    label: t('order.order_date'),
    field: 'orderDate',
    align: 'left',
    sortable: true,
    format: (val) => formatDateTime(val),
  },
  {
    name: 'deliveryWeek',
    label: t('order.delivery_week'),
    field: 'deliveryWeek',
    align: 'left',
    sortable: true,
  },
  {
    name: 'paymentMethod',
    label: t('order.payment_method.label'),
    field: 'paymentMethod',
    align: 'left',
    sortable: true,
  },
  {
    name: 'contactPhone',
    label: t('order.contact_phone'),
    field: 'contactPhone',
    align: 'left',
    sortable: true,
  },
  {
    name: 'note',
    label: t('order.note'),
    field: 'note',
    align: 'left',
    sortable: false,
  },
]);
</script>
