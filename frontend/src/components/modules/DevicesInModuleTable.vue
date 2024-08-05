<template>
  <div>
    <div class="q-mb-md row">
      <p class="devices-text">{{ t('device.label', 2) }}</p>
      <q-space></q-space>
      <q-btn
        v-if="authStore.isAdmin"
        class="shadow bg-white"
        color="primary"
        unelevated
        outline
        no-caps
        size="15px"
        :label="t('device.add_device')"
        :icon="mdiPlus"
        @click="addDeviceDialog = true"
      />
    </div>
    <q-table
      :rows="devices"
      :columns="columns"
      flat
      hide-pagination
      :no-data-label="t('table.no_data_label')"
      :loading-label="t('table.loading_label')"
      :rows-per-page-label="t('table.rows_per_page_label')"
      class="outline shadow"
    >
      <template #no-data="{ message }">
        <div class="full-width column flex-center q-pa-lg nothing-found-text">
          <q-icon :name="mdiCellphoneLink" class="q-mb-md" size="50px"></q-icon>
          {{ message }}
        </div>
      </template>

      <template #body-cell-lastResponse="propsContact">
        <q-td auto-width :props="propsContact">
          {{ formatTimeToDistance(propsContact.row.lastResponse) }}
          <q-tooltip v-if="propsContact.row.lastResponse" content-style="font-size: 11px" :offset="[0, 4]">
            {{ formatToLocalTime(propsContact.row.lastResponse) }}
          </q-tooltip>
        </q-td>
      </template>

      <template #body-cell="propsCell">
        <q-td :props="propsCell" no-hover>
          {{ propsCell.row[propsCell.col.field] }}
        </q-td>
      </template>

      <template #body-cell-name="propsCellName">
        <q-td :props="propsCellName" no-hover>
          <router-link :to="`/devices/${propsCellName.row.uid}`" class="text-black text-weight-regular">
            {{ propsCellName.row.name }}
          </router-link>
        </q-td>
      </template>

      <template #body-cell-status="propsStatus">
        <q-td auto-width :props="propsStatus">
          <StatusDot :status="getDeviceStatus(propsStatus.row)" />
        </q-td>
      </template>

      <template #body-cell-jobstatus="jobProps">
        <q-td auto-width :props="jobProps">
          <JobStatusIcon :status="getLastJobStatus(jobProps.row)" />
        </q-td>
      </template>

      <template #body-cell-actions="propsActions">
        <q-td auto-width :props="propsActions" no-hover>
          <q-btn :icon="mdiOpenInNew" color="grey-color" flat round :to="`/devices/${propsActions.row.uid}`"
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.open') }}
            </q-tooltip>
          </q-btn>
          <q-btn
            v-if="authStore.isAdmin"
            :icon="mdiPencil"
            color="grey-color"
            flat
            round
            :to="`/devices/${propsActions.row.uid}/edit`"
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.edit') }}
            </q-tooltip>
          </q-btn>
          <q-btn
            v-if="authStore.isAdmin"
            :icon="mdiTrashCanOutline"
            color="grey-color"
            flat
            round
            @click.stop="
              deleteDialog = true;
              deviceToDelete = propsActions.row;
            "
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.delete') }}
            </q-tooltip>
          </q-btn>
        </q-td>
      </template>
    </q-table>
    <RemoveDeviceFromModuleDialog
      v-if="deviceToDelete"
      v-model="deleteDialog"
      :device="deviceToDelete"
      :module="module"
      @on-deleted="deviceDeleted"
    />
    <AddDeviceToModuleDialog
      v-model="addDeviceDialog"
      :module="module"
      :already-added-devices="devices"
      @on-added="emit('onChange', devices)"
    />
  </div>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import RemoveDeviceFromModuleDialog from './RemoveDeviceFromModuleDialog.vue';
import AddDeviceToModuleDialog from './AddDeviceToModuleDialog.vue';
import { PropType, computed, ref } from 'vue';
import { Device, getDeviceStatus, getLastJobStatus } from '@/models/Device';
import { Module } from '@/models/Module';
import { useAuthStore } from '@/stores/auth-store';
import { useI18n } from 'vue-i18n';
import { mdiCellphoneLink, mdiOpenInNew, mdiPencil, mdiPlus, mdiTrashCanOutline } from '@quasar/extras/mdi-v7';
import JobStatusIcon from '../jobs/JobStatusIcon.vue';
import StatusDot from '@/components/devices/StatusDot.vue';
import { formatTimeToDistance, formatToLocalTime } from '@/utils/date-utils';

const devices = defineModel<Device[]>({ required: true });
defineProps({
  module: {
    type: Object as PropType<Module>,
    required: true,
  },
});
const emit = defineEmits(['onChange']);

const { t } = useI18n();
const authStore = useAuthStore();

const deleteDialog = ref(false);
const deviceToDelete = ref<Device>();
function deviceDeleted() {
  devices.value = devices.value.filter((device) => device.uid !== deviceToDelete.value?.uid);
  emit('onChange', devices.value);
}

const addDeviceDialog = ref(false);

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'name',
    label: t('global.name'),
    field: 'name',
    sortable: true,
    align: 'left',
  },
  {
    name: 'lastResponse',
    label: t('device.last_activity'),
    field: 'lastResponse',
    align: 'left',
    sortable: true,
  },
  {
    name: 'status',
    label: t('device.status'),
    field(row) {
      return getDeviceStatus(row);
    },
    align: 'center',
    sortable: false,
  },
  {
    name: 'jobstatus',
    label: t('job.job_status'),
    field(row) {
      return getLastJobStatus(row);
    },
    align: 'center',
    sortable: false,
  },
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);
</script>

<style lang="scss" scoped>
.devices-text {
  font-size: 1.4em;
  font-weight: 600;
  margin: 0;
  padding: 0;
  color: $secondary;
}
</style>
