<template>
  <PageLayout
    :breadcrumbs="[
      { label: t('device.label', 2), to: '/devices' },
      { label: device?.name ?? '', to: `/devices/${deviceId}` },
      { label: t('job_schedule.label', 2), to: `/devices/${deviceId}/schedules` },
    ]"
  >
    <template #actions>
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('job_schedule.create')"
        :icon="mdiPlus"
        @click="openCreateDialog"
      />
    </template>
    <template #default>
      <q-table
        row-key="id"
        :rows="schedules"
        :columns="columns"
        :loading="loadingSchedules"
        flat
        binary-state-sort
        :rows-per-page-options="[10, 20, 50]"
        class="shadow"
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
        :rows-per-page-label="t('table.rows_per_page_label')"
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiCodeTags" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>

        <template #body-cell-active="props">
          <q-td auto-width :props="props">
            <q-toggle
              v-model="props.row.isActive"
              color="primary"
              @update:model-value="(value) => toggleSchedule(props.row, value)"
            />
          </q-td>
        </template>

        <template #body-cell-name="props">
          <q-td :props="props">
            {{ props.row.name }}
          </q-td>
        </template>

        <template #body-cell-recipe="props">
          <q-td :props="props">
            {{ props.row.recipeName || t('job_schedule.unknown_recipe') }}
          </q-td>
        </template>

        <template #body-cell-interval="props">
          <q-td :props="props">
            {{ formatInterval(props.row) }}
          </q-td>
        </template>

        <template #body-cell-actions="props">
          <q-td auto-width :props="props">
            <q-btn
              :icon="mdiPencil"
              color="grey-color"
              flat
              round
              @click="openEditDialog(props.row)"
            >
              <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.edit') }}
              </q-tooltip>
            </q-btn>
            <q-btn
              :icon="mdiTrashCanOutline"
              color="grey-color"
              flat
              round
              @click="askToDelete(props.row)"
            >
              <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                {{ t('global.delete') }}
              </q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
  <JobScheduleDialog
    v-model="createDialogOpen"
    :device-id="deviceId"
    :schedule="null"
    @saved="handleScheduleSaved"
  />
  <JobScheduleDialog
    v-if="scheduleToEdit"
    v-model="editDialogOpen"
    :device-id="deviceId"
    :schedule="scheduleToEdit"
    @saved="handleScheduleSaved"
  />
  <DeleteConfirmationDialog v-model="deleteDialogOpen" :loading="isDeleting" @on-submit="deleteSchedule">
    <template #title>{{ t('job_schedule.delete_title') }}</template>
    <template #description>{{ t('job_schedule.delete_description') }}</template>
  </DeleteConfirmationDialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';
import { mdiCodeTags, mdiPencil, mdiPlus, mdiTrashCanOutline } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import DeviceService from '@/api/services/DeviceService';
import type { DeviceResponse } from '@/api/services/DeviceService';
import JobScheduleService, {
  type JobSchedulesResponse,
  type UpdateJobScheduleRequest,
} from '@/api/services/JobScheduleService';
import RecipeService from '@/api/services/RecipeService';
import type { RecipesQueryParams, RecipesResponse } from '@/api/services/RecipeService';
import { handleError } from '@/utils/error-handler';
import type { QTableProps } from 'quasar';
import JobScheduleDialog from '@/components/jobs/JobScheduleDialog.vue';
import DeleteConfirmationDialog from '@/components/core/DeleteConfirmationDialog.vue';
import type {
  JobScheduleListItem,
  JobScheduleWithRecipe,
} from '@/models/JobSchedule';

const { t } = useI18n();
const route = useRoute();

const deviceId = route.params.id?.toString() ?? '';
const device = ref<DeviceResponse>();
const schedules = ref<JobScheduleWithRecipe[]>([]);
const loadingSchedules = ref(false);

const createDialogOpen = ref(false);
const editDialogOpen = ref(false);
const scheduleToEdit = ref<JobScheduleWithRecipe | null>(null);

const deleteDialogOpen = ref(false);
const isDeleting = ref(false);
const schedulePendingDeletion = ref<JobScheduleWithRecipe | null>(null);

const recipeMap = ref<Record<string, string>>({});

async function loadDevice() {
  const { data, error } = await DeviceService.getDevice(deviceId);
  if (error) {
    handleError(error, t('device.toasts.loading_failed'));
    return;
  }
  device.value = data;
}

async function loadRecipes() {
  const map: Record<string, string> = {};
  let page = 1;
  let totalPages = 1;

  do {
    const query: RecipesQueryParams = {
      SortBy: 'name',
      Descending: false,
      SearchTerm: '',
      PageNumber: page,
      PageSize: 50,
      DeviceId: deviceId,
    };

    const { data, error } = await RecipeService.getRecipes(query);
    if (error) {
      handleError(error, t('job_schedule.toasts.recipes_failed'));
      break;
    }

    const response: RecipesResponse | undefined = data;
    response?.items?.forEach((recipe) => {
      if (recipe?.id) {
        map[recipe.id] = recipe.name ?? '';
      }
    });

    totalPages = response?.totalPages ?? 1;
    page += 1;
  } while (page <= totalPages);

  recipeMap.value = map;
}

