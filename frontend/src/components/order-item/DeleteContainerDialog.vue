<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('container.delete_container') }}</template>
    <template #description>{{ t('container.delete_container_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import { handleError } from '@/utils/error-handler';
import OrderItemsService from '@/api/services/OrderItemsService';
import DeleteConfirmationDialog from '@/components/core/DeleteConfirmationDialog.vue';


const props = defineProps<{
  orderId: string; // ID objednávky
  containerId: string; // ID kontajnera na odstránenie
  modelValue: boolean; // Riadenie otvorenia dialógu
}>();

const emit = defineEmits(['update:modelValue', 'onDeleted']); // Emituje udalosti pre nadradený komponent
const isDialogOpen = ref(props.modelValue);
const isDeleteInProgress = ref(false);

const { t } = useI18n();

watch(
  () => props.modelValue,
  (val) => {
    isDialogOpen.value = val;
  }
);

watch(
  () => isDialogOpen.value,
  (val) => {
    emit('update:modelValue', val);
  }
);

async function handleDelete() {
  isDeleteInProgress.value = true;

  try {
    await OrderItemsService.deleteContainerFromOrder(props.orderId, props.containerId);
    toast.success(t('container.toasts.delete_success'));
    emit('onDeleted'); // Informuje rodičovský komponent o úspešnom odstránení
    isDialogOpen.value = false;
  } catch (error: any) {
    const parsedError = typeof error === 'object' && error !== null ? error : { detail: 'Unknown error occurred' };
    handleError(parsedError, 'Error deleting container');
  } finally {
    isDeleteInProgress.value = false;
  }
}

</script>

<style scoped>
/* Špecifické štýly pre tento komponent */
</style>
