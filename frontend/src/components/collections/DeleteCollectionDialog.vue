<template>
  <DeleteConfirmationDialog v-model="isDialogOpen" :loading="isDeleteInProgress" @on-submit="handleDelete">
    <template #title>{{ t('collection.delete_collection') }}</template>
    <template #description>{{ t('collection.delete_collection_desc') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { handleError } from '@/utils/error-handler';
import { PropType, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import CollectionService from '@/services/CollectionService';
import DeleteConfirmationDialog from '@/components/core/DeleteConfirmationDialog.vue';
import { Collection } from '@/models/Collection';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  collection: {
    type: Object as PropType<Collection>,
    required: true,
  },
});
const emit = defineEmits(['onDeleted']);

const { t } = useI18n();

const isDeleteInProgress = ref(false);
async function handleDelete() {
  try {
    isDeleteInProgress.value = true;
    await CollectionService.deleteCollection(props.collection.uid);
    isDialogOpen.value = false;
    emit('onDeleted');
    toast.success(t('collection.toasts.delete_success'));
  } catch (error) {
    handleError(error, t('collection.toasts.delete_failed'));
  } finally {
    isDeleteInProgress.value = false;
  }
}
</script>
