<template>
  <q-card class="order-details-card">
    <!-- Hlavička karty -->
    <q-card-section class="header-section">
      <div>
        <div class="text-h6">{{ t('order.details') }}</div>
        <div class="text-subtitle2">Order #{{ order.customerName }}</div>
      </div>
      <div>
        <q-btn flat round size="md" :icon="mdiPlusBox" color="grey-color" @click="openAddContainerDialog" />
        <q-btn flat round size="md" :icon="mdiPencil" color="grey-color" @click.stop="openUpdateDialog" />
        <q-btn flat round size="md" :icon="mdiTrashCan" color="grey-color" @click.stop="openDeleteDialog" />
      </div>
    </q-card-section>

    <q-separator />

    <!-- Collapse detail sekcia s plynulou animáciou -->
    <q-slide-transition>
      <div v-show="isDetailsOpen">
        <q-card-section class="details-section">
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
            <span class="label">{{ t('order.discount') }}:</span>
            <span class="value">{{ order.discount }}</span>
          </div>
          <div class="order-detail">
            <span class="label">{{ t('order.note') }}:</span>
            <span class="value">{{ order.note }}</span>
          </div>
        </q-card-section>
      </div>
    </q-slide-transition>

    <!-- Footer karty s upraveným menším white pásom pre toggle tlačidlo -->
    <q-card-actions class="small-actions" align="center">
      <q-btn
        flat
        round
        size="sm"
        :icon="isDetailsOpen ? mdiChevronUp : mdiChevronDown"
        color="grey-color"
        @click="toggleDetails"
      />
    </q-card-actions>

    <!-- Dialógy -->
    <AddContainerDialog
      :orderId="order.id.toString()"
      v-model="isAddContainerDialogOpen"
      @onCreate="handleContainerCreated"
    />
    <UpdateOrderDialog v-model="isUpdateDialogOpen" :orderId="order.id" @onUpdate="handleOrderUpdated" />
    <DeleteOrderDialog v-model="isDeleteDialogOpen" :orderId="order.id.toString()" @onDeleted="handleOrderDeleted" />
  </q-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiPlusBox, mdiPencil, mdiTrashCan, mdiChevronUp, mdiChevronDown } from '@quasar/extras/mdi-v7';

import AddContainerDialog from '../order-item/AddContainerDialog.vue';
import UpdateOrderDialog from './UpdateOrderDialog.vue';
import DeleteOrderDialog from './DeleteOrderDialog.vue';

interface Order {
  id: number;
  customerName: string;
  orderDate: string;
  deliveryWeek: number;
  paymentMethod: string;
  contactPhone: string;
  discount: number;
  note: string;
}

defineProps<{ order: Order }>();
const { t } = useI18n();

const isAddContainerDialogOpen = ref(false);
const isUpdateDialogOpen = ref(false);
const isDeleteDialogOpen = ref(false);

// Riadi viditeľnosť detailov
const isDetailsOpen = ref(true);

// Prepína zobrazenie detailov
function toggleDetails() {
  isDetailsOpen.value = !isDetailsOpen.value;
}

// Formátovanie dátumu
function formatDate(dateString: string): string {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('en-GB', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

// Otváranie dialógov
function openAddContainerDialog() {
  isAddContainerDialogOpen.value = true;
}
function openUpdateDialog() {
  isUpdateDialogOpen.value = true;
}
function openDeleteDialog() {
  isDeleteDialogOpen.value = true;
}

// Callbacky dialógov
function handleContainerCreated() {}
function handleOrderUpdated() {}
function handleOrderDeleted() {}
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

/* Hlavička karty */
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

/* Detailná sekcia */
.details-section {
  padding: 8px 16px;
}

.order-detail {
  display: flex;
  justify-content: space-between;
  padding: 4px 0;
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

/* Footer – zmenšené padding pre menší biely pás */
.small-actions {
  padding: 4px !important; /* minimalny padding */
  background-color: transparent;
  margin: 0 !important;
}
</style>
