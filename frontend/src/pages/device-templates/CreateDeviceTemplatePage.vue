<template>
  <PageLayout
    :breadcrumbs="[{ label: t('device_template.label', 2), to: '/device-templates' }, { label: t('global.add') }]"
    previous-title="Device Templates"
    previous-route="/device-templates"
  >
    <DeviceTemplateForm
      v-model:template="templateData"
      v-model:sensors="sensorsData"
      :loading="submitting"
      class="q-mt-md"
      @on-submit="submitForm"
    />
  </PageLayout>
</template>

<script setup lang="ts">
import PageLayout from '@/layouts/PageLayout.vue';
import type { DeviceTemplateFormData } from '@/components/device-templates/DeviceTemplateForm.vue';
import DeviceTemplateForm from '@/components/device-templates/DeviceTemplateForm.vue';
import { useI18n } from 'vue-i18n';
import { ref } from 'vue';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';
import SensorService from '@/api/services/SensorService';
import { toast } from 'vue3-toastify';
import { useRouter } from 'vue-router';
import { handleError } from '@/utils/error-handler';
import type { SensorFormData } from '@/components/device-templates/SensorForm.vue';

const { t } = useI18n();
const router = useRouter();

const templateData = ref<DeviceTemplateFormData>({
  name: '',
  deviceType: 'Generic',
  isGlobal: false,
  enableMap: false,
  enableGrid: false,
  gridRowSpan: null,
  gridColumnSpan: null,
});
const sensorsData = ref<SensorFormData[]>([]);
const submitting = ref(false);

async function submitForm() {
  submitting.value = true;

  const templateRes = await DeviceTemplateService.createDeviceTemplate(templateData.value);
  if (templateRes.error) {
    handleError(templateRes.error, 'Error creating device template');
    submitting.value = false;
    return;
  }

  const sensorRes = await SensorService.updateTemplateSensors(templateRes.data, sensorsData.value);
  submitting.value = false;

  if (sensorRes.error) {
    handleError(sensorRes.error, 'Error updating sensors');
    return;
  }

  toast.success('Device template created successfully');
  router.push(`/device-templates/${templateRes.data}`);
}
</script>

<style lang="scss" scoped></style>
