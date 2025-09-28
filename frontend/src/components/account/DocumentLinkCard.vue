<template>
  <q-card class="shadow q-pa-lg q-mb-md">
    <div class="row items-center q-mb-md">
      <div class="col-4 text-h6">{{ documentHeader }}</div>
      <div class="col-8">
        <q-input
          ref="linkRef"
          v-model="link"
          autocomplete="off"
          :label="t('account.document_link')"
          type="text"
          :rules="linkRules"
          lazy-rules
          readonly
        >
          <template v-slot:append>
            <q-icon name="search" class="cursor-pointer" @click="openFileExplorer" />
          </template>
        </q-input>

        <input ref="fileInput" type="file" style="display: none" @change="handleFileSelection" />

        <div class="tw-flex tw-justify-end tw-gap-4">
          <q-btn
            class="float-right q-mt-lg"
            style="min-width: 95px"
            color="primary"
            unelevated
            :label="t('global.download')"
            :loading="loading"
            no-caps
            @click="downloadTemplate"
          ></q-btn>

          <q-btn
            class="float-right q-mt-lg"
            style="min-width: 95px"
            color="primary"
            unelevated
            :label="t('global.save')"
            :loading="loading"
            no-caps
            @click="onFileSave"
          ></q-btn>
        </div>
      </div>
    </div>
  </q-card>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import { ref, watch } from 'vue';
import { QInput } from 'quasar';
import { useI18n } from 'vue-i18n';
import TemplatesService from '@/api/services/TemplatesService';
import { toast } from 'vue3-toastify';
import { EDocumentIdentifier } from '@/api/types/EDocumentIdentifier';

const props = defineProps({
  documentHeader: {
    type: String,
    required: true,
  },
  loading: {
    type: Boolean,
    required: false,
  },
  documentType: {
    type: Number as PropType<EDocumentIdentifier>,
    required: true,
  },
  fileName: {
    type: String,
    required: true,
  },
});

defineEmits(['onSubmit']);
const link = defineModel({ type: String, default: '' });

const { t } = useI18n();

const fileInput = ref<HTMLInputElement | null>(null);

const linkRules = [
  (val: string) => {
    // Nepíš chybu, ak je pole prázdne
    if (!val) return true;
    // Inak validuj dĺžku
    return val.length > 0 || t('global.rules.required');
  },
  (val: string) => {
    if (!val) return true;
    const urlRegex = /^[\w,\s-]+\.[A-Za-z]{3,4}$/;
    return urlRegex.test(val) || t('account.rules.link_invalid');
  },
];

function openFileExplorer() {
  fileInput.value?.click();
}

function handleFileSelection(event: Event) {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    const file = target.files[0];
    link.value = file.name;
  }
}

function onFileSave() {
  const file = fileInput.value?.files?.[0];

  if (!file) {
    toast.error('Empty field!');
    return;
  }

  const formData = new FormData();
  formData.append('File', file);
  formData.append('Identifier', EDocumentIdentifier[props.documentType]);

  TemplatesService.updateTemplate(formData)
    .then(() => {
      toast.success('Document updated successfully');
    })
    .catch(() => {
      toast.error("Couldn't upload document");
    });
}

function downloadTemplate() {
  TemplatesService.downloadTemplate(props.documentType)
    .then((retVal) => {
      const contentDisposition = retVal.response.headers.get('Content-Disposition');
      const parts = contentDisposition?.split('filename=');
      const filename = parts?.length === 2 ? parts[1] : 'order_template.xlsx';

      retVal.response.blob().then((blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', filename);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      });
    })
    .catch(() => {
      toast.error("Couldn't download document");
    });
}

watch(
  () => props.fileName,
  (newVal) => {
    link.value = newVal;
  },
);
</script>

<style lang="scss" scoped></style>
