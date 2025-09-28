<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('order_item.add_item') }}</template>
    <template #default>
      <ContainerForm
        :containerData="containerData"
        :loading="addingContainer"
        @update:containerData="(val: AddOrderContainerRequest) => (containerData = val)"
        @on-submit="addContainerToOrder"
        @cancel="closeDialog"
      />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import ContainerForm from './ContainerForm.vue';
import OrderItemsService from '@/api/services/OrderItemsService';
import { useRoute } from 'vue-router'; // Import the route object to get orderId from the URL

interface AddOrderContainerRequest {
  orderId: string;
  name: string;
  quantity: number;
  pricePerContainer: number;
}

const props = defineProps<{
  orderId: string;
  modelValue: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'onCreate']);
const route = useRoute();
const { t } = useI18n();

const isDialogOpen = ref(props.modelValue);
watch(
  () => props.modelValue,
  (val) => {
    isDialogOpen.value = val;
  },
);
watch(isDialogOpen, (val) => emit('update:modelValue', val));

// Extract orderId from the URL
let orderId = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id;
onMounted(() => {
  orderId = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id || '';
});
// Inicializujeme údaje pre kontajner
const containerData = ref<AddOrderContainerRequest>({
  orderId: orderId,
  name: '',
  quantity: 1,
  pricePerContainer: 0,
});

const addingContainer = ref(false);

async function addContainerToOrder() {
  addingContainer.value = true;

  try {
    const { data, error } = await OrderItemsService.addOrderContainer(orderId, containerData.value);
    if (error) {
      handleError(error, t('order_item.toasts.add_failed'));
      return;
    }

    toast.success(t('order_item.toasts.add_success'));
    emit('onCreate', data);
    isDialogOpen.value = false;

    // Reset formu
    containerData.value = {
      orderId: orderId,
      name: '',
      quantity: 1,
      pricePerContainer: 0,
    };
  } catch (error) {
    handleError(error, t('order_item.toasts.add_failed'));
  } finally {
    addingContainer.value = false;
    isDialogOpen.value = false;
  }
}

function closeDialog() {
  isDialogOpen.value = false;
}
</script>
