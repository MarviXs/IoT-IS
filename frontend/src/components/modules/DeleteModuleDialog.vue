<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('module.delete_module') }}</template>
    <template #description>{{ t('module.delete_module_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { PropType, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DeleteConfirmationDialog from '../core/DeleteConfirmationDialog.vue';
import { Module } from '@/models/Module';
import ModuleService from '@/services/ModuleService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  module: {
    type: Object as PropType<Module>,
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();

const isDeleteInProgress = ref(false);

async function handleDelete() {
  try {
    isDeleteInProgress.value = true;
    await ModuleService.deleteModule(props.module.uid);
    isDialogOpen.value = false;
    emit('onDeleted');
    toast.success(t('module.toasts.delete_success'));
  } catch (error) {
    handleError(error, t('module.toasts.delete_failed'));
  } finally {
    isDeleteInProgress.value = false;
  }
}
</script>
