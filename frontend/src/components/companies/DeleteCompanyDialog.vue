<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('company.delete_company') }}</template>
    <template #description>{{ t('company.delete_company_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '../core/DeleteConfirmationDialog.vue';
import CompanyService from '@/api/services/CompanyService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  companyId: {
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
  const { error } = await CompanyService.deleteCompany(props.companyId);
  isDeleteInProgress.value = false;

  if (error) {
    handleError(error, t('company.toasts.delete_failed'));
    return;
  }

  toast.success(t('company.toasts.delete_success'));
  emit('onDeleted');
  isDialogOpen.value = false;
}
</script>
