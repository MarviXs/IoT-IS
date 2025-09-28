<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('order.update_order') }}</template>
    <template #default>
      <UpdateOrderForm
        v-if="orderData"
        :order="orderData"
        :loading="updatingOrder"
        @on-submit="updateOrder"
        @cancel="isDialogOpen = false"
      />
      <div v-else>{{ t('global.loading') }}...</div>
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import UpdateOrderForm from './UpdateOrderForm.vue'; // Nový formulár pre update
import OrdersService from '@/api/services/OrdersService'; // predpokladaná service
import { toast } from 'vue3-toastify';
import { handleError } from '@/utils/error-handler';
import { useRoute } from 'vue-router'; // Import the route object to get orderId from the URL

const props = defineProps<{ orderId: number; modelValue: boolean }>();
const emit = defineEmits(['update:modelValue', 'onUpdate']);
const { t } = useI18n();

const isDialogOpen = ref(props.modelValue);
watch(
  () => props.modelValue,
  (val) => (isDialogOpen.value = val),
);
watch(isDialogOpen, (val) => emit('update:modelValue', val));

const orderData = ref<any>(null);
const updatingOrder = ref(false);
const route = useRoute();
// Načítanie objednávky pri otvorení dialógu
watch(isDialogOpen, async (val) => {
  if (val) {
    await loadOrder();
  }
});

let orderId = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id;
onMounted(() => {
  orderId = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id || '';
});
async function loadOrder() {
  orderData.value = null;
  const { data, error } = await OrdersService.getOrder(orderId);
  if (error) {
    handleError(error, t('order.toasts.load_failed'));
    isDialogOpen.value = false;
    return;
  }
  orderData.value = data;
}

async function updateOrder(updatedData: any) {
  updatingOrder.value = true;
  const { data, error } = await OrdersService.updateOrder(orderId, updatedData);
  updatingOrder.value = false;

  if (error) {
    handleError(error, t('order.toasts.update_failed'));
    return;
  }

  toast.success(t('order.toasts.update_success'));
  emit('onUpdate', data);
  isDialogOpen.value = false;
}
</script>
