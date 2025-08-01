<template>
  <q-select
    ref="dateSelect"
    class="date-picker"
    :options="timeRanges"
    :label="t('time_range.label')"
    option-value="timeRange"
    option-label="title"
    outlined
    dense
    map-options
    :model-value="timeRanges[selectedTimeRangeIndex]"
    @update:model-value="setPredefinedTimeRange"
  >
    <template #prepend>
      <q-icon :name="mdiClockOutline" />
    </template>
    <template #after-options>
      <q-item clickable @click="customTimeRangeDialog = true">
        <q-item-section>
          <q-item-label>{{ t('time_range.custom') }}</q-item-label>
        </q-item-section>
      </q-item>
    </template>
    <template #selected-item="scope">
      <template v-if="isCustomTimeRangeSelected">
        <!-- eslint-disable-next-line @intlify/vue-i18n/no-raw-text -->
        {{ customTimeRangeSelected.from }} - {{ customTimeRangeSelected.to }}
      </template>
      <template v-else-if="scope.opt.name">
        {{ scope.opt.title }}
      </template>
    </template>
    <q-dialog v-model="customTimeRangeDialog">
      <q-card class="q-pa-xs">
        <q-card-section>
          <div class="text-h6">
            {{ t('time_range.custom') }}
          </div>
        </q-card-section>
        <q-card-section class="q-pt-none column q-gutter-md">
          <DateTimeInput v-model="customTimeRangeSelected.from" :label="t('time_range.from')" />
          <DateTimeInput v-model="customTimeRangeSelected.to" :label="t('time_range.to')" :show-now-button="true" />
        </q-card-section>
        <q-card-actions align="right" class="text-primary">
          <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
          <q-btn unelevated color="primary" label="Filter" no-caps padding="6px 20px" @click="setCustomTimeRange" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-select>
</template>

<script setup lang="ts">
import { format, subSeconds } from 'date-fns';
import { computed, ref, watch, onMounted } from 'vue';
import { PredefinedTimeRange } from '@/models/TimeRange';
import { useI18n } from 'vue-i18n';
import { mdiClockOutline } from '@quasar/extras/mdi-v7';
import DateTimeInput from './DateTimeInput.vue';

const emit = defineEmits(['update:modelValue', 'timeRangeChanged']);
defineExpose({
  updateTimeRange,
});

const props = defineProps({
  modelValue: {
    type: Object,
    default: undefined,
  },
  initialTimeRange: {
    type: String, // '5m', '15m', etc.
  },
  initialCustomTimeRange: {
    type: Object,
    default: () => ({ from: '', to: '' }),
  },
});

const { t } = useI18n();

const customTimeRangeDialog = ref(false);

const timeRanges = computed(() => [
  {
    title: t('time_range.predefined.last_5min'),
    name: '5m',
    time: 300,
  },

  {
    title: t('time_range.predefined.last_15min'),
    name: '15m',
    time: 900,
  },
  {
    title: t('time_range.predefined.last_30min'),
    name: '30m',
    time: 1800,
  },
  {
    title: t('time_range.predefined.last_1h'),
    name: '1h',
    time: 3600,
  },
  {
    title: t('time_range.predefined.last_6h'),
    name: '6h',
    time: 21600,
  },
  {
    title: t('time_range.predefined.last_12h'),
    name: '12h',
    time: 43200,
  },
  {
    title: t('time_range.predefined.last_24h'),
    name: '24h',
    time: 86400,
  },
  {
    title: t('time_range.predefined.last_week'),
    name: '1w',
    time: 604800,
  },
  {
    title: t('time_range.predefined.last_month'),
    name: '1m',
    time: 2592000,
  },
]);

const selectedTimeRangeIndex = ref(4);

onMounted(() => {
  if (props.initialCustomTimeRange && props.initialCustomTimeRange.from && props.initialCustomTimeRange.to) {
    customTimeRangeSelected.value = { ...props.initialCustomTimeRange };
    isCustomTimeRangeSelected.value = true;
  } else if (props.initialTimeRange) {
    const idx = timeRanges.value.findIndex((r) => r.name === props.initialTimeRange);
    if (idx !== -1) {
      selectedTimeRangeIndex.value = idx;
      isCustomTimeRangeSelected.value = false;
    }
  }
  updateTimeRange();
});

const customTimeRangeSelected = ref<{
  from: string;
  to: string;
}>({
  from: '',
  to: '',
});

const isCustomTimeRangeSelected = ref(false);

function setCustomTimeRange() {
  isCustomTimeRangeSelected.value = true;
  customTimeRangeDialog.value = false;
  updateTimeRange();
}

function setPredefinedTimeRange(val: PredefinedTimeRange) {
  selectedTimeRangeIndex.value = timeRanges.value.findIndex((r) => r.name === val.name);
  isCustomTimeRangeSelected.value = false;
  updateTimeRange();
}

const formatDate = (date: Date) => format(date, 'yyyy-MM-dd HH:mm:ss');

function updateTimeRange() {
  let newVal;
  let timeRangeName;
  let customRangeData = null;

  if (isCustomTimeRangeSelected.value) {
    if (customTimeRangeSelected.value.from === null) {
      return;
    }

    newVal = {
      from: formatDate(new Date(customTimeRangeSelected.value.from)),
      to: formatDate(new Date(customTimeRangeSelected.value.to ?? new Date())),
    };
    timeRangeName = 'custom';
    customRangeData = {
      from: customTimeRangeSelected.value.from,
      to: customTimeRangeSelected.value.to,
    };
  } else {
    const now = new Date();
    newVal = {
      from: formatDate(subSeconds(now, timeRanges.value[selectedTimeRangeIndex.value].time)),
      to: formatDate(now),
    };
    timeRangeName = timeRanges.value[selectedTimeRangeIndex.value].name;
  }
  emit('update:modelValue', newVal);
  emit('timeRangeChanged', timeRangeName, customRangeData);
}
</script>

<style lang="scss" scoped></style>
