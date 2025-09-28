<template>
  <q-btn-group unelevated class="shadow col-grow col-md-auto">
    <q-btn unelevated no-caps class="col-grow col-md-auto bg-white" :loading="loading" @click="emit('onRefresh')">
      <q-icon :name="mdiRefresh" class="text-btn-grey" />
    </q-btn>
    <q-btn color="accent" unelevated :icon="mdiCog" @click="refreshDialogOpen = true" />
  </q-btn-group>
  <dialog-common v-model="refreshDialogOpen">
    <template #title>{{ t('global.automatic_refresh_interval') }}</template>
    <template #default>
      <q-form @submit="refreshDialogOpen = false">
        <q-card-section class="q-pt-none column q-gutter-md">
          <q-input
            v-model.number="refreshInterval"
            type="number"
            :min="0"
            :label="t('global.automatic_refresh_interval')"
            :hint="t('global.enter_refresh_interval_s')"
          />
        </q-card-section>
        <q-card-actions align="right" class="text-primary">
          <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
          <q-btn unelevated color="primary" :label="t('global.save')" type="submit" no-caps padding="6px 20px" />
        </q-card-actions>
      </q-form>
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { mdiCog, mdiRefresh } from '@quasar/extras/mdi-v7';
import { useI18n } from 'vue-i18n';
import DialogCommon from './DialogCommon.vue';
import { useInterval } from '@/composables/useInterval';

const { t } = useI18n();

defineProps({
  loading: {
    type: Boolean,
    default: false,
  },
});

const refreshInterval = defineModel({ type: Number, required: true });
const emit = defineEmits(['onRefresh']);
const refreshDialogOpen = ref(false);

const intervalMilliseconds = computed(() => refreshInterval.value * 1000);
useInterval(() => {
  emit('onRefresh');
}, intervalMilliseconds);
</script>
