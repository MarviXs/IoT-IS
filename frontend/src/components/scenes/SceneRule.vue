<template>
  <div>
    <div v-if="isGroup(rule)">
      <SceneRuleGroup v-model="rule" :is-root="false" :depth="depth" :devices="devices" @remove="emit('remove')" />
    </div>
    <div class="row items-center gap-md" v-else>
      <q-btn :icon="mdiCloseCircleOutline" color="red" round dense flat class="q-ma-xs" @click="emit('remove')" />
      <SceneDeviceOperand v-model="leftCompare" :devices="devices" />
      <q-select
        v-model="comparisonOperator"
        :options="compareOptions"
        label="Operand"
        outlined
        :rules="operatorRules"
        class="q-ma-xs"
        emit-value
        map-options
      />
      <SceneConstantOperand v-model="rightCompare" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { RulesLogic } from 'json-logic-js';
import SceneRuleGroup from './SceneRuleGroup.vue';
import { mdiCloseCircleOutline } from '@quasar/extras/mdi-v7';
import { DevicesWithSensorsResponse } from '@/api/services/DeviceService';
import { computed, PropType } from 'vue';
import SceneDeviceOperand from './SceneDeviceOperand.vue';
import SceneConstantOperand from './SceneConstantOperand.vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  depth: { type: Number, required: true },
  devices: { type: Array as PropType<DevicesWithSensorsResponse>, required: true },
});
const emit = defineEmits(['remove']);
const rule = defineModel<RulesLogic>({ required: true });

const compareOptions = computed(() => {
  return [
    { label: 'is greater than', value: '>' },
    { label: 'is less than', value: '<' },
    { label: 'is greater than or equal to', value: '>=' },
    { label: 'is less than or equal to', value: '<=' },
    { label: 'is equal to', value: '==' },
    { label: 'is not equal to', value: '!=' },
  ];
});

const leftCompare = computed({
  get: () => {
    const [operator, [left]] = Object.entries(rule.value)[0];
    return left;
  },
  set: (value: string) => {
    const [operator, [, right]] = Object.entries(rule.value)[0];
    rule.value = { [operator]: [value, right] } as RulesLogic;
  },
});

const rightCompare = computed({
  get: () => {
    const [operator, [, right]] = Object.entries(rule.value)[0];
    return right;
  },
  set: (value: string) => {
    const [operator, [left]] = Object.entries(rule.value)[0];
    rule.value = { [operator]: [left, value] } as RulesLogic;
  },
});

const comparisonOperator = computed({
  get: () => {
    return Object.keys(rule.value)[0];
  },
  set: (value: string) => {
    const [, operands] = Object.entries(rule.value)[0];
    rule.value = { [value]: operands } as RulesLogic;
  },
});

const operatorRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

function isGroup(rule: RulesLogic) {
  return typeof rule === 'object' && ('and' in rule || 'or' in rule);
}
</script>
