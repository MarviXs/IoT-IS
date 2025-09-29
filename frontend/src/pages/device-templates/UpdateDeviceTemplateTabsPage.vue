<template>
  <page-layout
    :breadcrumbs="[
      { label: t('device_template.label', 2), to: '/device-templates' },
      { label: deviceRequest?.name, to: `/device-templates/${templateId}` },
    ]"
  >
    <q-card class="shadow q-mb-lg">
      <q-tabs dense class="text-grey" active-color="primary" indicator-color="primary" align="left">
        <q-route-tab
          :to="{ path: `/device-templates/${templateId}` }"
          style="min-width: 130px"
          name="edit"
          :icon="mdiContentCopy"
          :label="t('device_template.template_details')"
          no-caps
        />
        <q-route-tab
          :to="{ path: `/device-templates/${templateId}/commands` }"
          style="min-width: 130px"
          name="commands"
          :icon="mdiCodeTags"
          :label="t('command.label', 2)"
          no-caps
        />
        <q-route-tab
          :to="{ path: `/device-templates/${templateId}/recipes` }"
          style="min-width: 130px"
          name="recipes"
          :icon="mdiBookMultipleOutline"
          :label="t('recipe.label', 2)"
          no-caps
        />
        <q-route-tab
          :to="{ path: `/device-templates/${templateId}/controls` }"
          style="min-width: 130px"
          name="controls"
          :icon="mdiGestureTapButton"
          :label="t('device_template.controls')"
          no-caps
        />
        <q-route-tab
          :to="{ path: `/device-templates/${templateId}/grid` }"
          style="min-width: 130px"
          name="grid"
          :icon="mdiViewGridOutline"
          :label="t('device_template.grid_settings')"
          no-caps
        />
      </q-tabs>
    </q-card>
    <router-view />
  </page-layout>
</template>

<script setup lang="ts">
import PageLayout from '@/layouts/PageLayout.vue';
import { useI18n } from 'vue-i18n';
import { ref } from 'vue';
import type { UpdateDeviceTemplateRequest } from '@/api/services/DeviceTemplateService';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';
import { useRoute } from 'vue-router';
import { handleError } from '@/utils/error-handler';
import {
  mdiCodeTags,
  mdiContentCopy,
  mdiBookMultipleOutline,
  mdiGestureTapButton,
  mdiViewGridOutline,
} from '@quasar/extras/mdi-v7';

const { t } = useI18n();
const route = useRoute();

const deviceRequest = ref<UpdateDeviceTemplateRequest>();

const templateId = route.params.id as string;

async function getDeviceTemplate() {
  const { data, error } = await DeviceTemplateService.getDeviceTemplate(templateId);
  if (error) {
    handleError(error, 'Error fetching device template');
    return;
  }
  deviceRequest.value = {
    name: data.name,
    deviceType: data.deviceType,
  };
}
getDeviceTemplate();
</script>

<style lang="scss" scoped></style>
