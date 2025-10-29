<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>Import Template</template>
    <template #default>
      <q-card-section class="q-pt-none column q-gutter-md">
        <q-input
          v-model="templateName"
          autofocus
          :label="t('global.name')"
          :rules="[(val) => !!val || t('global.rules.required')]"
        />
        <q-toggle
          v-if="authStore.isAdmin"
          v-model="isGlobal"
          :label="t('device_template.global_template')"
          :hint="t('device_template.global_template_hint')"
        />
        <q-file outlined v-model="templateFile" label="Select Template File">
          <template v-slot:prepend>
            <q-icon name="attach_file" />
          </template>
        </q-file>
      </q-card-section>
      <q-card-actions align="right" class="text-primary">
        <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
        <q-btn
          unelevated
          color="primary"
          label="Import"
          type="submit"
          no-caps
          padding="6px 20px"
          :loading="importingTemplate"
          @click="importTemplate"
        />
      </q-card-actions>
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';
import { useAuthStore } from '@/stores/auth-store';

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onImported']);

const { t } = useI18n();

const templateName = ref('');
const templateFile = ref<File | null>(null);
const importingTemplate = ref(false);
const isGlobal = ref(false);
const authStore = useAuthStore();

async function importTemplate() {
  if (!templateFile.value) {
    toast.error(t('device_template.toasts.no_file_selected'));
    return;
  }

  // json file to object
  const reader = new FileReader();
  reader.readAsText(templateFile.value);
  reader.onload = async () => {
    const fileContent = reader.result as string;
    const templateData = JSON.parse(fileContent);

    if (!templateData) {
      toast.error('Invalid template file');
      return;
    }

    templateData.templateData.name = templateName.value;
    templateData.templateData.isGlobal = isGlobal.value;

    importingTemplate.value = true;
    const { error } = await DeviceTemplateService.importDeviceTemplate(templateData);
    importingTemplate.value = false;

    if (error) {
      handleError(error, 'Importing template failed');
      return;
    }

    emit('onImported');
    isDialogOpen.value = false;

    toast.success('Template imported successfully');
  };
}
</script>

<style lang="scss" scoped></style>
