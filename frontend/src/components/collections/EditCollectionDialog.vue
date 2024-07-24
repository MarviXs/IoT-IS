<template>
  <dialog-common
    v-model="isDialogOpen"
    :action-label="t('global.save')"
    :loading="updatingCollection"
    @on-submit="updateCollection"
  >
    <template #title>{{ t('collection.edit_collection') }}</template>
    <template #default>
      <collection-form v-model="collectionInput" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { PropType, ref, toRaw, watch } from 'vue';
import type { Collection, CollectionInput } from '@/models/Collection';
import CollectionService from '@/services/CollectionService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import CollectionForm from '@/components/collections/CollectionForm.vue';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  collection: {
    type: Object as PropType<Collection>,
    required: true,
  },
});
const emit = defineEmits(['onUpdate']);

const { t } = useI18n();

const collectionClone = (collection: Collection) => structuredClone(toRaw(collection));

const updatingCollection = ref(false);
const collectionInput = ref<CollectionInput>(collectionClone(props.collection));

async function updateCollection() {
  try {
    updatingCollection.value = true;
    await CollectionService.updateCollection(props.collection.uid, collectionInput.value);
    isDialogOpen.value = false;
    emit('onUpdate');
    toast.success(t('collection.toasts.update_success'));
  } catch (error) {
    handleError(error, t('collection.toasts.update_failed'));
  } finally {
    updatingCollection.value = false;
  }
}

watch(
  () => props.collection,
  (collection) => {
    collectionInput.value = collectionClone(collection);
  },
  { immediate: true },
);
</script>

<style lang="scss" scoped></style>
