<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('order.add_order') }}</template>
    <template #default>
      <OrderForm 
      ref="orderForm" 
      v-model="order" 
      @on-submit="createOrder" 
      :loading="creatingOrder" 
      />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import type { CreateOrderRequest } from '@/api/services/OrdersService';
import OrderForm from '@/components/orders/OrderForm.vue';
// Import for OrderFormData type to correctly type the `order` ref


import OrderService from '@/api/services/OrdersService';
import type { OrderFormData } from './OrderForm.vue';
import { isToday } from 'date-fns';

// Define reactive state for dialog visibility
const isDialogOpen = ref<boolean>(false);
const emit = defineEmits(['onCreate']);

const { t } = useI18n();

// Reactive state to track loading status during order creation
const creatingOrder = ref(false);

// Initialize the order form data with default values
const order = ref<OrderFormData>({
    customerId: '', // Changed to customerId to match backend requirements
    contactPhone: '',
    paymentMethod: '',
    note: '', 
    deliveryWeek: 0,
    orderDate: isToday(new Date()) ? new Date().toISOString() : ''
});

const orderForm = ref();

// Function to create an order based on the form data
async function createOrder() {
  // Create the request object using the form data
  const paymentMethodValue = typeof order.value.paymentMethod === 'object' ? order.value.paymentMethod.value : order.value.paymentMethod;

  const createRequest: CreateOrderRequest = {
      customerId: order.value.customerId, // Updated property to customerId
      contactPhone: order.value.contactPhone,
      note: order.value.note,
      paymentMethod: paymentMethodValue,
      deliveryWeek: order.value.deliveryWeek,
      orderDate: new Date(order.value.orderDate).toISOString(),
  };

  // Set loading state to true during API call
  creatingOrder.value = true;
  const { data, error } = await OrderService.createOrder(createRequest);
  // Set loading state back to false after API call
  creatingOrder.value = false;

  // Handle error if the API call fails
  if (error) {
    handleError(error, t('order.toasts.create_failed'));
    return;
  }

  // Emit success event and close dialog
  emit('onCreate', data);
  isDialogOpen.value = false;

  // Show success toast message
  toast.success(t('order.toasts.create_success'));
}
</script>

<style lang="scss" scoped></style>
