<template>
  <q-dialog v-model="isDialogOpen">
    <q-card style="min-width: 420px; max-width: 640px; width: 100%">
      <q-card-section class="row items-center justify-between">
        <div class="text-h6">Changelog</div>
        <q-btn flat round dense :icon="mdiClose" v-close-popup :aria-label="t('global.close')" />
      </q-card-section>
      <q-separator />
      <q-card-section class="column q-gutter-sm">
        <div class="text-subtitle1">v1.1</div>
        <ul class="q-pl-md q-my-none">
          <li>Added experiments</li>
          <li>Fixed disappearing heatmap issue</li>
        </ul>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat color="primary" :label="t('global.close')" v-close-popup />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { mdiClose } from '@quasar/extras/mdi-v7';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const isDialogOpen = defineModel<boolean>({
  required: true,
});
</script>
