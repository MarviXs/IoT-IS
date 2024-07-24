<template>
  <div>
    <q-input v-model="module.name" autofocus :label="t('global.name')" />
  </div>
</template>

<script setup lang="ts">
import { ModuleInput } from '@/models/Module';
import { useI18n } from 'vue-i18n';

const module = defineModel<ModuleInput>({ required: true });

const { t } = useI18n();
</script>
