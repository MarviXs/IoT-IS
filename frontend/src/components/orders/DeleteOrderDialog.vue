<template>
    <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
        <template #title>{{ t('order.delete_order') }}</template>
        <template #description>{{ t('order.delete_order_desc') }}</template>
    </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '../core/DeleteConfirmationDialog.vue';
import OrderService from '@/api/services/OrdersService';

const isDialogOpen = ref<boolean>(false);


const props = defineProps({
    orderId: {
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
    try {
        await OrderService.deleteOrder(props.orderId);
        toast.success(t('order.toasts.delete_success'));
        emit('onDeleted');
        isDialogOpen.value = false;
    } catch (error) {
        handleError(error, t('order.toasts.delete_error'));
    } finally {
        isDeleteInProgress.value = false;
    }
}
</script>

<style scoped>
/* Prípadné špecifické štýly pre tento komponent, ak sú potrebné */
</style>
