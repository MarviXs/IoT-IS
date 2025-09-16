<template>
  <DeviceTemplateForm
    v-if="templateData && sensorsData"
    v-model:template="templateData"
    v-model:sensors="sensorsData"
    :loading="submitting"
    @on-submit="submitForm"
  />
</template>

<script setup lang="ts">
import type { DeviceTemplateFormData } from '@/components/device-templates/DeviceTemplateForm.vue';
import DeviceTemplateForm from '@/components/device-templates/DeviceTemplateForm.vue';
import { useI18n } from 'vue-i18n';
import { ref } from 'vue';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';
import SensorService from '@/api/services/SensorService';
import { toast } from 'vue3-toastify';
import { useRoute, useRouter } from 'vue-router';
import { handleError } from '@/utils/error-handler';
import type { SensorFormData } from '@/components/device-templates/SensorForm.vue';

const { t } = useI18n();
const router = useRouter();
const route = useRoute();

const templateData = ref<DeviceTemplateFormData>();
const sensorsData = ref<SensorFormData[]>([]);
const submitting = ref(false);

const templateId = route.params.id as string;

async function getDeviceTemplate() {
  const { data, error } = await DeviceTemplateService.getDeviceTemplate(templateId);
  if (error) {
    handleError(error, 'Error fetching device template');
    return;
  }

  templateData.value = {
    name: data.name,
    deviceType: data.deviceType,
  };
}
getDeviceTemplate();

async function getDeviceSensors() {
  const { data, error } = await SensorService.getTemplateSensors(templateId);
  if (error) {
    handleError(error, 'Error fetching sensors');
    return;
  }
  sensorsData.value = data.map((sensor) => ({
    id: sensor.id,
    tag: sensor.tag,
    name: sensor.name,
    unit: sensor.unit,
    accuracyDecimals: sensor.accuracyDecimals,
    group: sensor.group,
  }));
}
getDeviceSensors();

async function submitForm() {
  if (!templateData.value || !sensorsData.value) {
    console.error('Invalid form data');
    return;
  }

  submitting.value = true;

  const templateRes = await DeviceTemplateService.updateDeviceTemplate(templateId, templateData.value);
  if (templateRes.error) {
    handleError(templateRes.error, 'Error updating device template');
    return;
  }

  const sensorRes = await SensorService.updateTemplateSensors(templateId, sensorsData.value);
  submitting.value = false;

  if (sensorRes.error) {
    handleError(sensorRes.error, 'Error updating sensors');
    return;
  }

  toast.success('Device template updated successfully');
  router.push('/device-templates');
}
</script>

<style lang="scss" scoped></style>
