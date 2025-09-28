<template>
  <div>
    <div class="q-mt-sm" v-for="(action, index) in actions" :key="index">
      <SceneAction v-model="actions[index]" @remove="removeAction(index)" :devices="devices" />
    </div>
    <q-btn label="Add action" color="primary" flat dense :icon="mdiPlus" @click="addAction" />
  </div>
</template>

<script setup lang="ts">
import type { PropType } from 'vue';
import type { SceneAction as SceneActionModel } from '@/models/Scene';
import SceneAction from './SceneAction.vue';
import type { SceneDevice } from '@/api/services/DeviceService';
import { mdiPlus } from '@quasar/extras/mdi-v7';

const actions = defineModel<SceneActionModel[]>({ required: true });

defineProps({
  devices: { type: Array as PropType<SceneDevice>, required: true },
});

function addAction() {
  actions.value.push({
    type: 'NOTIFICATION',
    notificationMessage: '',
    notificationSeverity: 'Info',
    deviceId: null,
    recipeId: null,
  });
}

function removeAction(index: number) {
  actions.value.splice(index, 1);
}
</script>

<style scoped lang="scss"></style>
