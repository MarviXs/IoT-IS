<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('collection.edit_collection') }}</template>
    <template #default>
      <collection-form v-model="collection" @on-submit="updateCollection" :is-loading="updatingCollection" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import CollectionService from '@/api/services/DeviceCollectionService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import CollectionForm from '@/components/collections/CollectionForm.vue';
import { UpdateCollectionRequest } from '@/api/services/DeviceCollectionService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  collectionId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onUpdate']);

const { t } = useI18n();

async function getCollection() {
  const { data, error } = await CollectionService.getCollection(props.collectionId, 0);
  if (error) {
    handleError(error, t('collection.toasts.load_failed'));
    return;
  }

  collection.value = {
    name: data.name,
  };
}

const updatingCollection = ref(false);
const collection = ref<UpdateCollectionRequest>({
  name: '',
});

async function updateCollection() {
  const collectionRequest: UpdateCollectionRequest = {
    name: collection.value?.name,
  };

  updatingCollection.value = true;
  const { data, error } = await CollectionService.updateCollection(props.collectionId, collectionRequest);
  updatingCollection.value = false;

  if (error) {
    handleError(error, t('collection.toasts.update_failed'));
    return;
  }

  isDialogOpen.value = false;
  emit('onUpdate', data);
  toast.success(t('collection.toasts.update_success'));
}

watch(
  () => props.collectionId,
  (collection) => {
    if (collection) {
      getCollection();
    }
  },
  { immediate: true },
);
</script>

<style lang="scss" scoped></style>
