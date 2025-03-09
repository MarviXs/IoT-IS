<template>
    <q-card class="shadow q-pa-lg">
      <div class="row items-center q-mb-md">
        <div class="col-4 text-h6">{{ documentType }}</div>
        <div class="col-8">
          <q-form ref="qform" autocomplete="off">
            <q-input
              ref="linkRef"
              v-model="link"
              autocomplete="off"
              :label="t('account.document_link')"
              type="text"
              :rules="linkRules"
              lazy-rules
            >
              <template v-slot:append>
                <q-icon
                  name="search"
                  class="cursor-pointer"
                  @click="openFileExplorer"
                />
              </template>
            </q-input>
  
            <input
              ref="fileInput"
              type="file"
              style="display: none"
              @change="handleFileSelection"
            />
  
            <q-btn
              class="float-right q-mt-lg"
              style="min-width: 95px"
              color="primary"
              unelevated
              type="submit"
              :label="t('global.save')"
              :loading="loading"
              no-caps
              @click.prevent="onSubmit"
            ></q-btn>
          </q-form>
        </div>
      </div>
    </q-card>
  </template>
  
  <script setup lang="ts">
  import { ref } from 'vue';
  import { QForm, QInput } from 'quasar';
  import { useI18n } from 'vue-i18n';
  
  defineProps({
    documentType: {
      type: String,
      required: true,
    },
    loading: {
      type: Boolean,
      required: false,
    },
  });
  const emit = defineEmits(['onSubmit']);
  const link = defineModel({ type: String, default: '' });
  
  const { t } = useI18n();
  
  const qform = ref<QForm>();
  const fileInput = ref<HTMLInputElement | null>(null); 
  
  const linkRules = [
    (val: string) => (val && val.length > 0) || t('global.rules.required'),
    (val: string) => {
      const urlRegex = /^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/;
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
    const fileName = file.name; 
    const fileUrl = URL.createObjectURL(file); 

    link.value = fileName; 
  }
}
  async function onSubmit() {
    const valid = qform.value?.validate();
    if (!valid) return;
  
    emit('onSubmit', link.value);
  }
  </script>
  
  <style lang="scss" scoped></style>