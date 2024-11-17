<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('order.label', 2), to: '/orders' },
      { label: 'Order #' + orderId, to: `/orders/${orderId}` },
    ]"
  >
  <template #default>
      <OrderDetailsCard 
        v-if="order" 
        :order="order" 
        @edit="isUpdateDialogOpen = true"
        />
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
import {  ref } from 'vue';
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
const orderId = Number(route.params.id);
interface Order {
  id: number;
  customerName: string;
  orderDate: string;
  deliveryWeek: number;
  paymentMethod: string;
  contactPhone: string;
  note: string;
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
    const { data, error } = await OrderItemsService.getOrderItemContainers(orderId, paginationQuery);
    isLoadingContainers.value = false;

    if (error) {
      console.error('Error loading containers:', error);
      ordersPaginated.value = [];
      return;
    }
    ordersPaginated.value = data.items.map(item => ({
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
async function getOrder() {
  const { data, error } = await OrderService.getOrder(orderId.toString());
  if (error) {
    handleError(error, t('order.toasts.loading_failed'));
    return;
  }
  console.log('Loaded order:', data);
  order.value = data;
  
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
