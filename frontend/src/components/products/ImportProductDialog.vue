<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('product.edit_product') }}</template>
    <template #default>
      <q-file filled bottom-slots v-model="file" label="Label" counter class="col-12">
        <template v-slot:prepend>
          <q-icon :name="mdiCloudUpload" @click.stop.prevent />
        </template>
        <template v-slot:append>
          <q-icon :name="mdiClose" @click.stop.prevent="file = null" class="cursor-pointer" />
        </template>
        <template v-slot:hint> Field hint </template>
      </q-file>
      <q-btn unelevated color="primary" label="Process" no-caps />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import { mdiCloudUpload, mdiClose } from '@quasar/extras/mdi-v7';

const isDialogOpen = defineModel<boolean>();

const file = ref<File | null>(null);

const { t } = useI18n();

function processFile() {
  if (!file.value) {
    return;
  }
}
</script>
