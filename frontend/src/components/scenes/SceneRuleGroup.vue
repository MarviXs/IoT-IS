<template>
  <div class="rule-group" :class="{ 'root-group': isRoot, 'sub-group': !isRoot }">
    <div class="row gap-md">
      <q-btn v-if="!isRoot" :icon="mdiCloseCircleOutline" color="red" round dense flat @click="emit('remove')">
        <q-tooltip>Remove Group</q-tooltip>
      </q-btn>
      <q-btn label="Add Rule" color="primary" unelevated @click="addRule" />
      <q-btn label="Add Group" color="primary" unelevated @click="addGroup" />
    </div>
    <q-select v-model="condition" :options="conditionOptions" outlined class="q-mt-md" emit-value map-options />
    <div class="q-mt-md">
      <SceneRule
        v-for="(rule, index) in getRules()"
        :key="index"
        :depth="depth + 1"
        v-model="getRules()[index]"
        :devices="devices"
        @remove="removeRule(getRules()[index])"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ReservedOperations, RulesLogic } from 'json-logic-js';
import { computed, PropType, ref } from 'vue';
import SceneRule from './SceneRule.vue';
import { mdiCloseCircleOutline } from '@quasar/extras/mdi-v7';
import { getRuleColor } from '@/utils/rule-colors';
import { DevicesWithSensorsResponse } from '@/api/services/DeviceService';

const props = defineProps({
  isRoot: { type: Boolean, required: true },
  depth: { type: Number, required: true },
  devices: { type: Array as PropType<DevicesWithSensorsResponse>, required: true },
});
const emit = defineEmits(['remove']);

const rules = defineModel<RulesLogic>({ required: true });

const conditionOptions = ref([
  { label: 'When all conditions are met', value: 'and' as ReservedOperations },
  { label: 'When any condition is met', value: 'or' as ReservedOperations },
]);

const condition = computed({
  get: () => {
    const rulesObj = rules.value as Record<string, unknown>;
    return rulesObj && 'or' in rulesObj ? 'or' : 'and';
  },
  set: (value: ReservedOperations) => {
    const currentRules = (rules.value as Record<string, unknown>)[condition.value] || [];
    rules.value = { [value]: currentRules } as RulesLogic;
  },
});

function addRule() {
  if (!rules.value) {
    rules.value = { and: [] };
  }
  const currentRules = getRules();
  currentRules.push({ '>': ['', ''] } as RulesLogic);
  rules.value = { [condition.value]: currentRules } as RulesLogic;
}

function removeRule(rule: RulesLogic) {
  const currentRules = getRules();
  currentRules.splice(currentRules.indexOf(rule), 1);
  rules.value = { [condition.value]: currentRules } as RulesLogic;
}

function addGroup() {
  if (!rules.value) {
    rules.value = { and: [] };
  }
  const currentRules = getRules();
  currentRules.push({ and: [] } as RulesLogic);
  rules.value = { [condition.value]: currentRules } as RulesLogic;
}

function getRules(): RulesLogic[] {
  const rulesValue = rules.value as Record<ReservedOperations, RulesLogic[]>;
  const firstKey = Object.keys(rulesValue)[0] as ReservedOperations;
  return rulesValue[firstKey] || [];
}

const getColor = computed(() => getRuleColor(props.depth));
</script>

<style scoped lang="scss">
.gap-md {
  gap: 10px;
}

.sub-group {
  border: 1px solid #b5b5b5;
  border-radius: 3px;
  padding: 20px;
  margin-top: 10px;
  border-left: 3px solid v-bind(getColor);
}

.rule-group {
}
</style>
