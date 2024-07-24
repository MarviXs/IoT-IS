<template>
  <q-dialog v-model="isDialogOpen">
    <q-card>
      <q-card-section>
        <div class="text-h6">{{ t('module.remove_device_from_module') }}</div>
      </q-card-section>

      <q-card-section class="q-pt-none">
        {{ t('module.remove_device_from_module_desc', [device.name, module.name]) }}
      </q-card-section>
      <q-card-actions align="right">
        <q-btn v-close-popup flat color="grey-9" :label="t('global.cancel')" no-caps />
        <q-btn
          unelevated
          :label="t('global.delete')"
          color="red"
          :loading="isDeleteInProgress"
          no-caps
          @click="handleDelete()"
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { Device } from '@/models/Device';
import { Module } from '@/models/Module';
import { handleError } from '@/utils/error-handler';
import { ref, PropType } from 'vue';
import { toast } from 'vue3-toastify';
import ModuleService from '@/services/ModuleService';
import { useI18n } from 'vue-i18n';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  device: {
    type: Object as PropType<Device>,
    required: true,
  },
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
    await ModuleService.removeDeviceFromModule(props.module.uid, props.device.uid);
    isDialogOpen.value = false;
    emit('onDeleted');
    toast.success('Device removed from module');
  } catch (error) {
    handleError(error, 'Removing device failed!');
  } finally {
    isDeleteInProgress.value = false;
  }
}
</script>

<style lang="scss" scoped></style>
