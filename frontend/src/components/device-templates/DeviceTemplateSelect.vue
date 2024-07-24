<template>
  <q-select
    v-model="selectedTemplate"
    :label="t('device_template.label')"
    :options="templateOptions"
    :loading="loadingTemplates"
    use-input
    option-label="name"
    option-value="id"
    :input-debounce="400"
    @filter="filterFn"
    @virtual-scroll="getTemplatesOnScroll"
  />
</template>

<script setup lang="ts">
import { computed, nextTick, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { DeviceTemplatesQueryParams, DeviceTemplatesResponse } from '@/api/types/DeviceTemplate';
import DeviceTemplateService from '@/api/services/DeviceTemplateService';
import { handleError } from '@/utils/error-handler';
import { QSelect } from 'quasar';

export interface DeviceTemplateSelectData {
  id?: string;
  name?: string;
}

const { t } = useI18n();

const selectedTemplate = defineModel<DeviceTemplateSelectData>({ required: false });

const deviceTemplates = ref<DeviceTemplatesResponse['items']>([]);
const loadingTemplates = ref(false);
const templateFilter = ref('');
const templateNextPage = ref(1);
const templateLastPage = ref(1);

async function getTemplatesOnScroll({ to, ref }: { to: number; ref: QSelect | null }) {
  const lastIndex = (deviceTemplates.value.length ?? 0) - 1;

  if (loadingTemplates.value || templateNextPage.value > templateLastPage.value || lastIndex != to) return;

  const paginationQuery: DeviceTemplatesQueryParams = {
    SortBy: 'name',
    Descending: false,
    SearchTerm: templateFilter.value,
    PageNumber: templateNextPage.value,
    PageSize: 50,
  };

  loadingTemplates.value = true;
  const { data, error } = await DeviceTemplateService.getDeviceTemplates(paginationQuery);
  loadingTemplates.value = false;

  if (error) {
    handleError(error, 'Loading templates failed');
    return;
  }

  if (data) {
    deviceTemplates.value = [...(deviceTemplates.value ?? []), ...(data.items ?? [])];
  }

  templateNextPage.value++;
  templateLastPage.value = data.totalPages ?? 1;

  if (ref) {
    await nextTick();
    ref.refresh();
  }
}
getTemplatesOnScroll({ to: -1, ref: null });

async function filterFn(
  val: string,
  doneFn: (callbackFn: () => void, afterFn?: ((ref: QSelect) => void) | undefined) => void,
) {
  if (val === templateFilter.value) {
    doneFn(() => {
      console.log('doneFn');
    });
    return;
  }

  templateFilter.value = val;
  templateNextPage.value = 1;
  templateLastPage.value = 1;
  deviceTemplates.value = [];
  await getTemplatesOnScroll({ to: -1, ref: null });
  doneFn(() => {
    console.log('doneFn');
  });
}

const templateOptions = computed(() => {
  return deviceTemplates.value?.map((template) => ({
    name: template.name,
    id: template.id,
  }));
});
</script>
