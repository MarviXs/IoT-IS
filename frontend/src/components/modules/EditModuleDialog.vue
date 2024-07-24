<template>
  <dialog-common
    v-model="isDialogOpen"
    :action-label="t('global.save')"
    :loading="updatingModule"
    @on-submit="updateModule"
  >
    <template #title>{{ t('module.edit_module') }}</template>
    <template #default>
      <module-form v-model="moduleInput" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { PropType, ref, toRaw, watch } from 'vue';
import ModuleService from '@/services/ModuleService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { Module, ModuleInput } from '@/models/Module';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import ModuleForm from '@/components/modules/ModuleForm.vue';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  module: {
    type: Object as PropType<Module>,
    required: true,
  },
});
const emit = defineEmits(['onUpdate']);

const { t } = useI18n();

const moduleClone = (module: Module) => structuredClone(toRaw(module));

const updatingModule = ref(false);
const moduleInput = ref<ModuleInput>(moduleClone(props.module));

async function updateModule() {
  try {
    updatingModule.value = true;
    await ModuleService.updateModule(props.module.uid, moduleInput.value);
    isDialogOpen.value = false;
    emit('onUpdate');
    toast.success(t('module.toasts.update_success'));
  } catch (error) {
    handleError(error, t('module.toasts.update_failed'));
  } finally {
    updatingModule.value = false;
  }
}

watch(
  () => props.module,
  (value) => {
    moduleInput.value = moduleClone(value);
  },
);
</script>
