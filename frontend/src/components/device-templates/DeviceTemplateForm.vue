<template>
  <q-card class="q-pa-sm shadow">
    <q-stepper ref="stepper" v-model="formStep" animated vertical header-nav keep-alive>
      <q-step :name="1" :title="t('device_template.template_details')" :icon="mdiPencil">
        <q-form class="q-py-md q-px-sm">
          <div class="row q-col-gutter-y-sm q-col-gutter-x-lg">
            <q-input
              ref="nameRef"
              v-model="template.name"
              :rules="nameRules"
              class="col-12"
              :label="t('global.name')"
            />
            <q-select
              v-model="template.deviceType"
              :options="[
                { label: 'Generic', value: 'Generic' },
                { label: 'NuviaMSU', value: 'NuviaMSU' },
              ]"
              class="col-12"
              label="Device Type"
              :rules="[(val) => (val && val.length > 0) || t('global.rules.required')]"
              emit-value
              map-options
            />
            <div v-if="authStore.isAdmin" class="col-12">
              <q-toggle v-model="template.isGlobal" :label="t('device_template.global_template')" />
              <div class="text-caption text-grey-7 q-ml-lg">
                {{ t('device_template.global_template_hint') }}
              </div>
            </div>
            <div class="col-12">
              <q-toggle v-model="template.enableMap" :label="t('device_template.enable_map')" />
            </div>
            <div class="col-12">
              <q-toggle v-model="template.enableGrid" :label="t('device_template.enable_grid')" />
            </div>

            <q-input
              v-if="template.enableGrid"
              v-model.number="template.gridRowSpan"
              type="number"
              min="1"
              step="1"
              class="col-12 col-sm-6"
              :rules="gridRowRules"
              :label="t('device_template.grid_rows')"
            />
            <q-input
              v-if="template.enableGrid"
              v-model.number="template.gridColumnSpan"
              type="number"
              min="1"
              step="1"
              class="col-12 col-sm-6"
              :rules="gridColumnRules"
              :label="t('device_template.grid_columns')"
            />
          </div>
          <q-card-actions align="left" class="text-primary q-mt-sm q-px-none">
            <q-btn
              unelevated
              color="primary"
              :label="t('global.next')"
              padding="7px 40px"
              outline
              no-caps
              type="submit"
              @click.prevent="goToStep(2)"
            />
          </q-card-actions>
        </q-form>
      </q-step>
      <q-step :name="2" :title="t('device.add_sensors')" :icon="matSensors">
        <q-form>
          <div v-for="(sensor, index) in sensors" :key="index">
            <sensor-form ref="sensorFormRef" v-model="sensors[index]" @remove="deleteSensor(index)" />
          </div>
          <q-btn
            class="full-width q-mb-md q-mt-sm"
            outline
            :icon="mdiPlusCircle"
            color="primary"
            no-caps
            padding="12px 30px"
            :label="t('device.add_sensor')"
            @click="addSensor"
          />
        </q-form>
      </q-step>
    </q-stepper>
  </q-card>
  <q-btn
    unelevated
    color="primary"
    :label="t('global.save')"
    padding="7px 35px"
    class="q-mt-md"
    :loading="loading"
    no-caps
    type="submit"
    @click.prevent="submitForm"
  />
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { QForm, QInput } from 'quasar';
import { isFormValid } from '@/utils/form-validation';
import type { SensorFormData } from './SensorForm.vue';
import SensorForm from './SensorForm.vue';
import { mdiPencil, mdiPlusCircle } from '@quasar/extras/mdi-v7';
import { matSensors } from '@quasar/extras/material-icons';
import { useAuthStore } from '@/stores/auth-store';

export type DeviceTemplateFormData = {
  name: string;
  deviceType: 'Generic' | 'NuviaMSU';
  isGlobal: boolean;
  enableMap: boolean;
  enableGrid: boolean;
  gridRowSpan: number | null;
  gridColumnSpan: number | null;
};

const { t } = useI18n();
const emit = defineEmits(['onSubmit']);

const template = defineModel<DeviceTemplateFormData>('template', {
  required: true,
});
const sensors = defineModel<SensorFormData[]>('sensors', {
  required: true,
});

defineProps({
  loading: {
    type: Boolean,
    required: true,
  },
});

const authStore = useAuthStore();

const formStep = ref(1);

function goToStep(step: number) {
  if (formStep.value == 1) {
    const firstStepForm = [nameRef.value];
    if (!isFormValid(firstStepForm)) return;
  }
  formStep.value = step;
}

async function submitForm() {
  const firstStepForm = [nameRef.value];
  const firstStepValid = isFormValid(firstStepForm);

  const allSensorsValid = sensorFormRef.value.map((form) => form.validate()).every((isValid: boolean) => isValid);

  if (!firstStepValid) {
    formStep.value = 1;
    return;
  }

  if (!allSensorsValid) {
    formStep.value = 2;
    return;
  }

  emit('onSubmit');
}

function addSensor() {
  sensors.value.push({
    id: null,
    name: '',
    tag: '',
    unit: '',
    accuracyDecimals: 2,
  });
}

function deleteSensor(index: number) {
  sensors.value.splice(index, 1);
}

// Input validation
const nameRef = ref<QInput>();

const sensorFormRef = ref<(typeof SensorForm)[]>([]);

const nameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const gridRowRules = [
  (val: number | null) =>
    !template.value.enableGrid || (val !== null && val > 0) || t('device_template.rules.grid_positive'),
];
const gridColumnRules = [
  (val: number | null) =>
    !template.value.enableGrid || (val !== null && val > 0) || t('device_template.rules.grid_positive'),
];

watch(
  () => template.value.enableGrid,
  (enabled) => {
    if (!enabled) {
      template.value.gridRowSpan = null;
      template.value.gridColumnSpan = null;
    } else {
      template.value.gridRowSpan = template.value.gridRowSpan ?? 1;
      template.value.gridColumnSpan = template.value.gridColumnSpan ?? 1;
    }
  },
  { immediate: true },
);
</script>
