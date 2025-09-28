<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>Add device to collection</template>
    <template #default>
      <q-form @submit="addDeviceToCollection">
        <q-card-section class="q-pt-none column q-gutter-md">
          <DeviceSelect v-model="selectedDevice" />
        </q-card-section>
        <q-card-actions align="right" class="text-primary">
          <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
          <q-btn
            unelevated
            color="primary"
            :label="t('global.add')"
            type="submit"
            no-caps
            padding="6px 20px"
            :loading="isLoading"
          />
        </q-card-actions>
      </q-form>
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
import type { DeviceSelectData } from '../devices/DeviceSelect.vue';
import DeviceSelect from '../devices/DeviceSelect.vue';

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onAdd']);

const { t } = useI18n();

const props = defineProps({
  collectionParentId: {
    type: String,
    required: true,
  },
});

const isLoading = ref(false);
const selectedDevice = ref<DeviceSelectData>();

async function addDeviceToCollection() {
  if (!selectedDevice.value?.id) {
    return;
  }

  isLoading.value = true;
  const { error } = await CollectionService.addDeviceToCollection(props.collectionParentId, selectedDevice.value.id);
  isLoading.value = false;
  isDialogOpen.value = false;

  if (error) {
    handleError(error, 'Adding device to collection failed');
  }

  toast.success('Device added to collection');
  emit('onAdd', props.collectionParentId, selectedDevice.value.id);
  selectedDevice.value = { id: '', name: '' };
}
</script>

<style lang="scss" scoped></style>
