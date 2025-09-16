<template>
  <div>
    <div v-if="isGroup(rule)">
      <SceneRuleGroup
        class="q-mb-md"
        v-model="rule"
        :is-root="false"
        :depth="depth"
        :devices="devices"
        @remove="emit('remove')"
      />
    </div>
    <div class="row items-center" v-else>
      <q-btn
        :icon="mdiCloseCircleOutline"
        color="red"
        round
        dense
        flat
        class="q-mb-lg q-mr-sm"
        @click="emit('remove')"
      />
      <SceneDeviceOperand v-model="leftCompare" :devices="devices" />
      <q-select
        v-model="comparisonOperator"
        :options="compareOptions"
        label="Operator"
        outlined
        :rules="operatorRules"
        class="q-mr-sm"
        emit-value
        map-options
      />
      <SceneConstantOperand v-model="rightCompare" :unit="getUnit(leftCompare)" />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { RulesLogic } from 'json-logic-js';
import SceneRuleGroup from './SceneRuleGroup.vue';
import { mdiCloseCircleOutline } from '@quasar/extras/mdi-v7';
import type { SceneDevice } from '@/api/services/DeviceService';
import type { PropType } from 'vue';
import { computed } from 'vue';
import SceneDeviceOperand from './SceneDeviceOperand.vue';
import SceneConstantOperand from './SceneConstantOperand.vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
  depth: { type: Number, required: true },
  devices: { type: Array as PropType<SceneDevice>, required: true },
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

function getUnit(compare: any) {
  if (!compare || !compare.var) {
    return '';
  }
  const deviceId = compare.var.split('.')[1];
  const tag = compare.var.split('.')[2];
  const device = props.devices.find((device) => device.id === deviceId);
  const tagInfo = device?.sensors.find((tagInfo) => tagInfo.tag === tag);
  return tagInfo?.unit;
}

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
