<template>
  <div>
    <q-input v-model="collection.name" autofocus :label="t('global.name')" />
  </div>
</template>

<script setup lang="ts">
import { UpdateCollectionRequest } from '@/api/types/DeviceCollection';
import { useI18n } from 'vue-i18n';

const collection = defineModel<UpdateCollectionRequest>({ required: true });

const { t } = useI18n();
</script>
