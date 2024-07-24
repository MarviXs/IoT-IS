<template>
  <div>
    <q-input ref="nameRef" v-model="device.name" :rules="nameRules" autofocus :label="t('global.name')" />
    <device-template-select v-model="device.deviceTemplate" />
    <q-input v-model="device.accessToken" class="q-mt-lg q-mb-sm" :label="t('device.access_token')">
      <template #append>
        <q-icon v-if="!device.accessToken" :name="mdiAutorenew" class="cursor-pointer" @click="generateAccessToken">
          <q-tooltip>{{ t('device.generate_access_token') }}</q-tooltip>
        </q-icon>
        <q-icon v-else :name="mdiContentCopy" class="cursor-pointer" @click="copyAccessToken">
          <q-tooltip>{{ copied ? t('device.access_token_copied') : t('device.copy_access_token') }}</q-tooltip>
        </q-icon>
      </template>
    </q-input>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { isFormValid } from '@/utils/form-validation';
import DeviceTemplateSelect from '@/components/device-templates/DeviceTemplateSelect.vue';
import { mdiAutorenew, mdiContentCopy } from '@quasar/extras/mdi-v6';
import { copyToClipboard } from 'quasar';
import { DeviceTemplateSelectData } from '@/components/device-templates/DeviceTemplateSelect.vue';

export interface DeviceFormData {
  name: string;
  accessToken: string;
  deviceTemplate?: DeviceTemplateSelectData;
}

const { t } = useI18n();

async function generateAccessToken() {
  const charset = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  const tokenLength = 24;
  const randomValues = new Uint8Array(tokenLength);
  window.crypto.getRandomValues(randomValues);
  let token = '';
  for (let i = 0; i < tokenLength; i++) {
    token += charset[randomValues[i] % charset.length];
  }
  device.value.accessToken = token;
}

const copied = ref(false);
function copyAccessToken() {
  copyToClipboard(device.value.accessToken || '');
  copied.value = true;
  setTimeout(() => {
    copied.value = false;
  }, 2000);
}

const device = defineModel<DeviceFormData>({ required: true });
const nameRef = ref();
const nameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

function validate() {
  const refs = [nameRef.value];
  return isFormValid(refs);
}
defineExpose({
  validate,
});
</script>

<style lang="scss" scoped></style>
