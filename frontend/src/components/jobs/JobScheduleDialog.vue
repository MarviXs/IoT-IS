<template>
  <DialogCommon v-model="isDialogOpen" min-width="620px">
    <template #title>{{ dialogTitle }}</template>
    <q-form ref="formRef" @submit="submitForm">
      <q-card-section class="column q-gutter-md">
        <q-input v-model="form.name" :label="t('job_schedule.name')" :rules="nameRules" lazy-rules />

        <div>
          <RecipeSelect v-model="selectedRecipe" :device-id="deviceId" />
          <div v-if="recipeError" class="text-negative text-caption q-mt-xs">
            {{ recipeError }}
          </div>
        </div>

        <div class="column">
          <div>{{ t('job_schedule.type_label') }}</div>
          <q-btn-toggle
            v-model="form.type"
            unelevated
            class="toggle-btn q-mt-sm"
            no-caps
            text-color="primary"
            toggle-color="primary"
            spread
            :options="typeOptions"
          />
        </div>

        <div v-if="form.type === 'Repeat'" class="column">
          <div>
            <div>{{ t('job_schedule.interval') }}</div>
            <q-btn-toggle
              v-model="form.interval"
              unelevated
              no-caps
              class="toggle-btn q-mt-sm"
              text-color="primary"
              toggle-color="primary"
              spread
              :options="intervalOptions"
            />
          </div>

          <q-input
            v-if="form.interval !== 'Week'"
            v-model.number="form.intervalValue"
            type="number"
            class="q-mt-sm"
            :label="t('job_schedule.interval_value')"
            :rules="intervalValueRules"
            lazy-rules
            min="1"
          />

          <div v-if="form.interval === 'Week'" class="column q-mt-md">
            <div>{{ t('job_schedule.week_days') }}</div>
            <q-btn-group unelevated class="toggle-btn q-mt-sm" spread>
              <q-btn
                v-for="option in weekDayOptions"
                :key="option.value"
                no-caps
                dense
                class="week-day-btn"
                :label="option.label"
                :color="isWeekDaySelected(option.value) ? 'primary' : 'white'"
                :text-color="isWeekDaySelected(option.value) ? 'white' : 'primary'"
                :flat="!isWeekDaySelected(option.value)"
                @click="toggleWeekDay(option.value)"
              />
            </q-btn-group>
            <div v-if="weekDayError" class="text-negative text-caption q-mt-xs">
              {{ weekDayError }}
            </div>
          </div>
        </div>

        <q-input
          v-model="form.startTime"
          type="datetime-local"
          :label="t('job_schedule.start_time')"
          :rules="startTimeRules"
          lazy-rules
        />
        <q-input v-model="form.endTime" type="datetime-local" :label="t('job_schedule.end_time')" />

        <q-input
          v-model.number="form.cycles"
          type="number"
          :label="t('job_schedule.cycles')"
          :rules="cycleRules"
          lazy-rules
          min="1"
        />
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
          :loading="isSubmitting"
        />
      </q-card-actions>
    </q-form>
  </DialogCommon>
</template>

