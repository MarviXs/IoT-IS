<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('order_item.add_item') }}</template>
    <template #default>
      <OrderItemForm
        v-model:orderItem="orderItem"
        :orderId="Number(props.orderId)" 
        @on-submit="addItemToOrder"
        :loading="addingItem"
      />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import OrderItemForm from '@/components/order-item/OrderItemForm.vue';
import OrderItemsService from '@/api/services/OrderItemsService';
import type { AddOrderItemRequest } from '@/api/services/OrderItemsService';

// Props pre prijatie orderId
const props = defineProps<{ orderId: string }>();

// State pre sledovanie viditeľnosti dialogu a načítanie stavu
const isDialogOpen = ref<boolean>(false);
const addingItem = ref(false);

// Emit eventu, ktorý použijeme na aktualizáciu hlavného zoznamu po pridaní položky
const emit = defineEmits(['onCreate']);

// Lokalizácia
const { t } = useI18n();

// Reactive state pre formulár položky objednávky s predvolenými hodnotami
// Inicializujeme `orderItem` mimo `defineModel`
const orderItem = ref<AddOrderItemRequest>({
  orderId: Number(props.orderId),
  productNumber: '',
  varietyName: '',
  quantity: 1,
});


// Funkcia pre pridanie položky do objednávky
async function addItemToOrder() {
  const productNumber = typeof orderItem.value.productNumber === 'object' && orderItem.value.productNumber !== null
  ? (orderItem.value.productNumber as { id: string }).id
    : orderItem.value.productNumber;
  
  const addItemRequest: AddOrderItemRequest = {
    orderId: Number(props.orderId),
    productNumber: productNumber,  // Nastavíme iba ID
    varietyName: orderItem.value.varietyName,
    quantity: orderItem.value.quantity,
  };

  // Nastavenie stavu načítania na true počas API volania
  addingItem.value = true;

  try {
    // Zavoláme službu na pridanie položky do objednávky
    const { data, error } = await OrderItemsService.addItemToOrder(addItemRequest.orderId, addItemRequest);

    if (error) {
      handleError(error, t('order_item.toasts.add_failed'));
      return;
    }

    // Emit event na aktualizáciu zoznamu položiek a zavrieme dialog
    emit('onCreate', data);
    isDialogOpen.value = false;

    // Zobrazíme toast s úspešnou správou
    toast.success(t('order_item.toasts.add_success'));
  } catch (error) {
    handleError(
      error as { type?: string; title?: string; status?: number; detail?: string; instance?: string; errors: Record<string, any> },
      t('order_item.toasts.add_failed')
    );
  } finally {
    // Vypneme stav načítania po ukončení API volania
    addingItem.value = false;
  }
}

</script>

<style lang="scss" scoped></style>
