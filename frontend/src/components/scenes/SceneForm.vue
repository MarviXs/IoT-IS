<template>
  <q-form @submit="emit('onSubmit')" ref="formRef" greedy>
    <q-btn type="submit" style="display: none" />
    <q-card class="q-pa-lg shadow">
      <div class="row items-center q-col-gutter-x-xl q-col-gutter-y-md">
        <q-input v-model="scene.name" :label="t('global.name')" class="col-12" />
        <q-input v-model="scene.description" :label="t('global.description')" class="col-12" type="textarea" autogrow />
      </div>
    </q-card>
    <div class="q-mt-md">
      <div class="card-name text-secondary">Conditions</div>
      <q-card class="q-pa-lg shadow">
        <SceneRuleGroup v-model="scene.condition.if[0]" :depth="0" :is-root="true" />
      </q-card>
    </div>
    <q-card class="q-pa-lg shadow">
      {{ scene.condition }}
    </q-card>
  </q-form>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { Scene } from '@/models/Scene';
import SceneRuleGroup from './SceneRuleGroup.vue';

const { t } = useI18n();
const emit = defineEmits(['onSubmit']);

const scene = defineModel<Scene>({ required: true });
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
