<template>
  <q-dialog v-model="isDialogOpen">
    <q-card class="q-pa-xs" style="max-width: 380px; width: 100%">
      <q-card-section>
        <div class="text-h6">{{ t('device_template.change_owner') }}</div>
        <div class="text-body2 text-grey-7 q-mt-xs">
          {{ currentOwnerLabel }}
        </div>
      </q-card-section>
      <q-card-section class="q-pt-none column q-gutter-md">
        <UserSelect v-model="selectedOwner" :label="t('device_template.select_new_owner')" />
        <q-btn
          unelevated
          color="primary"
          :label="t('global.save')"
          no-caps
          :loading="isSaving"
          :disable="!selectedOwner || selectedOwner.id === props.currentOwnerId"
          @click="changeOwner"
        />
      </q-card-section>
      <q-card-actions align="right" class="text-primary">
        <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import { handleError } from '@/utils/error-handler';
import AdminDeviceTemplateService from '@/api/services/AdminDeviceTemplateService';
import UserSelect, { type UserSelectOption } from '@/components/users/UserSelect.vue';

const isDialogOpen = defineModel<boolean>({ required: true });

const props = defineProps({
  templateId: {
    type: String,
    required: true,
  },
  currentOwnerId: {
    type: String,
    required: true,
  },
  currentOwnerEmail: {
    type: String,
    required: false,
    default: undefined,
  },
});

const emit = defineEmits(['onChanged']);

const { t } = useI18n();

const selectedOwner = ref<UserSelectOption | null>(null);
const isSaving = ref(false);

const currentOwnerLabel = computed(() => {
  if (!props.currentOwnerEmail) {
    return t('device_template.current_owner_unknown');
  }

  return t('device_template.current_owner', { email: props.currentOwnerEmail });
});

watch(
  isDialogOpen,
  (open) => {
    if (!open) {
      selectedOwner.value = null;
    }
  },
  { immediate: true },
);

async function changeOwner() {
  if (!selectedOwner.value || selectedOwner.value.id === props.currentOwnerId) {
    return;
  }

  isSaving.value = true;
  const { error } = await AdminDeviceTemplateService.updateOwner(props.templateId, {
    ownerId: selectedOwner.value.id,
  });
  isSaving.value = false;

  if (error) {
    handleError(error, t('device_template.toasts.change_owner_failed'));
    return;
  }

  toast.success(t('device_template.toasts.change_owner_success'));
  emit('onChanged');
  isDialogOpen.value = false;
}
</script>
