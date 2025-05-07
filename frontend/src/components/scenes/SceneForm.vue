<template>
  <q-form @submit="emit('onSubmit')" ref="formRef" greedy>
    <q-btn type="submit" style="display: none" />
    <q-card class="q-pa-lg shadow">
      <div class="row items-center q-col-gutter-x-xl">
        <q-input v-model="scene.name" :label="t('global.name')" class="col-12" :rules="nameRules" />
        <q-input v-model="scene.description" :label="t('global.description')" class="col-12" type="textarea" autogrow />
        <q-input
          v-model="scene.cooldownAfterTriggerTime"
          label="Cooldown after trigger (seconds)"
          class="col-12 q-mt-md"
          type="number"
          :rules="triggerDeactivateRules"
        />
        <q-toggle dense v-model="scene.isEnabled" label="Active" class="q-mt-sm" />
      </div>
    </q-card>
    <div class="q-mt-md">
      <div class="card-name text-dark-blue">Conditions</div>
      <q-card class="q-pa-lg shadow">
        <SceneRuleGroup v-model="scene.condition" :depth="0" :is-root="true" :devices="devices ?? []" />
        <!-- {{ scene.condition }} -->
      </q-card>
    </div>
    <div class="q-mt-md">
      <div class="card-name text-dark-blue">Actions</div>
      <q-card class="q-pa-lg shadow">
        <SceneActions v-model="scene.actions" :devices="devices ?? []" />
      </q-card>
    </div>
  </q-form>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { Scene } from '@/models/Scene';
import SceneRuleGroup from './SceneRuleGroup.vue';
import { ref } from 'vue';
import DeviceService, { SceneDevice } from '@/api/services/DeviceService';
import { handleError } from '@/utils/error-handler';
import SceneActions from './SceneActions.vue';

const { t } = useI18n();
const emit = defineEmits(['onSubmit']);

const scene = defineModel<Scene>({ required: true });

const devices = ref<SceneDevice>();
async function getDevices() {
  const { data, error } = await DeviceService.getSceneDevices();

  if (error) {
    handleError(error, 'Failed to load devices');
    return;
  }

  devices.value = data;
}
getDevices();

const nameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const triggerDeactivateRules = [(val: string) => (!isNaN(Number(val)) && Number(val) >= 0) || t('global.rules.number')];

const formRef = ref();
async function validate() {
  return await formRef.value?.validate();
}

defineExpose({ validate });
</script>

<style scoped>
.card-name {
  font-size: 1.1rem;
  font-weight: 500;
  margin-bottom: 10px;
}

.gap-md {
  gap: 10px;
}
</style>
