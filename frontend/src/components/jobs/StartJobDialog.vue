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
      <RecipeSelect v-model="selectedRecipe" />
      <q-input
        ref="repetitionsRef"
        v-model="jobToRun.cycles"
        :label="t('job.repetitions')"
        type="number"
        lazy-rules
        :rules="repetitionRules"
      />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { QInput } from 'quasar';
import { toast } from 'vue3-toastify';
import { handleError } from '@/utils/error-handler';
import { useI18n } from 'vue-i18n';
import { isFormValid } from '@/utils/form-validation';
import DialogCommon from '../core/DialogCommon.vue';
import JobService from '@/api/services/JobService';
import { RecipeSelectData } from '@/components/recipes/RecipeSelect.vue';
import { StartJobRequest } from '@/api/types/Job';
import RecipeSelect from '@/components/recipes/RecipeSelect.vue';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  deviceId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['jobStarted']);

const { t } = useI18n();

const jobToRun = ref<StartJobRequest>({
  cycles: 1,
  recipeId: '',
});
const selectedRecipe = ref<RecipeSelectData>();
const jobIsStarting = ref(false);

async function runJob() {
  if (!jobToRun.value) return;

  const form = [repetitionsRef.value, recipeRef.value];
  if (!isFormValid(form)) return;

  jobIsStarting.value = true;
  jobToRun.value.recipeId = selectedRecipe.value?.id ?? '';
  const { data, error } = await JobService.startJob(props.deviceId, jobToRun.value);
  jobIsStarting.value = false;

  if (error) {
    handleError(error, t('job.toasts.start_failed'));
    return;
  }

  jobToRun.value = { cycles: 1, recipeId: '' };

  isDialogOpen.value = false;
  toast.success(t('job.toasts.start_success'));
  emit('jobStarted');
}

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
