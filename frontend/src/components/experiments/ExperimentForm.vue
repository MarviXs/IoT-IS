<template>
  <q-form ref="experimentForm" @submit="onSubmit">
    <q-card-section class="q-pt-none column q-gutter-md">
      <q-input v-model="form.note" :label="t('experiment.note')" type="textarea" autogrow :rows="3" clearable />

      <DeviceSelect v-model="form.device" :rules="deviceRules" />
      <template v-if="isCreateMode">
        <q-btn-toggle
          v-model="form.mode"
          :options="experimentTypeOptions"
          toggle-color="primary"
          color="grey-3"
          text-color="dark"
          no-caps
          unelevated
          spread
        />
      </template>

      <template v-if="isCreateMode && form.mode === 'automatic'">
        <RecipeSelect
          v-model="form.recipe"
          :device-id="form.device?.id"
          :disable="!form.device?.id"
          :rules="recipeRules"
          clearable
        />
      </template>

      <template v-if="!isCreateMode">
        <q-select
          v-model="form.ranJob"
          :label="t('experiment.job')"
          :options="ranJobOptions"
          :loading="loadingRanJobs"
          :disable="!form.device?.id"
          option-label="name"
          option-value="id"
          use-input
          clearable
          :input-debounce="400"
          @filter="filterRanJobs"
          @virtual-scroll="getRanJobsOnScroll"
        />
      </template>

      <template v-if="isCreateMode && form.mode === 'automatic'">
        <q-input
          v-model.number="form.cycles"
          type="number"
          min="1"
          :label="t('experiment.cycles')"
          :rules="cyclesRules"
        />
      </template>

      <template v-if="!isCreateMode || form.mode === 'manual'">
        <q-input v-model="form.startedAt" :label="t('experiment.started_at')" :rules="manualStartedAtRules">
          <template #append>
            <q-icon :name="mdiCalendar" class="cursor-pointer">
              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                <q-date v-model="form.startedAt" :mask="DATE_TIME_MASK">
                  <div class="row items-center justify-end">
                    <q-btn v-close-popup :label="t('global.close')" color="primary" flat no-caps />
                  </div>
                </q-date>
              </q-popup-proxy>
            </q-icon>
            <q-icon :name="mdiClock" class="cursor-pointer">
              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                <q-time v-model="form.startedAt" :mask="DATE_TIME_MASK" format24h>
                  <div class="row items-center justify-end">
                    <q-btn v-close-popup :label="t('global.close')" color="primary" flat no-caps />
                  </div>
                </q-time>
              </q-popup-proxy>
            </q-icon>
          </template>
        </q-input>

        <q-input v-model="form.finishedAt" :label="t('experiment.finished_at')" :rules="manualFinishedAtRules">
          <template #append>
            <q-icon :name="mdiCalendar" class="cursor-pointer">
              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                <q-date v-model="form.finishedAt" :mask="DATE_TIME_MASK">
                  <div class="row items-center justify-end">
                    <q-btn v-close-popup :label="t('global.close')" color="primary" flat no-caps />
                  </div>
                </q-date>
              </q-popup-proxy>
            </q-icon>
            <q-icon :name="mdiClock" class="cursor-pointer">
              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                <q-time v-model="form.finishedAt" :mask="DATE_TIME_MASK" format24h>
                  <div class="row items-center justify-end">
                    <q-btn v-close-popup :label="t('global.close')" color="primary" flat no-caps />
                  </div>
                </q-time>
              </q-popup-proxy>
            </q-icon>
          </template>
        </q-input>
      </template>
    </q-card-section>

    <q-card-actions align="right" class="text-primary">
      <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
      <q-btn
        unelevated
        color="primary"
        :label="t('global.save')"
        type="submit"
        no-caps
        padding="6px 20px"
        :loading="props.loading"
      />
    </q-card-actions>
  </q-form>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiCalendar, mdiClock } from '@quasar/extras/mdi-v7';
import DeviceSelect from '@/components/devices/DeviceSelect.vue';
import type { DeviceSelectData } from '@/components/devices/DeviceSelect.vue';
import RecipeSelect from '@/components/recipes/RecipeSelect.vue';
import type { RecipeSelectData } from '@/components/recipes/RecipeSelect.vue';
import type { QSelect } from 'quasar';
import JobService from '@/api/services/JobService';
import type { JobsQueryParams, JobsResponse } from '@/api/services/JobService';
import { handleError } from '@/utils/error-handler';

type ExperimentMode = 'automatic' | 'manual';

export interface ExperimentFormData {
  note: string;
  device: DeviceSelectData | null;
  recipe: RecipeSelectData | null;
  ranJob: RanJobSelectData | null;
  mode: ExperimentMode;
  cycles: number;
  startedAt: string | null;
  finishedAt: string | null;
}

export interface RanJobSelectData {
  id?: string;
  name?: string;
}

const props = defineProps<{
  loading?: boolean;
  isCreateMode?: boolean;
}>();

