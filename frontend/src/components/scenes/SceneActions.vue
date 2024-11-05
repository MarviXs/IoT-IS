<template>
  <div>
    <q-btn label="Add action" color="primary q-mb-md" unelevated @click="addAction" />
    <div v-if="actions.length == 0">
      <div class="text-secondary">No actions yet</div>
    </div>
    <div class="q-mt-sm" v-for="(action, index) in actions" :key="index">
      <SceneAction v-model="actions[index]" @remove="removeAction(index)" :devices="devices" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { PropType, ref } from 'vue';
import { SceneAction as SceneActionModel } from '@/models/Scene';
import SceneAction from './SceneAction.vue';
import { SceneDevice } from '@/api/services/DeviceService';

const actions = defineModel<SceneActionModel[]>({ required: true });

const props = defineProps({
  devices: { type: Array as PropType<SceneDevice>, required: true },
});

function addAction() {
  actions.value.push({
    type: 'notification',
    notificationMessage: '',
    recipeId: '',
  });
}

function removeAction(index: number) {
  actions.value.splice(index, 1);
}
</script>

<style scoped lang="scss"></style>
