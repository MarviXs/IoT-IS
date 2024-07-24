<template>
  <dialog-common
    v-model="isDialogOpen"
    :action-label="t('global.save')"
    :loading="updatingCommand"
    @on-submit="updateCommand"
  >
    <template #title>{{ t('command.edit_command') }}</template>
    <template #default>
      <CommandForm ref="commandForm" v-model="command" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import CommandService from '@/api/services/CommandService';
import CommandForm from './CommandForm.vue';
import { CommandResponse } from '@/api/types/Command';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  commandId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onUpdate']);
const { t } = useI18n();

const command = ref<CommandResponse>({
  id: '',
  displayName: '',
  name: '',
  params: [],
});
async function getCommand() {
  const { data, error } = await CommandService.getCommand(props.commandId);
  if (error) {
    handleError(error, t('command.toasts.load_failed'));
    return;
  }
  command.value = data;
}
getCommand();

const updatingCommand = ref(false);
const commandForm = ref();

async function updateCommand() {
  if (!commandForm.value?.validate()) {
    return;
  }

  if (!command.value.params) {
    command.value.params = [];
  }
  for (let i = 0; i < command.value.params.length; i++) {
    if (command.value.params[i] === null) {
      command.value.params.splice(i, 1);
    }
  }

  updatingCommand.value = true;
  const { data, error } = await CommandService.updateCommand(props.commandId, command.value);
  updatingCommand.value = false;

  if (error) {
    handleError(error, t('command.toasts.update_failed'));
    return;
  }

  emit('onUpdate');
  isDialogOpen.value = false;
  toast.success(t('command.toasts.update_success'));
}
watch(
  () => props.commandId,
  async () => {
    command.value = {
      id: '',
      displayName: '',
      name: '',
      params: [],
    };
    getCommand();
  },
);
</script>

<style lang="scss" scoped></style>
