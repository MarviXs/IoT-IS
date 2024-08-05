<template>
  <div>
    <q-input v-model="collection.name" autofocus :label="t('global.name')" />
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

export interface CollectionData {
  name: string;
}

const collection = defineModel<CollectionData>({ required: true });

const { t } = useI18n();
</script>
