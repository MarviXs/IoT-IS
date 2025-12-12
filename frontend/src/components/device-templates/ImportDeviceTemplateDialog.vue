<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>Import Template</template>
    <template #default>
      <q-card-section class="q-pt-none column q-gutter-md">
        <q-input
          v-model="templateName"
          autofocus
          :label="t('global.name')"
        />
        <q-file outlined v-model="templateFile" label="Select Template File" @update:model-value="onFileSelected">
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

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onImported']);

const { t } = useI18n();

const templateName = ref('');
const templateFile = ref<File | null>(null);
const importingTemplate = ref(false);
const parsedTemplateData = ref<any | null>(null);

async function importTemplate() {
  if (!templateFile.value) {
    toast.error(t('device_template.toasts.no_file_selected') ?? 'No template file selected');
    return;
  }

  const loadTemplateData = async () => {
    if (parsedTemplateData.value) {
      return parsedTemplateData.value;
    }

    const reader = new FileReader();
    const promise = new Promise<any | null>((resolve) => {
      reader.onload = () => {
        const fileContent = reader.result as string;
        try {
          resolve(JSON.parse(fileContent));
        } catch {
          resolve(null);
        }
      };
    });
    reader.readAsText(templateFile.value);
    return await promise;
  };

  const templateData = await loadTemplateData();
  if (!templateData || !templateData.templateData) {
    toast.error('Invalid template file');
    return;
  }

  const incomingName = templateData?.templateData?.name ?? '';
  const finalName = templateName.value?.trim() || incomingName;
  if (!finalName) {
    toast.error(t('global.rules.required'));
    return;
  }
  templateName.value = finalName;
  templateData.templateData.name = finalName;

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
}

async function onFileSelected(file: File | null) {
  parsedTemplateData.value = null;
  if (!file) {
    return;
  }

  const reader = new FileReader();
  reader.onload = () => {
    try {
      const data = JSON.parse(reader.result as string);
      parsedTemplateData.value = data;
      const incomingName = data?.templateData?.name;
      if (!templateName.value && incomingName) {
        templateName.value = incomingName;
      }
    } catch {
      parsedTemplateData.value = null;
    }
  };
  reader.readAsText(file);
}
</script>

<style lang="scss" scoped></style>
