<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('order.label', 2), to: '/orders' },
      { label: order?.customerName || 'Order Details', to: `/orders/${orderId}` },
    ]"
  >
    <template #default>
      <OrderDetailsCard v-if="order" :order="order" @edit="isUpdateDialogOpen = true" />
      <div>
        <OrderItemTable
          v-model:pagination="pagination"
          :orders="ordersPaginated || []"
          :containers="ordersPaginated || []"
          :loading="isLoadingContainers"
          class="shadow"
          @on-change="getOrderItemContainers(pagination)"
        />
      </div>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import OrderItemTable from '@/components/order-item/OrderItemTable.vue';
import { useRoute } from 'vue-router';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import OrderDetailsCard from '@/components/orders/OrderDetailsCard.vue';
import { PaginationClient, PaginationTable } from '@/models/Pagination';

import OrderItemsService, { OrderItemContainersQueryParams } from '@/api/services/OrderItemsService.ts';
import OrderService from '@/api/services/OrdersService';
import AddItemToOrderDialog from '@/components/order-item/AddItemToOrderDialog.vue';
import { is } from 'quasar';

const { t } = useI18n();
const route = useRoute();
const isAddDialogOpen = ref(false);
const order = ref<Order | null>(null);
const isUpdateDialogOpen = ref(false);
const isLoadingContainers = ref(false);
const ordersPaginated = ref<any>([]);

const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});
const orderId = route.params.id;
interface Order {
  id: number;
  customerName: string;
  orderDate: string;
  deliveryWeek: number;
  paymentMethod: string;
  contactPhone: string;
  note: string ;
}

async function getOrderItemContainers(paginationTable: PaginationTable) {
  try {
    const paginationQuery: OrderItemContainersQueryParams = {
      SortBy: paginationTable.sortBy,
      Descending: paginationTable.descending,
      PageNumber: paginationTable.page,
      PageSize: paginationTable.rowsPerPage,
    };
    isLoadingContainers.value = true;
    const orderId = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id;

    const { data, error } = await OrderItemsService.getOrderItemContainers(orderId, paginationQuery);
    isLoadingContainers.value = false;

    if (error) {
      console.error('Error loading containers:', error);
      ordersPaginated.value = [];
      return;
    }
    ordersPaginated.value = data.items.map((item) => ({
      ...item,
      products: item.products || [], // Zabezpečíme, že `products` vždy existuje ako pole
    }));
  } catch (e) {
    console.error('Unexpected error:', e);
    isLoadingContainers.value = false;
    ordersPaginated.value = [];
  }
}

// Načítanie údajov o objednávke
// Načítanie údajov o objednávke
async function getOrder() {
  try {
    // Kontrola, či je orderId pole, a použitie správneho formátu
    const validOrderId = Array.isArray(orderId) ? orderId[0] : orderId.toString();

    // Načítanie údajov o objednávke
    const { data, error } = await OrderService.getOrder(validOrderId);
    if (error) {
      handleError(error, t('order.toasts.loading_failed'));
      return;
    }

    if (data) {
      console.log('Loaded order:', data);

      // Transformácia dát tak, aby boli kompatibilné s rozhraním Order
      order.value = {
        id: Number(data.id), // Konverzia `id` na číslo
        customerName: data.customerName,
        orderDate: data.orderDate,
        deliveryWeek: data.deliveryWeek,
        paymentMethod: data.paymentMethod,
        contactPhone: data.contactPhone,
        note: data.note ?? '', // Zabezpečíme, že `note` nebude null ani undefined
      };
    }
  } catch (e) {
    // Spracovanie neočakávaných chýb
    console.error('Unexpected error while loading order:', e);
    handleError(e, t('order.toasts.loading_failed'));
  }
}

// Funkcia na spracovanie požiadavky
function handleRequest() {
  // Implementácia spracovania požiadavky
  console.log('Request handled');
}

// Spustenie načítania položiek objednávky
getOrder();
getOrderItemContainers(pagination.value);

function handleError(error: any, message: string) {
  console.error(message, error);
}
</script>

<style scoped>
.order-details-page {
  padding: 20px;
}

h1 {
  font-size: 24px;
  margin-bottom: 20px;
}
</style>
