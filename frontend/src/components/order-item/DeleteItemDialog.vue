<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('order_item.delete_item') }}</template>
    <template #description>{{ t('order_item.delete_item_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '../core/DeleteConfirmationDialog.vue';
import OrderItemsService from '@/api/services/OrderItemsService';

// Definujeme, či je dialóg otvorený
const isDialogOpen = defineModel<boolean>();

// Prijímame `orderId` a `itemId` ako props
const props = defineProps({
  orderId: {
    type: Number,
    required: true,
  },
  itemId: {
    type: Number,
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

// Lokalizácia
const { t } = useI18n();

const isDeleteInProgress = ref(false);

// Funkcia na vymazanie položky
async function handleDelete() {
  isDeleteInProgress.value = true;

  // Zavoláme metódu deleteItemFromOrder s `orderId` a `itemId`
  const { error } = await OrderItemsService.deleteItemFromOrder(props.orderId, props.itemId);
  isDeleteInProgress.value = false;

  if (error) {
    handleError(error, 'Error deleting item');
    return;
  }

  // Ak sa vymazanie podarilo, zobrazíme toast a zavrieme dialóg
  toast.success(t('order_item.toasts.delete_success'));
  emit('onDeleted');
  isDialogOpen.value = false;
}
</script>
