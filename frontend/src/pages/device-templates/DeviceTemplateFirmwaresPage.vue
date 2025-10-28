<template>
  <div class="actions">
    <div class="title">{{ t('device_template.firmwares.title') }}</div>
    <q-space />
    <q-btn
      class="shadow col-grow col-lg-auto"
      color="primary"
      :icon="mdiPlus"
      :label="t('device_template.firmwares.create_firmware')"
      unelevated
      no-caps
      size="15px"
      @click="createDialogOpen = true"
    />
  </div>
  <q-table
    :rows="firmwares"
    :columns="columns"
    :loading="isLoading"
    row-key="id"
    flat
    class="shadow"
    :rows-per-page-options="[10, 20, 50]"
    :no-data-label="t('table.no_data_label')"
    :loading-label="t('table.loading_label')"
    :rows-per-page-label="t('table.rows_per_page_label')"
  >
    <template #no-data="{ message }">
      <div class="full-width column flex-center q-pa-lg nothing-found-text">
        <q-icon :name="mdiMemory" class="q-mb-md" size="2rem"></q-icon>
        {{ message }}
      </div>
    </template>
    <template #body-cell-createdAt="props">
      <q-td :props="props">
        {{ formatDate(props.row.createdAt) }}
      </q-td>
    </template>
    <template #body-cell-isActive="props">
      <q-td :props="props">
        <q-chip
          :color="props.row.isActive ? 'positive' : 'grey-5'"
          :text-color="props.row.isActive ? 'white' : 'grey-10'"
          dense
          square
        >
          {{ props.row.isActive ? t('global.yes') : t('global.no') }}
        </q-chip>
      </q-td>
    </template>
    <template #body-cell-actions="props">
      <q-td auto-width :props="props">
        <q-btn :icon="mdiPencil" color="grey-7" flat round @click.stop="openEditDialog(props.row.id)">
          <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">{{ t('global.edit') }}</q-tooltip>
        </q-btn>
        <q-btn :icon="mdiTrashCanOutline" color="grey-7" flat round @click.stop="openDeleteDialog(props.row.id)">
          <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">{{ t('global.delete') }}</q-tooltip>
        </q-btn>
      </q-td>
    </template>
  </q-table>

  <CreateDeviceFirmwareDialog
    v-model="createDialogOpen"
    :template-id="templateId"
    @created="loadFirmwares"
  />
  <EditDeviceFirmwareDialog
    v-if="firmwareIdForEdit"
    v-model="editDialogOpen"
    :template-id="templateId"
    :firmware-id="firmwareIdForEdit"
    @updated="loadFirmwares"
  />
  <DeleteDeviceFirmwareDialog
    v-if="firmwareIdForDelete"
    v-model="deleteDialogOpen"
    :template-id="templateId"
    :firmware-id="firmwareIdForDelete"
    @deleted="handleDeleted"
  />
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import type { QTableProps } from 'quasar';
import { date as quasarDate } from 'quasar';
import { mdiMemory, mdiPencil, mdiTrashCanOutline, mdiPlus } from '@quasar/extras/mdi-v7';
import DeviceFirmwareService, { type DeviceFirmwareResponse } from '@/api/services/DeviceFirmwareService';
import { handleError } from '@/utils/error-handler';
import CreateDeviceFirmwareDialog from '@/components/device-templates/firmwares/CreateDeviceFirmwareDialog.vue';
import EditDeviceFirmwareDialog from '@/components/device-templates/firmwares/EditDeviceFirmwareDialog.vue';
import DeleteDeviceFirmwareDialog from '@/components/device-templates/firmwares/DeleteDeviceFirmwareDialog.vue';

const { t } = useI18n();
const route = useRoute();

const templateId = route.params.id as string;

const firmwares = ref<DeviceFirmwareResponse[]>([]);
const isLoading = ref(false);

async function loadFirmwares() {
  isLoading.value = true;
  const { data, error } = await DeviceFirmwareService.getDeviceFirmwares(templateId);
  isLoading.value = false;

  if (error) {
    handleError(error, t('device_template.firmwares.toasts.load_failed'));
    return;
  }

  firmwares.value = data ?? [];
}

loadFirmwares();

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'versionNumber',
    field: 'versionNumber',
    align: 'left',
    sortable: true,
    label: t('device_template.firmwares.version_number'),
  },
  {
    name: 'createdAt',
    field: 'createdAt',
    align: 'left',
    sortable: true,
    label: t('device_template.firmwares.created_at'),
  },
  {
    name: 'isActive',
    field: 'isActive',
    align: 'left',
    sortable: true,
    label: t('device_template.firmwares.is_active'),
  },
  {
    name: 'actions',
    field: 'actions',
    align: 'center',
    sortable: false,
    label: t('global.actions'),
  },
]);

function formatDate(value: string) {
  if (!value) {
    return '-';
  }
  return quasarDate.formatDate(value, 'YYYY-MM-DD HH:mm');
}

const createDialogOpen = ref(false);
const editDialogOpen = ref(false);
const deleteDialogOpen = ref(false);
const firmwareIdForEdit = ref<string | null>(null);
const firmwareIdForDelete = ref<string | null>(null);

function openEditDialog(firmwareId: string) {
  firmwareIdForEdit.value = firmwareId;
  editDialogOpen.value = true;
}

function openDeleteDialog(firmwareId: string) {
  firmwareIdForDelete.value = firmwareId;
  deleteDialogOpen.value = true;
}

function handleDeleted() {
  loadFirmwares();
  firmwareIdForDelete.value = null;
}

watch(deleteDialogOpen, (open) => {
  if (!open) {
    firmwareIdForDelete.value = null;
  }
});

watch(editDialogOpen, (open) => {
  if (!open) {
    firmwareIdForEdit.value = null;
  }
});
</script>

<style lang="scss" scoped>
.actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 0.75rem 1rem;
  margin-bottom: 1rem;
}

.title {
  font-size: 1.4rem;
  font-weight: 600;
  margin: 0;
}

.nothing-found-text {
  color: #7a7a7a;
  font-size: 14px;
  text-align: center;
}
</style>