<script setup lang="ts">
import { computed, ref, watch, type PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import DialogCommon from '@/components/core/DialogCommon.vue';
import RecipeSelect from '@/components/recipes/RecipeSelect.vue';
import type { RecipeSelectData } from '@/components/recipes/RecipeSelect.vue';
import JobScheduleService, {
  type CreateJobScheduleRequest,
  type UpdateJobScheduleRequest,
} from '@/api/services/JobScheduleService';
import { handleError } from '@/utils/error-handler';
import type {
  JobScheduleInterval,
  JobScheduleType,
  JobScheduleWeekDay,
  JobScheduleWithRecipe,
} from '@/models/JobSchedule';

const props = defineProps({
  deviceId: { type: String, required: true },
  schedule: { type: Object as PropType<JobScheduleWithRecipe | null>, default: null },
});
const emit = defineEmits(['saved']);

const { t } = useI18n();

const isDialogOpen = defineModel<boolean>({ default: false });
const formRef = ref();

interface JobScheduleForm {
  name: string;
  type: JobScheduleType;
  interval: JobScheduleInterval;
  intervalValue: number;
  daysOfWeek: JobScheduleWeekDay[];
  startTime: string;
  endTime: string;
  cycles: number;
  isActive: boolean;
}

function createDefaultForm(): JobScheduleForm {
  return {
    name: '',
    type: 'Once',
    interval: 'Minute',
    intervalValue: 1,
    daysOfWeek: [],
    startTime: toInputDateTime(new Date().toISOString()),
    endTime: '',
    cycles: 1,
    isActive: true,
  };
}

const form = ref<JobScheduleForm>(createDefaultForm());
const selectedRecipe = ref<RecipeSelectData>();
const recipeError = ref('');
const weekDayError = ref('');
const isSubmitting = ref(false);

const dialogTitle = computed(() => (props.schedule ? t('job_schedule.edit_title') : t('job_schedule.create_title')));

const typeOptions = computed(() => [
  { label: t('job_schedule.types.once'), value: 'Once' as JobScheduleType },
  { label: t('job_schedule.types.repeat'), value: 'Repeat' as JobScheduleType },
]);

const intervalOptions = computed(() => [
  { label: t('job_schedule.interval_options.second'), value: 'Second' as JobScheduleInterval },
  { label: t('job_schedule.interval_options.minute'), value: 'Minute' as JobScheduleInterval },
  { label: t('job_schedule.interval_options.hour'), value: 'Hour' as JobScheduleInterval },
  { label: t('job_schedule.interval_options.day'), value: 'Day' as JobScheduleInterval },
  { label: t('job_schedule.interval_options.week'), value: 'Week' as JobScheduleInterval },
]);

const weekDayOptions = computed(() =>
  (['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'] as JobScheduleWeekDay[]).map(
    (day) => ({
      label: t(`job_schedule.days.${day.toLowerCase()}`),
      value: day,
    }),
  ),
);

function isWeekDaySelected(day: JobScheduleWeekDay) {
  return form.value.daysOfWeek.includes(day);
}

function toggleWeekDay(day: JobScheduleWeekDay) {
  if (isWeekDaySelected(day)) {
    form.value.daysOfWeek = form.value.daysOfWeek.filter((selectedDay) => selectedDay !== day);
  } else {
    form.value.daysOfWeek = [...form.value.daysOfWeek, day];
  }
}

const nameRules = [(val: string) => !!val || t('job_schedule.rules.name_required')];
const startTimeRules = [(val: string) => !!val || t('job_schedule.rules.start_time_required')];
const cycleRules = [(val: number) => val > 0 || t('job_schedule.rules.cycles_min')];
const intervalValueRules = [
  (val: number) =>
    form.value.type !== 'Repeat' ||
    form.value.interval === 'Week' ||
    (val ?? 0) > 0 ||
    t('job_schedule.rules.interval_value_min'),
];

watch(
  () => props.schedule,
  (schedule) => {
    if (schedule) {
      fillForm(schedule);
    } else {
      resetForm();
    }
  },
  { immediate: true },
);

watch(isDialogOpen, (open) => {
  if (!open && !props.schedule) {
    resetForm();
  }
});

watch(
  () => form.value.type,
  (type) => {
    if (type === 'Once') {
      form.value.daysOfWeek = [];
    }
  },
);

watch(
  () => form.value.interval,
  (interval) => {
    if (interval !== 'Week') {
      form.value.daysOfWeek = [];
    }
    if (!form.value.intervalValue || form.value.intervalValue < 1) {
      form.value.intervalValue = 1;
    }
  },
);

watch(selectedRecipe, () => {
  if (selectedRecipe.value?.id) {
    recipeError.value = '';
  }
});

watch(
  () => form.value.daysOfWeek,
  (days) => {
    if (days.length > 0) {
      weekDayError.value = '';
    }
  },
  { deep: true },
);

function resetForm() {
  form.value = createDefaultForm();
  selectedRecipe.value = undefined;
  recipeError.value = '';
  weekDayError.value = '';
}

function fillForm(schedule: JobScheduleWithRecipe) {
  form.value = {
    name: schedule.name,
    type: schedule.type,
    interval: schedule.interval ?? 'Minute',
    intervalValue: schedule.intervalValue ?? 1,
    daysOfWeek: [...(schedule.daysOfWeek ?? [])],
    startTime: toInputDateTime(schedule.startTime),
    endTime: toInputDateTime(schedule.endTime ?? undefined),
    cycles: schedule.cycles,
    isActive: schedule.isActive,
  };
  selectedRecipe.value = { id: schedule.recipeId, name: schedule.recipeName };
  recipeError.value = '';
  weekDayError.value = '';
}

async function submitForm() {
  recipeError.value = '';
  weekDayError.value = '';

  const isValid = await formRef.value?.validate?.();
  if (!isValid) {
    return;
  }

  if (!selectedRecipe.value?.id) {
    recipeError.value = t('job_schedule.rules.recipe_required');
    return;
  }

  if (form.value.type === 'Repeat' && form.value.interval === 'Week' && form.value.daysOfWeek.length === 0) {
    weekDayError.value = t('job_schedule.rules.week_day_required');
    return;
  }

  isSubmitting.value = true;
  const baseRequest = buildRequestPayloadBase();

  let error;
  if (props.schedule) {
    const updateRequest = {
      deviceId: props.schedule.deviceId,
      ...baseRequest,
    } as UpdateJobScheduleRequest;
    ({ error } = await JobScheduleService.updateSchedule(props.schedule.id, updateRequest));
  } else {
    const createRequest = baseRequest as CreateJobScheduleRequest;
    ({ error } = await JobScheduleService.createSchedule(props.deviceId, createRequest));
  }
  isSubmitting.value = false;

  if (error) {
    handleError(error, t('job_schedule.toasts.save_failed'));
    return;
  }

  toast.success(t('job_schedule.toasts.save_success'));
  isDialogOpen.value = false;
  emit('saved');
}

function buildRequestPayloadBase() {
  const start = fromInputDateTime(form.value.startTime);
  const end = fromInputDateTime(form.value.endTime);

  const base = {
    recipeId: selectedRecipe.value?.id ?? '',
    name: form.value.name,
    type: form.value.type,
    startTime: start ?? new Date().toISOString(),
    endTime: end,
    cycles: form.value.cycles,
    isActive: form.value.isActive,
  };

  if (form.value.type === 'Repeat') {
    const intervalValue = form.value.intervalValue ?? 1;
    return {
      ...base,
      interval: form.value.interval,
      intervalValue,
      daysOfWeek: form.value.interval === 'Week' ? form.value.daysOfWeek : [],
    };
  }

  return {
    ...base,
    interval: null,
    intervalValue: null,
    daysOfWeek: [],
  };
}

function toInputDateTime(value?: string) {
  if (!value) {
    return '';
  }
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return '';
  }
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
  return local.toISOString().slice(0, 16);
}

function fromInputDateTime(value?: string) {
  if (!value) {
    return null;
  }
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return null;
  }
  return new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString();
}
</script>

<style scoped>
.toggle-btn {
  border: 1px solid #526cff;
}

.week-day-btn {
  min-width: 0;
}
</style>
