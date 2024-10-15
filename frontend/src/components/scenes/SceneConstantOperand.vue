<template>
  <q-input v-model="constant" label="Value" outlined class="q-ma-xs" :rules="constantRules" type="number" />
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const constant = defineModel<number>({ required: true });

const constantRules = [
  (val: string) => (val && val.length > 0) || t('global.rules.required'),
  (val: string) => !isNaN(Number(val)) || t('global.rules.number'),
];
</script>
