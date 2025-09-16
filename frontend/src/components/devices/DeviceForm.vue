<template>
  <q-form @submit="onSubmit" ref="deviceForm">
    <q-card-section class="q-pt-none column q-gutter-md">
      <q-input v-model="device.name" :rules="nameRules" autofocus :label="t('global.name')" />
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
      <q-select
        v-model="device.protocol"
        :options="protocolOptions"
        label="Protocol"
        class="q-mt-lg"
        :rules="[(val) => !!val || t('global.rules.required')]"
        emit-value
        map-options
      />
    </q-card-section>
    <q-card-actions align="right" class="text-primary">
      <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
      <q-btn
        unelevated
        color="primary"
        :label="t('global.save')"
        type="submit"
        no-caps
        padding="6px 20px"
        :loading="props.loading"
      />
    </q-card-actions>
  </q-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import DeviceTemplateSelect from '@/components/device-templates/DeviceTemplateSelect.vue';
import { mdiAutorenew, mdiContentCopy } from '@quasar/extras/mdi-v7';
import { copyToClipboard } from 'quasar';
import type { DeviceTemplateSelectData } from '@/components/device-templates/DeviceTemplateSelect.vue';

export interface DeviceFormData {
  name: string;
  accessToken: string;
  deviceTemplate?: DeviceTemplateSelectData;
  protocol: 'MQTT' | 'HTTP';
}

const props = defineProps<{
  loading?: boolean;
}>();

const emit = defineEmits(['onSubmit']);

const { t } = useI18n();

const protocolOptions = [
  { label: 'MQTT', value: 'MQTT' },
  { label: 'HTTP', value: 'HTTP' },
];

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
const nameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

const deviceForm = ref();

function onSubmit() {
  if (!deviceForm.value?.validate()) {
    return;
  }
  emit('onSubmit');
}
</script>

<style lang="scss" scoped></style>
