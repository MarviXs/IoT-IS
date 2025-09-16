<template>
  <dialog-common v-if="jobToRun" v-model="isDialogOpen">
    <template #title>{{ t('job.run_job') }}</template>
    <template #default>
      <q-form @submit="runJob" ref="jobForm">
        <q-card-section class="column q-gutter-md">
          <RecipeSelect v-model="selectedRecipe" :device-id="deviceId" />
          <q-input
            v-model="jobToRun.cycles"
            :label="t('job.repetitions')"
            type="number"
            lazy-rules
            :rules="repetitionRules"
            :disable="jobToRun.isInfinite"
          />
          <q-checkbox v-model="jobToRun.isInfinite" dense :label="t('job.infinite_job')" />
        </q-card-section>
        <q-card-actions align="right" class="text-primary">
          <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
          <q-btn
            unelevated
            color="primary"
            :label="t('job.run_job')"
            type="submit"
            no-caps
            padding="6px 20px"
            :loading="jobIsStarting"
          />
        </q-card-actions>
      </q-form>
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { QInput } from 'quasar';
import { toast } from 'vue3-toastify';
import { handleError } from '@/utils/error-handler';
import { useI18n } from 'vue-i18n';
import DialogCommon from '../core/DialogCommon.vue';
import JobService from '@/api/services/JobService';
import type { RecipeSelectData } from '@/components/recipes/RecipeSelect.vue';
import type { StartJobRequest } from '@/api/services/JobService';
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
  isInfinite: false,
});
const selectedRecipe = ref<RecipeSelectData>();
const jobIsStarting = ref(false);
const jobForm = ref();

async function runJob() {
  if (!jobToRun.value) return;
  if (!jobForm.value?.validate()) return;

  jobIsStarting.value = true;
  jobToRun.value.recipeId = selectedRecipe.value?.id ?? '';
  const { data, error } = await JobService.startJob(props.deviceId, jobToRun.value);
  jobIsStarting.value = false;

  if (error) {
    handleError(error, t('job.toasts.start_failed'));
    return;
  }

  jobToRun.value = { cycles: 1, recipeId: '', isInfinite: false };

  isDialogOpen.value = false;
  toast.success(t('job.toasts.start_success'));
  emit('jobStarted');
}

//Input validation
const repetitionRules = [(val: number) => (val && val > 0) || t('job.rules.repetitions_min')];
</script>

<style lang="scss" scoped></style>
