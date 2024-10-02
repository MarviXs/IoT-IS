<template>
  <dialog-common
    v-model="isDialogOpen"
    :action-label="t('global.create')"
    :loading="isCreatingCollection"
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
import CollectionService from '@/api/services/DeviceCollectionService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import CollectionForm from '@/components/collections/CollectionForm.vue';
import { CreateCollectionRequest, UpdateCollectionRequest } from '@/api/services/DeviceCollectionService';

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onCreate']);

const { t } = useI18n();

const props = defineProps({
  collectionParentId: {
    type: String,
    required: false,
    default: '',
  },
});

const isCreatingCollection = ref(false);
const collection = ref<UpdateCollectionRequest>({
  name: '',
});

async function createCollection() {
  const request: CreateCollectionRequest = {
    name: collection.value.name,
  };

  if (props.collectionParentId) {
    request.collectionParentId = props.collectionParentId;
  }

  isCreatingCollection.value = true;
  const { data, error } = await CollectionService.createCollection(request);
  isCreatingCollection.value = false;
  isDialogOpen.value = false;

  collection.value = { name: '' };

  if (error) {
    handleError(error, 'Failed to create collection');
  }

  toast.success(t('collection.toasts.create_success'));
  emit('onCreate', data, props.collectionParentId);
}
</script>

<style lang="scss" scoped></style>
