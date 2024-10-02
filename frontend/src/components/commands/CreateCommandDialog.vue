<template>
  <dialog-common
    v-model="isDialogOpen"
    :action-label="t('global.create')"
    :loading="creatingCommand"
    @on-submit="createCommand"
  >
    <template #title>{{ t('command.create_command') }}</template>
    <template #default>
      <CommandForm ref="commandForm" v-model="command" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import CommandService from '@/api/services/CommandService';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import CommandForm, { CommandFormData } from '@/components/commands/CommandForm.vue';
import { useRoute } from 'vue-router';
import { CreateCommandRequest } from '@/api/services/CommandService';

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onCreate']);

const { t } = useI18n();
const route = useRoute();

const creatingCommand = ref(false);
const command = ref<CommandFormData>({
  displayName: '',
  name: '',
  params: [],
});
const commandForm = ref();

async function createCommand() {
  if (!commandForm.value?.validate()) {
    return;
  }

  const createCommandRequest: CreateCommandRequest = {
    displayName: command.value.displayName,
    name: command.value.name,
    deviceTemplateId: route.params.id as string,
    params: command.value.params,
  };

  for (let i = 0; createCommandRequest.params && i < createCommandRequest.params.length; i++) {
    if (createCommandRequest.params[i] === null) {
      createCommandRequest.params.splice(i, 1);
    }
  }

  creatingCommand.value = true;
  const { error } = await CommandService.createCommand(createCommandRequest);
  creatingCommand.value = false;

  if (error) {
    handleError(error, t('command.toasts.create_failed'));
    return;
  }

  command.value = {
    displayName: '',
    name: '',
    params: [],
  };

  isDialogOpen.value = false;
  emit('onCreate');
  toast.success(t('command.toasts.create_success'));
}
</script>

<style lang="scss" scoped></style>
