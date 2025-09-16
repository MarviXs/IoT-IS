<template>
    <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
        <template #title>{{ t('order.delete_order') }}</template>
        <template #description>{{ t('order.delete_order_desc') }}</template>
    </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref ,onMounted} from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import { useRoute } from 'vue-router'; // Import the route object to get orderId from the URL
import DeleteConfirmationDialog from '../core/DeleteConfirmationDialog.vue';
import OrderService from '@/api/services/OrdersService';
const route = useRoute();
const isDialogOpen = ref<boolean>(false);

let orderId = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id;
onMounted(() => {
  orderId = Array.isArray(route.params.id) ? route.params.id[0] : route.params.id || '';
});


const emit = defineEmits(['onDeleted']);

const { t } = useI18n();
const isDeleteInProgress = ref(false);

async function handleDelete() {
    isDeleteInProgress.value = true;
    
    const { error } = await OrderService.deleteOrder(orderId);
    toast.success(t('order.toasts.delete_success'));
    emit('onDeleted');
    isDeleteInProgress.value = false;

    isDialogOpen.value = false;
    if (error) {
    handleError(error, 'Error deleting item');
    return;
  }
    
}
</script>

<style scoped>
/* Prípadné špecifické štýly pre tento komponent, ak sú potrebné */
</style>
