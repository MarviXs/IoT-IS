<template>
  <div class="q-pa-md">
    <q-input v-model="date" :label="label">
      <template #append>
        <q-btn
          v-if="showNowButton"
          flat
          round
          dense
          no-caps
          :label="t('time_range.now')"
          color="grey-8"
          @click="setCurrentDate"
        />
        <q-icon :name="mdiCalendar" class="cursor-pointer">
          <q-popup-proxy cover transition-show="scale" transition-hide="scale">
            <q-date v-model="date" mask="YYYY-MM-DD HH:mm:ss">
              <div class="row items-center justify-end">
                <q-btn v-close-popup label="Close" color="primary" flat />
              </div>
            </q-date>
          </q-popup-proxy>
        </q-icon>
        <q-icon :name="mdiClock" class="cursor-pointer">
          <q-popup-proxy cover transition-show="scale" transition-hide="scale">
            <q-time v-model="date" mask="YYYY-MM-DD HH:mm:ss" format24h with-seconds>
              <div class="row items-center justify-end">
                <q-btn v-close-popup label="Close" color="primary" flat />
              </div>
            </q-time>
          </q-popup-proxy>
        </q-icon>
      </template>
    </q-input>
  </div>
</template>

<script setup lang="ts">
import { mdiCalendar, mdiClock } from '@quasar/extras/mdi-v6';
import { format } from 'date-fns';
import { useI18n } from 'vue-i18n';

const date = defineModel({ type: String, required: false });

const { t } = useI18n();

defineProps({
  label: {
    type: String,
    default: '',
  },
  showNowButton: {
    type: Boolean,
    default: false,
  },
});

function setCurrentDate() {
  date.value = format(new Date(), 'yyyy-mm-dd HH:mm:ss');
}
</script>
