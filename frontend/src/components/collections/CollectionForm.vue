<template>
  <q-form @submit="onSubmit" greedy>
    <q-card-section class="q-pt-none column q-gutter-md">
      <q-input v-model="collection.name" autofocus :label="t('global.name')" />
    </q-card-section>
    <q-card-actions align="right" class="text-primary">
      <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
      <q-btn
        unelevated
        color="primary"
        :label="t('global.save')"
        no-caps
        padding="6px 20px"
        type="submit"
        :loading="props.isLoading"
      />
    </q-card-actions>
  </q-form>
</template>

<script setup lang="ts">
import { UpdateCollectionRequest } from '@/api/services/DeviceCollectionService';
import { useI18n } from 'vue-i18n';

const props = defineProps({
  isLoading: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['onSubmit']);
const collection = defineModel<UpdateCollectionRequest>({ required: true });

const { t } = useI18n();
function onSubmit() {
  emit('onSubmit', collection.value);
}
</script>
