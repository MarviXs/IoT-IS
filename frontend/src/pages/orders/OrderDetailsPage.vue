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
          :currentOrderId="orderId"
          :orders="ordersPaginated || []"
          :containers="ordersPaginated || []"
          :loading="isLoadingContainers"
          :refreshTable="refreshTable"
          class="shadow"
          @on-change="getOrderItemContainers(pagination)"
          @open-delete-dialog="openDeleteContainerDialog"
        />
      </div>
    </template>
  </PageLayout>
  <!-- Delete Container Dialog -->
  <DeleteContainerDialog
    v-model="isDeleteDialogOpen"
    :orderId="Array.isArray(orderId) ? orderId[0] : orderId"
    :containerId="selectedContainerId || ''"
    @onDeleted="refreshTable"
  />
</template>

<script setup lang="ts">
import OrderItemTable from '@/components/order-item/OrderItemTable.vue';
import { useRoute } from 'vue-router';
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import OrderDetailsCard from '@/components/orders/OrderDetailsCard.vue';
import { PaginationClient, PaginationTable } from '@/models/Pagination';

import OrderItemsService, { OrderItemContainersQueryParams } from '@/api/services/OrderItemsService.ts';
import OrderService from '@/api/services/OrdersService';
import AddItemToOrderDialog from '@/components/order-item/AddItemToOrderDialog.vue';
import DeleteContainerDialog from '@/components/order-item/DeleteContainerDialog.vue';

const { t } = useI18n();
const route = useRoute();
const isAddDialogOpen = ref(false);
const order = ref<Order | null>(null);
const isUpdateDialogOpen = ref(false);
const isLoadingContainers = ref(false);
const ordersPaginated = ref<any>([]);
const isDeleteDialogOpen = ref(false); // Stav otvorenia dialógu
const selectedContainerId = ref<string | null>(null); // Aktuálne vybraný kontajner

const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});

interface Order {
  id: number;
  customerName: string;
  orderDate: string;
  deliveryWeek: number;
  paymentMethod: string;
  contactPhone: string;
  note: string ;
}
var orderId = String(Array.isArray(route.params.id) ? route.params.id[0] : route.params.id);

onMounted(() => {
  orderId = String(Array.isArray(route.params.id) ? route.params.id[0] : route.params.id);
  refreshTable(); // Po úvodnom mount načítaj dáta
});

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

async function getOrder() {
  try {
    const { data, error } = await OrderService.getOrder(orderId);
    if (error) {
      console.error('Error loading order:', error);
      return;
    }
    if (data) {
      order.value = {
        id: Number(data.id),
        customerName: data.customerName,
        orderDate: data.orderDate,
        deliveryWeek: data.deliveryWeek,
        paymentMethod: data.paymentMethod,
        contactPhone: data.contactPhone,
        note: data.note ?? '',
      };
    }
  } catch (e) {
    console.error('Unexpected error while loading order:', e);
  }
}

function refreshTable() {
  getOrder(); // Znovu načíta detaily objednávky
  getOrderItemContainers(pagination.value); // Znovu načíta kontajnery
}

function openDeleteContainerDialog(containerId: string) {
  selectedContainerId.value = containerId;
  isDeleteDialogOpen.value = true;
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
