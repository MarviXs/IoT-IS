<template>
  <div>
    <q-table
      v-model:pagination="pagination"
      :rows="devicesFiltered"
      :columns="columns"
      :loading="props.loading"
      flat
      :rows-per-page-options="[10, 20, 50]"
      :no-data-label="t('table.no_data_label')"
      :loading-label="t('table.loading_label')"
      :rows-per-page-label="t('table.rows_per_page_label')"
      binary-state-sort
      @request="emit('onRequest', $event)"
    >
      <template #no-data="{ message }">
        <div class="full-width column flex-center q-pa-lg nothing-found-text">
          <q-icon :name="mdiCellphoneLink" class="q-mb-md" size="50px"></q-icon>
          {{ message }}
        </div>
      </template>

      <template #body-cell-name="propsCell">
        <q-td :props="propsCell">
          <router-link :to="`/devices/${propsCell.row.id}`">
            {{ propsCell.row.name }}
          </router-link>
        </q-td>
      </template>

      <template #body-cell-lastResponse="propsContact">
        <q-td auto-width :props="propsContact">
          {{ formatTimeToDistance(propsContact.row.lastSeen) }}
          <q-tooltip v-if="propsContact.row.lastSeen" content-style="font-size: 11px" :offset="[0, 4]">
            {{ new Date(propsContact.row.lastSeen).toLocaleString() }}
          </q-tooltip>
        </q-td>
      </template>

      <template #body-cell-status="propsStatus">
        <q-td auto-width :props="propsStatus">
          <StatusDot :connected="propsStatus.row.connected" />
        </q-td>
      </template>

      <template #body-cell-jobstatus="jobProps">
        <q-td auto-width :props="jobProps">
          Job status
          <!-- <JobStatusIcon :status="getLastJobStatus(jobProps.row)" /> -->
        </q-td>
      </template>

      <template #body-cell-actions="propsActions">
        <q-td auto-width :props="propsActions">
          <q-btn :icon="mdiChartLine" color="grey-color" flat round :to="`/devices/${propsActions.row.id}`"
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.open') }}
            </q-tooltip>
          </q-btn>
          <q-btn :icon="mdiPencil" color="grey-color" flat round @click.stop="openUpdateDialog(propsActions.row.id)"
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.edit') }}
            </q-tooltip>
          </q-btn>
          <q-btn :icon="mdiDotsVertical" color="grey-color" flat round>
            <q-menu anchor="bottom right" self="top right">
              <q-list>
                <q-item v-close-popup clickable @click.stop="openDeleteDialog(propsActions.row.id)">
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiTrashCan" />
                    <div>{{ t('global.delete') }}</div>
                  </div>
                </q-item>

                <!-- <q-item
                  v-close-popup
                  clickable
                  @click="
                    shareDialog = true;
                    deviceToShare = propsActions.row;
                  "
                >
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiShare" />
                    <div>{{ t('device.share_device') }}</div>
                  </div>
                </q-item> -->
              </q-list>
            </q-menu>
          </q-btn>
        </q-td>
      </template>
    </q-table>
    <DeleteDeviceDialog
      v-if="deviceToDelete"
      v-model="isDeleteDialogOpen"
      :device-id="deviceToDelete"
      @on-deleted="emit('onChange')"
    />
    <EditDeviceDialog
      v-if="deviceToUpdate"
      v-model="isUpdateDialogOpen"
      :device-id="deviceToUpdate"
      @on-create="emit('onChange')"
    />

    <!-- <ShareDeviceDialog v-if="deviceToShare" v-model="shareDialog" :device="deviceToShare" /> -->
  </div>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
// import ShareDeviceDialog from './ShareDeviceDialog.vue';
import { PropType, computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAuthStore } from '@/stores/auth-store';
import {
  mdiCellphoneLink,
  mdiChartLine,
  mdiDotsVertical,
  mdiEye,
  mdiOpenInNew,
  mdiPencil,
  mdiTrashCan,
  mdiTrashCanOutline,
} from '@quasar/extras/mdi-v7';
import DeleteDeviceDialog from '@/components/devices/DeleteDeviceDialog.vue';
import { formatTimeToDistance } from '@/utils/date-utils';
import { PaginationClient } from '@/models/Pagination';
import { DevicesResponse } from '@/api/services/DeviceService';
import EditDeviceDialog from '@/components/devices/EditDeviceDialog.vue';
import StatusDot from './StatusDot.vue';

const props = defineProps({
  devices: {
    type: Object as PropType<DevicesResponse>,
    required: false,
    default: null,
  },
  loading: {
    type: Boolean,
    required: true,
  },
});

const pagination = defineModel<PaginationClient>('pagination');

const devicesFiltered = computed(() => props.devices?.items ?? []);

const emit = defineEmits(['onChange', 'onRequest']);

const { t } = useI18n();
const authStore = useAuthStore();

const isDeleteDialogOpen = ref(false);
const deviceToDelete = ref<string>();
function openDeleteDialog(deviceId: string) {
  isDeleteDialogOpen.value = true;
  deviceToDelete.value = deviceId;
}

const isUpdateDialogOpen = ref(false);
const deviceToUpdate = ref<string>();
function openUpdateDialog(deviceId: string) {
  deviceToUpdate.value = deviceId;
  isUpdateDialogOpen.value = true;
}

const shareDialog = ref(false);
// const deviceToShare = ref<Device>();

// const getPermissions = (device: Device) => {
//   if (DeviceService.isOwner(device)) {
//     return t('global.owner');
//   }
//   return t('global.shared');
// };

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'status',
    label: t('device.status'),
    field: 'connected',
    align: 'center',
    sortable: false,
  },
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
    field: 'lastSeen',
    align: 'left',
    sortable: false,
  },
  // {
  //   name: 'jobstatus',
  //   label: t('job.job_status'),
  //   field(row) {
  //     return 'LastJobStatus';
  //   },
  //   align: 'center',
  //   sortable: false,
  // },
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);
</script>

<style lang="scss" scoped></style>
