<template>
  <q-input
    v-model="constant"
    label="Value"
    outlined
    class="q-mr-sm input-value"
    type="number"
    :rules="constantRules"
    :suffix="unit"
  />
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

defineProps({
  unit: { type: String, required: false },
});

const constant = defineModel<string>({ required: true });

const constantRules = [
  (val: string) => (val && val.length > 0) || t('global.rules.required'),
  (val: string) => !isNaN(Number(val)) || t('global.rules.number'),
];
</script>
