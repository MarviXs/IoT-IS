<template>
  <div>
    <q-input v-model="collection.name" autofocus :label="t('global.name')" />
  </div>
</template>

<script setup lang="ts">
import { CollectionInput } from '@/models/Collection';
import { useI18n } from 'vue-i18n';

const collection = defineModel<CollectionInput>({ required: true });

const { t } = useI18n();
</script>
