<template>
  <div class="row items-center">
    <q-btn :icon="mdiCloseCircleOutline" color="red" round dense flat class="q-mr-sm q-mb-md" @click="emit('remove')" />
    <q-select
      v-model="action.type"
      label="Action"
      outlined
      style="min-width: 140px"
      :options="actionTypeOptions"
      :rules="actionRules"
      class="q-mr-sm"
      emit-value
      map-options
    />
    <div v-if="action.type == 'NOTIFICATION'" class="row">
      <q-select
        v-model="action.notificationSeverity"
        label="Severity"
        outlined
        :options="notificationSeverityOptions"
        emit-value
        map-options
        style="min-width: 150px"
        class="q-mr-sm"
      />
      <q-input
        v-model="action.notificationMessage"
        label="Message"
        outlined
        class="q-mr-sm"
        :rules="notificationMessageRules"
      />
    </div>
    <div v-else-if="action.type == 'JOB'" class="row">
      <q-select
        v-model="action.deviceId"
        label="Device"
        outlined
        :options="deviceOptions"
        emit-value
        map-options
        style="min-width: 150px"
        :rules="deviceRules"
        class="q-mr-sm"
      />
      <q-select
        v-model="action.recipeId"
        label="Recipe"
        outlined
        :options="recipeOptions"
        emit-value
        map-options
        style="min-width: 150px"
        :rules="recipeRules"
        class="q-mr-sm"
      />
    </div>
    <div v-else-if="action.type == 'DISCORD_NOTIFICATION'" class="row">
      <q-select
        v-model="action.notificationSeverity"
        label="Severity"
        outlined
        :options="notificationSeverityOptions"
        emit-value
        map-options
        style="min-width: 150px"
        class="q-mr-sm"
      />
      <q-input
        v-model="action.notificationMessage"
        label="Message"
        outlined
        class="q-mr-sm"
        :rules="notificationMessageRules"
      />
      <q-input
        v-model="action.discordWebhookUrl"
        label="Webhook URL"
        outlined
        class="q-mr-sm"
        :rules="discordWebhookRules"
      />
      <q-checkbox
        v-model="action.includeSensorValues"
        label="Include sensor values"
        dense
        class="q-mt-sm"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import { computed, ref, watch } from 'vue';
import type { SceneAction } from '@/models/Scene';
import { mdiCloseCircleOutline } from '@quasar/extras/mdi-v7';
import type { SceneDevice } from '@/api/services/DeviceService';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const emit = defineEmits(['remove']);

const props = defineProps({
  devices: { type: Array as PropType<SceneDevice>, required: true },
});
const action = defineModel<SceneAction>({ required: true });

const actionTypeOptions = ref([
  { label: 'Notification', value: 'NOTIFICATION' },
  { label: 'Discord notification', value: 'DISCORD_NOTIFICATION' },
  { label: 'Run job', value: 'JOB' },
]);

const notificationSeverityOptions = ref([
  { label: 'Info', value: 'Info' },
  { label: 'Warning', value: 'Warning' },
  { label: 'Serious', value: 'Serious' },
  { label: 'Critical', value: 'Critical' },
]);

const deviceOptions = computed(() => {
  return props.devices?.map((device) => ({
    label: device.name,
    value: device.id,
  }));
});

const recipeOptions = computed(() => {
  const selectedDevice = props.devices.find((device) => device.id === action.value.deviceId);
  return selectedDevice?.recipes.map((recipe) => ({
    label: recipe.name,
    value: recipe.id,
  }));
});

watch(
  () => action.value.deviceId,
  () => {
    action.value.recipeId = undefined;
  },
);

const actionRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const deviceRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const recipeRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const notificationMessageRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const discordWebhookRules = [
  (val: string | null) => {
    if (!val || val.length === 0) {
      return t('global.rules.required');
    }

    try {
      new URL(val);
      return true;
    } catch {
      return 'Invalid URL';
    }
  },
];

watch(
  () => action.value.type,
  (type) => {
    if (type === 'NOTIFICATION') {
      if (!action.value.notificationSeverity) {
        action.value.notificationSeverity = 'Info';
      }
      if (!action.value.notificationMessage) {
        action.value.notificationMessage = '';
      }
      action.value.discordWebhookUrl = null;
      action.value.deviceId = null;
      action.value.recipeId = null;
      action.value.includeSensorValues = false;
    } else if (type === 'DISCORD_NOTIFICATION') {
      if (!action.value.notificationSeverity) {
        action.value.notificationSeverity = 'Info';
      }
      if (!action.value.notificationMessage) {
        action.value.notificationMessage = '';
      }
      action.value.deviceId = null;
      action.value.recipeId = null;
      if (!action.value.discordWebhookUrl) {
        action.value.discordWebhookUrl = '';
      }
      if (action.value.includeSensorValues === undefined || action.value.includeSensorValues === null) {
        action.value.includeSensorValues = false;
      }
    } else if (type === 'JOB') {
      action.value.notificationSeverity = null;
      action.value.notificationMessage = null;
      action.value.discordWebhookUrl = null;
      action.value.includeSensorValues = false;
    }
  },
  { immediate: true },
);
</script>

<style scoped lang="scss"></style>
