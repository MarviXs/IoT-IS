<template>
  <div>
    <q-card class="tw-p-10 shadow">
      <q-form ref="formRef" class="q-gutter-y-md" @submit.prevent="submitForm">
        <div class="row q-col-gutter-md">
          <q-input
            v-model.number="gridForm.gridRowSpan"
            class="col-12 col-md-6"
            type="number"
            :label="t('device_template.grid_rows')"
            clearable
            :rules="gridValueRules"
            lazy-rules
            :min="1"
          />
          <q-input
            v-model.number="gridForm.gridColumnSpan"
            class="col-12 col-md-6"
            type="number"
            :label="t('device_template.grid_columns')"
            clearable
            :rules="gridValueRules"
            lazy-rules
            :min="1"
          />
        </div>
      </q-form>
    </q-card>
    <q-btn
      unelevated
      color="primary"
      :label="t('global.save')"
      padding="7px 35px"
      class="q-mt-md"
      :loading="submitting"
      :disable="isLoading"
      no-caps
      @click="submitForm"
    />
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import { QForm } from 'quasar';

import DeviceTemplateService from '@/api/services/DeviceTemplateService';
import type { DeviceTemplateResponse, UpdateDeviceTemplateRequest } from '@/api/services/DeviceTemplateService';
import { handleError } from '@/utils/error-handler';

const { t } = useI18n();
const route = useRoute();

const templateId = route.params.id as string;
const isLoading = ref(true);
const submitting = ref(false);
const formRef = ref<QForm>();

interface GridFormState {
  name: DeviceTemplateResponse['name'];
  deviceType: DeviceTemplateResponse['deviceType'];
  gridRowSpan: number | null;
  gridColumnSpan: number | null;
}

const gridForm = reactive<GridFormState>({
  name: '',
  deviceType: 'Generic' as DeviceTemplateResponse['deviceType'],
  gridRowSpan: null,
  gridColumnSpan: null,
});

const gridValueRules = [
  (val: number | null | string) => {
    if (val === null || val === '' || typeof val === 'undefined') {
      return true;
    }
    if (typeof val === 'number' && Number.isFinite(val) && val > 0) {
      return true;
    }
    return t('device_template.rules.grid_positive');
  },
];

watch(
  () => gridForm.gridRowSpan,
  (value) => {
    if (typeof value === 'number' && Number.isNaN(value)) {
      gridForm.gridRowSpan = null;
    }
  },
);

watch(
  () => gridForm.gridColumnSpan,
  (value) => {
    if (typeof value === 'number' && Number.isNaN(value)) {
      gridForm.gridColumnSpan = null;
    }
  },
);

async function fetchTemplate() {
  isLoading.value = true;
  const { data, error } = await DeviceTemplateService.getDeviceTemplate(templateId);
  isLoading.value = false;

  if (error || !data) {
    handleError(error, 'Error fetching device template');
    return;
  }

  gridForm.name = data.name;
  gridForm.deviceType = data.deviceType;
  gridForm.gridRowSpan = data.gridRowSpan ?? null;
  gridForm.gridColumnSpan = data.gridColumnSpan ?? null;
}

fetchTemplate();

async function submitForm() {
  const isValid = await formRef.value?.validate();
  if (isValid === false) {
    return;
  }

  submitting.value = true;

  const payload: UpdateDeviceTemplateRequest = {
    name: gridForm.name,
    deviceType: gridForm.deviceType,
    gridRowSpan: gridForm.gridRowSpan,
    gridColumnSpan: gridForm.gridColumnSpan,
  };

  const response = await DeviceTemplateService.updateDeviceTemplate(templateId, payload);
  submitting.value = false;

  if (response.error) {
    handleError(response.error, 'Error updating grid settings');
    return;
  }

  toast.success(t('device_template.toasts.update_success'));
}
</script>

<style scoped></style>
