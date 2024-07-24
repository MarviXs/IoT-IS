<template>
  <div class="data-point-container q-my-md shadow">
    <!-- <div class="row items-center justify-end">
      <q-btn flat round color="grey-color" :icon="mdiTrashCanOutline" dense @click="emit('remove')" />
    </div> -->
    <div class="row q-col-gutter-y-sm q-col-gutter-x-lg q-mt-sm">
      <q-input
        ref="dataTagNameRef"
        v-model="dataPointTag.name"
        :rules="nameRules"
        class="col-12 col-md-6 col-lg-grow"
        :label="t('global.name')"
      />
      <q-input
        ref="tagRef"
        v-model="dataPointTag.tag"
        :rules="tagRules"
        class="col-12 col-md-6 col-lg-3"
        :label="t('device.tag')"
      />
      <q-input
        ref="unitRef"
        v-model="dataPointTag.unit"
        :rules="unitRules"
        class="col-12 col-md-6 col-lg-2"
        :label="t('device.unit')"
      />
      <q-input
        ref="decimalRef"
        v-model.number="dataPointTag.accuracyDecimals"
        type="number"
        inputmode="numeric"
        :error="false"
        :rules="decimalRules"
        class="col-12 col-md-6 col-lg-2"
        :label="t('device.decimal')"
      />
      <div class="col-12 col-lg-shrink self-center flex items-center">
        <q-space></q-space>
        <q-btn flat round color="grey-color" :icon="mdiTrashCanOutline" @click="emit('remove')" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { mdiTrashCanOutline } from '@quasar/extras/mdi-v6';
import { QInput } from 'quasar';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { isFormValid } from '@/utils/form-validation';

export type SensorFormData = {
  id: string | null;
  tag: string;
  name: string;
  unit?: string | null;
  accuracyDecimals?: number | null;
};

const dataPointTag = defineModel<SensorFormData>({
  required: true,
});

const emit = defineEmits(['remove']);

const { t } = useI18n();

const dataTagNameRef = ref<QInput>();
const tagRef = ref<QInput>();
const unitRef = ref<QInput>();
const decimalRef = ref<QInput>();

function validate() {
  const refs = [dataTagNameRef.value, tagRef.value, unitRef.value, decimalRef.value];
  return isFormValid(refs);
}

const nameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const tagRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const unitRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

const decimalRules = [
  (val: string) => (val != null && isNumeric(val)) || t('global.rules.required'),
  (val: number) => val >= 0 || t('device.rules.decimal_min'),
];

const isNumeric = (value: string) => {
  return /^-?\d+$/.test(value);
};

defineExpose({
  validate,
});
</script>

<style lang="scss" scoped>
.data-point-container {
  border: 1px solid #ccc;
  border-radius: 5px;
  padding: 1rem 2.5rem 1.5rem 2.5rem;
}
</style>
@/api/types/Sensor
