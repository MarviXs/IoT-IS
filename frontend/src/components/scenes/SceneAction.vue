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
  </div>
</template>

<script setup lang="ts">
import { computed, PropType, ref, watch } from 'vue';
import { SceneAction } from '@/models/Scene';
import { mdiCloseCircleOutline } from '@quasar/extras/mdi-v7';
import { SceneDevice } from '@/api/services/DeviceService';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const emit = defineEmits(['remove']);

const props = defineProps({
  devices: { type: Array as PropType<SceneDevice>, required: true },
});
const action = defineModel<SceneAction>({ required: true });

const actionTypeOptions = ref([
  { label: 'Notification', value: 'NOTIFICATION' },
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
</script>

<style scoped lang="scss"></style>
