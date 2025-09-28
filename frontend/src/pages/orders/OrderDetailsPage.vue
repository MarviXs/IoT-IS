<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('order.label', 2), to: '/orders' },
      { label: order?.customerName || 'Order Details', to: `/orders/${orderId}` },
    ]"
  >
    <template #actions>
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('order.download_quotation_sheet')"
        :icon="mdiDownload"
        @click="downloadQuotationSheet"
      />
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('order.download_plants_passports')"
        :icon="mdiDownload"
        @click="downloadPlantsPassports"
      />
    </template>
    <template #default>
      <OrderDetailsCard v-if="order" :order="order" @edit="isUpdateDialogOpen = true" />
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
        @refresh-summary="handleRefreshSummary"
      />
      <!-- Odovzdávame aj refreshKey pre OrderSummaryCard -->
      <OrderSummaryCard :summary="orderSummary" :orderId="orderId" :refreshKey="summaryRefreshKey" />
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
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import OrderDetailsCard from '@/components/orders/OrderDetailsCard.vue';
import OrderItemTable from '@/components/order-item/OrderItemTable.vue';
import OrderSummaryCard from '@/components/orders/OrderSummaryCard.vue';
import DeleteContainerDialog from '@/components/order-item/DeleteContainerDialog.vue';
import PageLayout from '@/layouts/PageLayout.vue';
import type { OrderItemContainersQueryParams } from '@/api/services/OrderItemsService';
import OrderItemsService from '@/api/services/OrderItemsService';
import OrderService from '@/api/services/OrdersService';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import { mdiDownload } from '@quasar/extras/mdi-v7';

const { t } = useI18n();
const route = useRoute();
const order = ref<Order | null>(null);
const isUpdateDialogOpen = ref(false);
const isLoadingContainers = ref(false);
const ordersPaginated = ref<any>([]);
const isDeleteDialogOpen = ref(false);
const selectedContainerId = ref<string | null>(null);

// Reaktívny kľúč pre refresh súhrnu (OrderSummaryCard)
const summaryRefreshKey = ref(0);

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
  note: string;
}

// Typ pre súhrn objednávky
interface OrderSummary {
  vat12: {
    priceExclVAT: number;
    vat: number;
    priceInclVAT: number;
  };
  vat21: {
    priceExclVAT: number;
    vat: number;
    priceInclVAT: number;
  };
  total: number;
}

// Predvolený súhrn (nahradzujte podľa potreby alebo načítajte z API)
const orderSummary = ref<OrderSummary>({
  vat12: { priceExclVAT: 100, vat: 12, priceInclVAT: 112 },
  vat21: { priceExclVAT: 200, vat: 42, priceInclVAT: 242 },
  total: 354,
});

let orderId = String(Array.isArray(route.params.id) ? route.params.id[0] : route.params.id);

onMounted(() => {
  orderId = String(Array.isArray(route.params.id) ? route.params.id[0] : route.params.id);
  refreshTable(); // Po úvodnom mount načítaj dáta
});

function downloadQuotationSheet() {
  OrderService.downloadOrderTemplate(orderId).then((retVal) => {
    const contentDisposition = retVal.response.headers.get('Content-Disposition');
    const parts = contentDisposition?.split('filename=');
    const filename = parts?.length === 2 ? parts[1] : 'order_template.xlsx';

    retVal.response.blob().then((blob) => {
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', filename);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    });
  });
}

function downloadPlantsPassports() {
  OrderService.downloadPlantsPassports(orderId).then((retVal) => {
    const contentDisposition = retVal.response.headers.get('Content-Disposition');
    const parts = contentDisposition?.split('filename=');
    const filename = parts?.length === 2 ? parts[1] : 'plants_passports.xlsx';

    retVal.response.blob().then((blob) => {
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', filename);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    });
  });
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

    ordersPaginated.value = data.items.map((item) => ({
      ...item,
      products: item.products || [], // zabezpečí, že `products` je vždy pole
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
  getOrder(); // Načítaj detaily objednávky
  getOrderItemContainers(pagination.value); // Načítaj kontajnery
}

function openDeleteContainerDialog(containerId: string) {
  selectedContainerId.value = containerId;
  isDeleteDialogOpen.value = true;
}

// Táto metóda sa volá z OrderItemTable, aby osviežila OrderSummaryCard
function handleRefreshSummary() {
  summaryRefreshKey.value++;
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
