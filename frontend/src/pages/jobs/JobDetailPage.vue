<template>
  <PageLayout
    v-if="job.data"
    :title="job.data.name"
    :previous-title="t('job.label', 2)"
    :previous-route="`/devices/${job.data.deviceId}`"
  >
    <template #description>
      <q-badge class="q-pa-xs q-ml-sm" color="primary">
        <!-- eslint-disable-next-line @intlify/vue-i18n/no-raw-text -->
        {{ t('job.job_cycle') }}: {{ job?.data.status.currentCycle ?? 1 }}/{{ job?.data.noOfReps }}
      </q-badge>
      <job-status-badges v-if="job" class="q-ml-sm" :job="job.data"></job-status-badges>
    </template>
    <template #actions>
      <AutoRefreshButton v-model="refreshInterval" :loading="job.isLoading" @on-refresh="job.refresh" />
      <job-controls
        v-if="job && authStore.isAdmin"
        class="col-grow"
        :running-job="job.data"
        @action-performed="job.refresh"
      />
    </template>
    <template #default>
      <q-table
        :rows="steps"
        :columns="columns"
        :loading="job.isLoading"
        flat
        :rows-per-page-options="[0]"
        class="shadow"
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
        hide-bottom
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiListStatus" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>
        <template #body-cell-progress="props">
          <q-td auto-width :props="props">
            <div style="min-height: 45px" class="row items-center justify-center">
              <div
                v-if="currentStep >= props.row.step && currentStep < props.row.step + props.row.cycles"
                class="row items-center justify-center"
              >
                <q-circular-progress
                  size="32px"
                  :thickness="0.15"
                  color="primary"
                  show-value
                  :indeterminate="!job?.data.paused && job?.data.currentStatus == JobStatusEnum.JOB_PROCESSING"
                >
                  <div class="current-step-progress">
                    <!-- eslint-disable-next-line @intlify/vue-i18n/no-raw-text -->
                    {{ currentStepCycle }}/{{ props.row.cycles }}
                  </div>
                </q-circular-progress>
              </div>
              <q-icon v-else-if="props.row.step < currentStep" :name="mdiCheck" size="28px" color="green"></q-icon>
              <q-icon v-else :name="mdiCheck" size="28px" color="grey"></q-icon>
            </div>
          </q-td>
        </template>
        <template #body-cell-step="props">
          <q-td auto-width :props="props">
            {{ props.row.step }}
          </q-td>
        </template>
        <template #body-cell-cycles="props">
          <q-td auto-width :props="props">
            {{ props.row.cycles }}
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { computed, reactive } from 'vue';
import { useRoute } from 'vue-router';
import jobService from '@/services/JobService';
import JobControls from '@/components/jobs/JobControls.vue';
import { JobStatusEnum } from '@/models/JobStatusEnum';
import JobStatusBadges from '@/components/jobs/JobStatusBadges.vue';
import { useAuthStore } from '@/stores/auth-store';
import { useI18n } from 'vue-i18n';
import { mdiCheck, mdiListStatus } from '@quasar/extras/mdi-v6';
import PageLayout from '@/layouts/PageLayout.vue';
import { useAsyncData } from '@/composables/useAsyncData';
import { useJobStore } from '@/stores/job-store';
import { useStorage } from '@vueuse/core';
import AutoRefreshButton from '@/components/core/AutoRefreshButton.vue';

const { t } = useI18n();

const route = useRoute();
const authStore = useAuthStore();
const jobStore = useJobStore();

const refreshInterval = useStorage('JobDetailRefreshInterval', 30);
const job = reactive(
  useAsyncData(() => jobService.getJobById(route.params.id.toString()), t('job.toasts.load_failed')),
);
job.data = jobStore.getJobById(route.params.id.toString());

const currentStep = computed(() => {
  if (!job.data || !job.data.status || !job.data.status.currentStep) return 1;
  return job.data.status.currentStep;
});

const currentStepCycle = computed(() => {
  if (!job.data?.status || !steps.value.length) return 1;

  const currentStepIndex = steps.value.findIndex(
    (step) => currentStep.value > step.step && currentStep.value < step.step + step.cycles,
  );

  if (currentStepIndex === -1) return 1;

  const currentStepObj = steps.value[currentStepIndex];
  return currentStep.value - currentStepObj.step + 1;
});

const steps = computed(() => {
  if (!job.data || !job.data.commands || job.data.commands.length === 0) return [];

  const groupedCommands = [];
  let lastCommandId = job.data.commands[0].id;
  let cycleCount = 1;
  let step = 1;

  for (let i = 1; i < job.data.commands.length; i++) {
    const command = job.data.commands[i];
    if (command.id !== lastCommandId) {
      groupedCommands.push({
        step: step,
        name: job.data.commands[i - 1].name,
        cycles: cycleCount,
      });
      step += cycleCount;
      lastCommandId = command.id;
      cycleCount = 1;
    } else {
      cycleCount++;
    }
  }

  groupedCommands.push({
    step: step,
    name: job.data.commands[job.data.commands.length - 1].name,
    cycles: cycleCount,
  });

  return groupedCommands;
});

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'progress',
    label: t('job.progress'),
    field: '',
    sortable: false,
    align: 'center',
  },
  {
    name: 'step',
    label: t('job.step'),
    field: 'step',
    sortable: false,
    align: 'center',
  },
  {
    name: 'cycles',
    label: t('job.cycle', 2),
    field: 'cycles',
    sortable: false,
    align: 'center',
  },
  {
    name: 'name',
    label: t('command.label'),
    field: 'name',
    sortable: false,
    align: 'left',
  },
]);
</script>

<style lang="scss" scoped>
.job-name {
  font-size: 1.8em;
  font-weight: 600;
  margin: 0 0 0 0.75rem;
  padding: 0;
  color: $accent;
}

.current-step-progress {
  font-size: 0.75rem;
  font-weight: 500;
  color: $primary;
}
</style>
