<template>
  <q-card class="order-details-card">
    <q-card-section>
      <div class="text-h6">{{ t('order.details') }}</div>
      <div class="text-subtitle2">Order #{{ order.id }}</div>
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
        <span class="label">{{ t('order.payment_method') }}:</span>
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

    <q-card-actions align="right">
      <q-btn flat color="primary" @click="$emit('edit')" label="Edit" />
    </q-card-actions>
  </q-card>
</template>

<script setup lang="ts">
import { defineProps, defineEmits } from 'vue';
import { useI18n } from 'vue-i18n';

// Definujeme typ pre objekt objednávky
interface Order {
  id: number;
  customerName: string;
  orderDate: string;
  deliveryWeek: number;
  paymentMethod: string;
  contactPhone: string;
  note: string;
}

// Definujeme vlastnosti (props) komponentu
const props = defineProps<{
  order: Order;
}>();

// Definujeme emisie udalostí komponentu
const emit = defineEmits(['edit']);

const { t } = useI18n();

// Funkcia na formátovanie dátumu
function formatDate(dateString: string): string {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('en-GB', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}
</script>

<style lang="scss" scoped>
.order-details-card {
  width: 100%;
  margin: 20px auto;
  background-color: #ffffff;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.order-detail {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
}

.order-detail .label {
  font-weight: 600;
  color: #333;
}

.order-detail .value {
  color: #555;
}
</style>
