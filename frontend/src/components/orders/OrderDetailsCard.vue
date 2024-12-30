<template>
  <q-card class="order-details-card">
    <q-card-section class="header-section">
      <div>
        <div class="text-h6">{{ t('order.details') }}</div>
        <div class="text-subtitle2">Order #{{ order.id }}</div>
      </div>
      <div>
        <q-btn flat round size="md" :icon="mdiPlusBox" color="grey-color" @click="openAddContainerDialog"></q-btn>
        <q-btn flat round size="md" :icon="mdiPencil" color="grey-color"
          @click.stop="openUpdateDialog(order.id)"></q-btn>
        <q-btn flat round size="md" :icon="mdiTrashCan" color="grey-color"
          @click.stop="openDeleteDialog(order.id)"></q-btn>
      </div>
    </q-card-section>

    <q-separator />

    <q-card-section>
      <div class="order-detail">
        <span class="label">{{ t('order.customer') }}:</span>
        <span class="value">{{ order.customerName }}</span>
      </div>
      <div class="order-detail">
        <span class="label">{{ t('order.order_date') }}:</span>
        <span class="value">{{ formatDate(order.orderDate) }}</span>
      </div>
      <div class="order-detail">
        <span class="label">{{ t('order.delivery_week') }}:</span>
        <span class="value">{{ order.deliveryWeek }}</span>
      </div>
      <div class="order-detail">
        <span class="label">{{ t('order.payment_method.label') }}:</span>
        <span class="value">{{ order.paymentMethod }}</span>
      </div>
      <div class="order-detail">
        <span class="label">{{ t('order.contact_phone') }}:</span>
        <span class="value">{{ order.contactPhone }}</span>
      </div>
      <div class="order-detail">
        <span class="label">{{ t('order.note') }}:</span>
        <span class="value">{{ order.note }}</span>
      </div>
    </q-card-section>

    <AddContainerDialog :orderId="order.id" v-model="isAddContainerDialogOpen" @onCreate="handleContainerCreated" />

    <!-- Update dialog -->
    <UpdateOrderDialog v-model="isUpdateDialogOpen" :orderId="order.id" @onUpdate="handleOrderUpdated" />

    <!-- Delete dialog -->
    <DeleteOrderDialog v-model="isDeleteDialogOpen" :orderId="order.id" @onDeleted="handleOrderDeleted" />
    

  </q-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { defineProps, defineEmits } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiPlusBox, mdiPencil, mdiTrashCan } from '@quasar/extras/mdi-v7';
import AddContainerDialog from '../order-item/AddContainerDialog.vue';
import UpdateOrderDialog from './UpdateOrderDialog.vue';
import DeleteOrderDialog from './DeleteOrderDialog.vue'; // Import nového komponentu

interface Order {
  id: number;
  customerName: string;
  orderDate: string;
  deliveryWeek: number;
  paymentMethod: string;
  contactPhone: string;
  note: string;
}

const props = defineProps<{
  order: Order;
}>();

const emit = defineEmits(['edit', 'delete']);
const { t } = useI18n();

const isAddContainerDialogOpen = ref(false);
const isUpdateDialogOpen = ref(false);
const isDeleteDialogOpen = ref(false);

function formatDate(dateString: string): string {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('en-GB', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

function openUpdateDialog(orderId: number): void {
  isUpdateDialogOpen.value = true;
}

function openAddContainerDialog() {
  isAddContainerDialogOpen.value = true;
}

function openDeleteDialog(orderId: number): void {
  isDeleteDialogOpen.value = true;
}

function handleContainerCreated(newContainerId: number) {
}

function handleOrderUpdated(updatedOrder: any) {
}

function handleOrderDeleted() {
}

</script>

<style lang="scss" scoped>
.order-details-card {
  width: 100%;
  margin: 20px auto;
  border: 1px solid #ddd;
  border-radius: 8px;
  background-color: #ffffff;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
}

.header-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background-color: #f5f5f5;
  border-bottom: 1px solid #ddd;
}

.text-h6 {
  font-size: 1.2em;
  font-weight: bold;
  color: #333;
}

.text-subtitle2 {
  font-size: 0.9em;
  color: #777;
}

.order-detail {
  display: flex;
  justify-content: space-between;
  padding: 8px 16px;
  border-bottom: 1px solid #eee;
}

.order-detail:last-child {
  border-bottom: none;
}

.label {
  font-weight: 600;
  color: #444;
}

.value {
  font-weight: 400;
  color: #666;
}
</style>
