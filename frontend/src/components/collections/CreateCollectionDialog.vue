<template>
  <dialog-common
    v-model="isDialogOpen"
    :action-label="t('global.create')"
    :loading="creatingCollection"
    @on-submit="createCollection"
  >
    <template #title>{{ t('collection.create_collection') }}</template>
    <template #default>
      <collection-form v-model="collection" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { CollectionInput } from '@/models/Collection';
import CollectionService from '@/services/CollectionService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import CollectionForm from '@/components/collections/CollectionForm.vue';

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onCreate']);

const { t } = useI18n();

const getEmptyCollection = (): CollectionInput => ({
  name: '',
});

const creatingCollection = ref(false);
const collection = ref<CollectionInput>(getEmptyCollection());

async function createCollection() {
  try {
    creatingCollection.value = true;
    await CollectionService.createCollection(collection.value);
    collection.value = getEmptyCollection();
    isDialogOpen.value = false;
    emit('onCreate');
    toast.success(t('collection.toasts.create_success'));
  } catch (error) {
    handleError(error, t('collection.toasts.create_failed'));
  } finally {
    creatingCollection.value = false;
  }
}
</script>

<style lang="scss" scoped></style>
