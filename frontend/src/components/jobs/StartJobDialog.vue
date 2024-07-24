<template>
  <dialog-common
    v-if="jobToRun"
    v-model="isDialogOpen"
    :action-label="t('job.run_job')"
    :loading="jobIsStarting"
    @on-submit="runJob"
  >
    <template #title>{{ t('job.run_job') }}</template>
    <template #default>
      <q-select
        ref="recipeRef"
        v-model="jobToRun.recipeId"
        :label="t('recipe.label')"
        :options="commandStore.commands.data"
        option-value="id"
        option-label="name"
        lazy-rules
        :rules="recipeRules"
        emit-value
        map-options
      />
      <q-input
        ref="repetitionsRef"
        v-model="jobToRun.repetitions"
        :label="t('job.repetitions')"
        type="number"
        lazy-rules
        :rules="repetitionRules"
      />
      <div>{{ t('job.schedule') }}</div>
      <q-btn-group unelevated>
        <q-btn
          v-for="(button, index) in dayButtons"
          :key="index"
          :label="button.label.toUpperCase()"
          :color="button.onOff ? 'primary' : 'grey'"
          class="full-width"
          @click="dayButtons[index].onOff = !dayButtons[index].onOff"
        ></q-btn>
      </q-btn-group>
      <q-input v-model="scheduledTime" mask="time" fill-mask="-" :rules="['time']" :label="t('job.scheduled_time')">
        <template #append>
          <q-icon :name="mdiClock" class="cursor-pointer">
            <q-popup-proxy transition-show="scale" transition-hide="scale">
              <q-time v-model="scheduledTime" format24h>
                <div class="row items-center justify-end">
                  <q-btn v-close-popup :label="t('global.close')" color="primary" flat />
                </div>
              </q-time>
            </q-popup-proxy>
          </q-icon>
        </template>
      </q-input>
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { PropType, computed, ref } from 'vue';
import JobService from '@/services/JobService';
import { Recipe } from '@/models/Recipe';
import { QInput } from 'quasar';
import { JobToRun } from '@/models/Job';
import { toast } from 'vue3-toastify';
import { parse } from 'date-fns';
import { handleError } from '@/utils/error-handler';
import { Device } from '@/models/Device';
import { useI18n } from 'vue-i18n';
import { isFormValid } from '@/utils/form-validation';
import { mdiClock } from '@quasar/extras/mdi-v6';
import DialogCommon from '../core/DialogCommon.vue';
import { useCommandStore } from '@/stores/command-store';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  device: {
    type: Object as PropType<Device>,
    required: true,
  },
});
const emit = defineEmits(['jobStarted']);

const { t } = useI18n();

const jobToRun = ref<JobToRun>();
const selectedRecipe = ref<Recipe | null>(null);
const jobIsStarting = ref(false);
const commandStore = useCommandStore();
commandStore.commands.refresh();

async function runJob() {
  if (!jobToRun.value || !props.device) return;

  const form = [repetitionsRef.value, recipeRef.value];
  if (!isFormValid(form)) return;

  try {
    jobIsStarting.value = true;
    jobToRun.value.scheduledDays = selectedDays.value;

    const date = parse(scheduledTime.value, 'HH:mm', new Date());

    jobToRun.value.scheduledHour = date.getHours();
    jobToRun.value.scheduledMinute = date.getMinutes();

    await JobService.runJobFromRecipe(jobToRun.value);

    isDialogOpen.value = false;
    toast.success(t('job.toasts.start_success'));
    emit('jobStarted');
  } catch (error) {
    handleError(error, t('job.toasts.start_failed'));
  } finally {
    jobIsStarting.value = false;
  }
}

const dayButtons = ref([
  { label: t('days_short.mon'), value: 1, onOff: false },
  { label: t('days_short.tue'), value: 2, onOff: false },
  { label: t('days_short.wed'), value: 3, onOff: false },
  { label: t('days_short.thu'), value: 4, onOff: false },
  { label: t('days_short.fri'), value: 5, onOff: false },
  { label: t('days_short.sat'), value: 6, onOff: false },
  { label: t('days_short.sun'), value: 7, onOff: false },
]);

const selectedDays = computed(() => {
  return dayButtons.value.filter((button) => button.onOff).map((button) => button.value);
});

const scheduledTime = ref();

function resetDialog() {
  const now = new Date();
  const day = now.getDay() || 7;
  const formattedTime = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`;

  jobToRun.value = {
    recipeId: '',
    deviceId: props.device?.uid ?? '',
    repetitions: 1,
    scheduledDays: [day],
    scheduledHour: now.getHours(),
    scheduledMinute: now.getMinutes(),
  };
  dayButtons.value.forEach((button) => (button.onOff = false));
  scheduledTime.value = formattedTime;
  selectedRecipe.value = null;

  dayButtons.value[day - 1].onOff = true;
}
resetDialog();

//Input validation
const repetitionsRef = ref<QInput>();
const repetitionRules = [(val: number) => (val && val > 0) || t('job.rules.repetitions_min')];

const recipeRef = ref<QInput>();
const recipeRules = [
  (val: string) => {
    if (!val) return t('global.rules.required');
    return true;
  },
];
</script>

<style lang="scss" scoped></style>
