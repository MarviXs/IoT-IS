<template>
  <q-dialog v-model="isOpen" @hide="closeDialog">
    <q-card>
      <q-card-section>
        <div class="text-h6">{{ t('order_item.add_item') }}</div>
      </q-card-section>

      <q-card-section>
        <!-- Komponent OrderItemForm -->
        <OrderItemForm  
        v-model:orderItem="orderItem" 
        :loading="addingItem" 
        @on-submit="onSubmit" 
        @cancel="closeDialog" />
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { toast } from 'vue3-toastify';
import 'vue3-toastify/dist/index.css';
import { ref, defineProps, defineEmits, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router'; // Import the route object to get orderId from the URL
import OrderItemForm from '@/components/order-item/OrderItemForm.vue';
import OrderItemsService from '@/api/services/OrderItemsService';

const props = defineProps({
  containerId: { type: String, required: true }
});

const emit = defineEmits(['update:modelValue', 'onCreate']);
const { t } = useI18n();
const route = useRoute();

const isOpen = ref(false);
const addingItem = ref(false);

// Extract orderId from the URL
let orderId = route.params.id;
onMounted(() => {
  orderId = route.params.id || '';
});

// Objednávkový objekt pre formulár
const orderItem = ref({
  productId: '',
  quantity: 1
});

// Synchronizácia s modelValue
watch(
  () => props.modelValue,
  (newVal) => {
    isOpen.value = newVal;
  },
  { immediate: true }
);

watch(isOpen, (val) => {
  emit('update:modelValue', val);
});

// Funkcia na zatvorenie dialógu
function closeDialog() {
  isOpen.value = false; // Zavrie dialóg
  emit('update:modelValue', false); // Synchronizácia s rodičovským modelom
}

async function onSubmit() {
  if (!orderItem.value.productId || !orderItem.value.quantity) {
    toast.error('Product and quantity are required');
    return;
  }

  console.log('OrderId:', orderId);
  console.log('ContainerId:', props.containerId);
  console.log('ProductId:', orderItem.value.productId);
  console.log('Quantity:', orderItem.value.quantity);

  try {
    await OrderItemsService.addItemToContainer(
      orderId,
      props.containerId,
      {
        productId: orderItem.value.productId,
        quantity: Number(orderItem.value.quantity),
      }
    );

    toast.success('Item added successfully');
    emit('onCreate');
  } catch (error) {
    console.error('Failed to add item:', error);
    toast.error('Failed to add item');
  }
}
</script>

<style scoped>
.q-card {
  width: 500px;
  max-width: 90vw;
}
</style>
