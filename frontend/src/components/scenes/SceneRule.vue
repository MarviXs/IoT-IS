<template>
  <div>
    <div v-if="isGroup(rule)">
      <SceneRuleGroup v-model="rule" :is-root="false" :depth="depth" @remove="emit('remove')" />
    </div>
    <div class="row items-center gap-md" v-else>
      <q-btn :icon="mdiCloseCircleOutline" color="red" round dense flat class="q-ma-xs" @click="emit('remove')" />
      <div>Rule</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { RulesLogic } from 'json-logic-js';
import SceneRuleGroup from './SceneRuleGroup.vue';
import { mdiCloseCircleOutline } from '@quasar/extras/mdi-v7';

const props = defineProps({
  depth: { type: Number, required: true },
});
const emit = defineEmits(['remove']);
const rule = defineModel<RulesLogic>({ required: true });

function isGroup(rule: RulesLogic) {
  return typeof rule === 'object' && ('and' in rule || 'or' in rule);
}
</script>