const emit = defineEmits(['onSubmit']);
const { t } = useI18n();
const DATE_TIME_MASK = 'YYYY-MM-DDTHH:mm';

const form = defineModel<ExperimentFormData>({
  required: true,
});

const experimentForm = ref();

const experimentTypeOptions = computed(() => [
  { label: t('experiment.automatic_from_recipe'), value: 'automatic' },
  { label: t('experiment.manual_from_job'), value: 'manual' },
]);

const cyclesRules = computed(() => [
  (val: number) => {
    if (isCreateMode.value && form.value.mode === 'automatic' && (!val || val < 1)) {
      return t('device_template.rules.cycle_positive');
    }

    return true;
  },
]);

const isCreateMode = computed(() => !!props.isCreateMode);

const deviceRules = computed(() => [
  (val: DeviceSelectData | null) => {
    if (isCreateMode.value && !val?.id) {
      return t('global.rules.required');
    }
    return true;
  },
]);

const recipeRules = computed(() => [
  (val: RecipeSelectData | null) => {
    if (isCreateMode.value && form.value.mode === 'automatic' && !val?.id) {
      return t('global.rules.required');
    }
    return true;
  },
]);

const ranJobs = ref<JobsResponse['items']>([]);
const loadingRanJobs = ref(false);
const ranJobsFilter = ref('');
const ranJobsNextPage = ref(1);
const ranJobsLastPage = ref(1);

const ranJobOptions = computed(() => {
  return (
    ranJobs.value?.map((job) => ({
      id: job.id,
      name: `${job.name} (${formatJobDate(job.startedAt)} - ${formatJobDate(job.finishedAt)})`,
    })) ?? []
  );
});

function formatJobDate(value?: string | null) {
  if (!value) {
    return '-';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return '-';
  }

  return date.toLocaleString();
}

function resetRanJobs() {
  ranJobs.value = [];
  ranJobsFilter.value = '';
  ranJobsNextPage.value = 1;
  ranJobsLastPage.value = 1;
}

async function getRanJobsOnScroll({ to, ref }: { to: number; ref: QSelect | null }) {
  const deviceId = form.value.device?.id;
  if (!deviceId) {
    return;
  }

  const lastIndex = (ranJobs.value.length ?? 0) - 1;
  if (loadingRanJobs.value || ranJobsNextPage.value > ranJobsLastPage.value || lastIndex !== to) {
    return;
  }

  const queryParams: JobsQueryParams = {
    DeviceId: deviceId,
    SortBy: 'createdAt',
    Descending: true,
    SearchTerm: ranJobsFilter.value,
    PageNumber: ranJobsNextPage.value,
    PageSize: 50,
  };

  loadingRanJobs.value = true;
  const { data, error } = await JobService.getJobs(queryParams);
  loadingRanJobs.value = false;

  if (error) {
    handleError(error, 'Loading jobs failed');
    return;
  }

  if (data) {
    ranJobs.value = [...(ranJobs.value ?? []), ...(data.items ?? [])];
  }

  ranJobsNextPage.value++;
  ranJobsLastPage.value = data?.totalPages ?? 1;

  if (ref) {
    await nextTick();
    ref.refresh();
  }
}

async function filterRanJobs(
  val: string,
  doneFn: (callbackFn: () => void, afterFn?: ((ref: QSelect) => void)) => void,
) {
  if (val === ranJobsFilter.value) {
    doneFn(() => {});
    return;
  }

  ranJobsFilter.value = val;
  ranJobsNextPage.value = 1;
  ranJobsLastPage.value = 1;
  ranJobs.value = [];
  await getRanJobsOnScroll({ to: -1, ref: null });
  doneFn(() => {});
}

const manualStartedAtRules = computed(() => [
  (val: string | null) => {
    if (isCreateMode.value && form.value.mode === 'manual' && !val) {
      return t('global.rules.required');
    }
    return true;
  },
]);

const manualFinishedAtRules = computed(() => [
  (val: string | null) => {
    if (isCreateMode.value && form.value.mode === 'manual' && !val) {
      return t('global.rules.required');
    }
    return true;
  },
]);

watch(
  () => form.value.device?.id,
  async (newDeviceId, oldDeviceId) => {
    form.value.recipe = null;

    if (oldDeviceId && oldDeviceId !== newDeviceId) {
      form.value.ranJob = null;
    }

    resetRanJobs();
    if (!isCreateMode.value && newDeviceId) {
      await getRanJobsOnScroll({ to: -1, ref: null });
    }
  },
);

watch(
  () => form.value.mode,
  (mode) => {
    if (!isCreateMode.value) {
      return;
    }

    if (mode === 'automatic') {
      form.value.startedAt = null;
      form.value.finishedAt = null;
      return;
    }

    form.value.recipe = null;
  },
);

function onSubmit() {
  if (!experimentForm.value?.validate()) {
    return;
  }

  emit('onSubmit');
}
</script>
