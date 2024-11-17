<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('order_item.add_item') }}</template>
    <template #default>
      <OrderItemForm
        v-model:orderItem="orderItem"
        :orderId="Number(props.orderId)" 
        @on-submit="addContainerToOrder"
        :loading="addingContainer"
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
import type { AddOrderContainerRequest } from '@/api/services/OrderItemsService';

// Props pre prijatie orderId
const props = defineProps<{ orderId: string }>();

// State pre sledovanie viditeľnosti dialogu a načítanie stavu
const isDialogOpen = ref<boolean>(false);
const addingContainer = ref(false);

// Emit eventu, ktorý použijeme na aktualizáciu hlavného zoznamu po pridaní položky
const emit = defineEmits(['onCreate']);

// Lokalizácia
const { t } = useI18n();

// Reactive state pre formulár položky objednávky s predvolenými hodnotami
// Inicializujeme `orderItem` mimo `defineModel`
const orderItem = ref<AddOrderContainerRequest>({
  orderId: Number(props.orderId),
  name: '',
  quantity: 1,
  pricePerContainer: 0,
});


// Funkcia pre pridanie položky do objednávky
async function addContainerToOrder() {
  
  const addContainerRequest: AddOrderContainerRequest = {
    orderId: orderItem.value.orderId,
    name: orderItem.value.name,
    quantity: orderItem.value.quantity,
    pricePerContainer: orderItem.value.pricePerContainer,
  };

  // Nastavenie stavu načítania na true počas API volania
  addingContainer.value = true;

  try {
    // Zavoláme službu na pridanie položky do objednávky
    const { data, error } = await OrderItemsService.addOrderContainer(addContainerRequest.orderId, addContainerRequest);

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
    addingContainer.value = false;
  }
}

</script>

<style lang="scss" scoped></style>