function applyRecipeNames(items: JobSchedulesResponse | JobScheduleWithRecipe[]): JobScheduleWithRecipe[] {
  return items.map((schedule) => {
    const existingName = hasRecipeName(schedule) ? schedule.recipeName ?? '' : '';
    return {
      ...schedule,
      recipeName: recipeMap.value[schedule.recipeId] ?? existingName,
    };
  });
}

async function loadSchedules() {
  loadingSchedules.value = true;
  const { data, error } = await JobScheduleService.getSchedules(deviceId);
  loadingSchedules.value = false;

  if (error) {
    handleError(error, t('job_schedule.toasts.load_failed'));
    return;
  }

  schedules.value = applyRecipeNames(data ?? []);
}

watch(recipeMap, () => {
  schedules.value = applyRecipeNames(schedules.value);
});

watch(editDialogOpen, (open) => {
  if (!open) {
    scheduleToEdit.value = null;
  }
});

function openCreateDialog() {
  createDialogOpen.value = true;
}

function openEditDialog(schedule: JobScheduleWithRecipe) {
  scheduleToEdit.value = { ...schedule };
  editDialogOpen.value = true;
}

function askToDelete(schedule: JobScheduleWithRecipe) {
  schedulePendingDeletion.value = schedule;
  deleteDialogOpen.value = true;
}

async function deleteSchedule() {
  if (!schedulePendingDeletion.value) {
    return;
  }

  isDeleting.value = true;
  const { error } = await JobScheduleService.deleteSchedule(schedulePendingDeletion.value.id);
  isDeleting.value = false;

  if (error) {
    handleError(error, t('job_schedule.toasts.delete_failed'));
    return;
  }

  toast.success(t('job_schedule.toasts.delete_success'));
  deleteDialogOpen.value = false;
  schedulePendingDeletion.value = null;
  await loadSchedules();
}

async function handleScheduleSaved() {
  createDialogOpen.value = false;
  editDialogOpen.value = false;
  scheduleToEdit.value = null;
  await loadRecipes();
  await loadSchedules();
}

async function toggleSchedule(schedule: JobScheduleWithRecipe, isActive: boolean) {
  const previousValue = schedule.isActive;
  schedule.isActive = isActive;
  const payload = buildUpdatePayload(schedule);

  const { error } = await JobScheduleService.updateSchedule(schedule.id, payload);
  if (error) {
    schedule.isActive = previousValue;
    handleError(error, t('job_schedule.toasts.update_failed'));
    return;
  }
  toast.success(t('job_schedule.toasts.update_success'));
}

function buildUpdatePayload(schedule: JobScheduleWithRecipe): UpdateJobScheduleRequest {
  if (schedule.type === 'Repeat') {
    const interval = schedule.interval ?? 'Minute';
    const daysOfWeek = schedule.interval === 'Week' ? schedule.daysOfWeek : [];
    return {
      deviceId: schedule.deviceId,
      recipeId: schedule.recipeId,
      name: schedule.name,
      type: schedule.type,
      interval,
      intervalValue: schedule.intervalValue ?? 1,
      startTime: schedule.startTime,
      endTime: schedule.endTime ?? null,
      daysOfWeek,
      cycles: schedule.cycles,
      isActive: schedule.isActive,
    };
  }

  return {
    deviceId: schedule.deviceId,
    recipeId: schedule.recipeId,
    name: schedule.name,
    type: schedule.type,
    interval: schedule.interval ?? 'Minute',
    intervalValue: schedule.intervalValue ?? 1,
    startTime: schedule.startTime,
    endTime: schedule.endTime ?? null,
    daysOfWeek: [],
    cycles: schedule.cycles,
    isActive: schedule.isActive,
  };
}

function hasRecipeName(
  schedule: JobScheduleWithRecipe | JobScheduleListItem,
): schedule is JobScheduleWithRecipe {
  return 'recipeName' in schedule;
}

function formatInterval(schedule: JobScheduleWithRecipe) {
  if (schedule.type === 'Once') {
    return t('job_schedule.interval_once');
  }

  const value = schedule.intervalValue ?? 1;
  const interval = schedule.interval ?? 'Minute';

  if (interval === 'Week') {
    const days = (schedule.daysOfWeek ?? []).map((day) => t(`job_schedule.days.${day.toLowerCase()}`));
    const daysLabel = days.length > 0 ? days.join(', ') : t('job_schedule.days.none');
    return t('job_schedule.interval_week_days_only', { days: daysLabel });
  }

  const unitKey = `job_schedule.interval_units.${interval.toLowerCase()}`;
  return t('job_schedule.interval_every', { value, unit: t(unitKey, value) });
}

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'active',
    label: t('job_schedule.active'),
    field: 'isActive',
    sortable: true,
    align: 'center',
  },
  {
    name: 'name',
    label: t('job_schedule.name'),
    field: 'name',
    sortable: true,
    align: 'left',
  },
  {
    name: 'recipe',
    label: t('job_schedule.recipe'),
    field: 'recipeName',
    sortable: true,
    align: 'left',
  },
  {
    name: 'interval',
    label: t('job_schedule.interval'),
    field: 'interval',
    sortable: false,
    align: 'left',
  },
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);

loadDevice();
loadRecipes().then(loadSchedules);
</script>

<style scoped></style>
