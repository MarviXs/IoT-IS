<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('product.delete_product') }}</template>
    <template #description>{{ t('product.delete_product_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '../core/DeleteConfirmationDialog.vue';
import ProductService from '@/api/services/ProductService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  productId: {
    type: String,
    default: '',
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();

const isDeleteInProgress = ref(false);

async function handleDelete() {
  isDeleteInProgress.value = true;
  const { error } = await ProductService.deleteProduct(props.productId);
  isDeleteInProgress.value = false;

  if (error) {
    handleError(error, 'Error deleting product');
    return;
  }

  toast.success(t('product.toasts.delete_success'));
  emit('onDeleted');
  isDialogOpen.value = false;
}
</script>
