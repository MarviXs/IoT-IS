<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('order.label', 2), to: '/orders' },
      { label: 'Order #' + orderId, to: `/orders/${orderId}` },
    ]"
  >
    <template #actions>
        <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('order_item.add_item')"
        :icon="mdiPlus"
        @click="isAddDialogOpen = true"
      />
    </template>
  
    <template #default>
      <OrderDetailsCard 
        v-if="order" 
        :order="order" 
        @edit="isUpdateDialogOpen = true" />
    <div>
      <OrderItemTable 
        :orderItems="orderItems" 
        :loading="loading" 
        @onRequest="handleRequest" />
    </div>
  </template>

</PageLayout>

  <AddItemToOrderDialog v-model="isAddDialogOpen" :orderId="orderId.toString()" @onCreate="getOrderItems" />
  <EditOrderDialog v-model="isUpdateDialogOpen" :order-id="orderId" @on-update="getOrder" />
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import {  ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import OrderDetailsCard from '@/components/orders/OrderDetailsCard.vue';
import OrderItemTable from '@/components/order-item/OrderItemTable.vue';
import OrderItemsService from '@/api/services/OrderItemsService.ts';
import OrderService from '@/api/services/OrdersService';
import AddItemToOrderDialog from '@/components/order-item/AddItemToOrderDialog.vue';
interface OrderItem {
  id: number;
  orderId: number;
  productNumber: string;
  varietyName: string;
  quantity: number;
}

interface PaginatedOrderItems {
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  readonly hasPrevious: boolean;
  readonly hasNext: boolean;
  items: Array<OrderItem>;
}

const orderItems = ref<PaginatedOrderItems>({
  currentPage: 0,
  totalPages: 0,
  pageSize: 0,
  totalCount: 0,
  hasPrevious: false,
  hasNext: false,
  items: []
});
const loading = ref(true);
import { handleError } from '@/utils/error-handler';

const { t } = useI18n();
const route = useRoute();
const isAddDialogOpen = ref(false);
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

const order = ref<Order | null>(null);
const isUpdateDialogOpen = ref(false);

// Načítanie údajov o objednávke
async function getOrder() {
  const { data, error } = await OrderService.getOrder(orderId.toString());
  if (error) {
    handleError(error, t('order.toasts.loading_failed'));
    return;
  }
  order.value = data;
  
}
async function getOrderItems() {
  loading.value = true;
  const queryParams = {
    SortBy: 'VarietyName',
    Descending: false,
    PageNumber: 1,
    PageSize: 10,
  };

  const { data, error } = await OrderItemsService.getOrderItems(orderId, queryParams);
  loading.value = false;

  if (error) {
    console.error('Error fetching order items:', error);
    return;
  }

  orderItems.value = data;
}

// Funkcia na spracovanie požiadavky
function handleRequest() {
  // Implementácia spracovania požiadavky
  console.log('Request handled');
}

// Spustenie načítania položiek objednávky
getOrderItems();
getOrder();
</script>

<style lang="scss" scoped>
/* Môžete pridať vlastné štýly pre stránku objednávok */
</style>
