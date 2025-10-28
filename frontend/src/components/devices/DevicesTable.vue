<template>
  <div>
    <div v-if="isMobile">
      <q-infinite-scroll v-if="mobileItems.length" ref="infiniteScrollRef" :offset="120" @load="onLoadMore">
        <div class="devices-grid">
          <q-card
            v-for="device in mobileItems"
            :key="device.id"
            class="device-card shadow"
            v-ripple
            @click="goToDevice(device.id)"
          >
            <q-card-section class="device-card__section">
              <div class="device-card__name text-subtitle1 ellipsis">{{ device.name }}</div>
              <div class="device-card__status row items-center">
                <StatusDot :connected="device.connected" />
                <span class="q-ml-sm">{{ device.connected ? t('device.connected') : t('device.disconnected') }}</span>
              </div>
            </q-card-section>
            <q-menu context-menu touch-position>
              <q-list>
                <q-item clickable v-close-popup @click.stop="openUpdateDialog(device.id)">
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiPencil" />
                    <div>{{ t('global.edit') }}</div>
                  </div>
                </q-item>
                <q-item
                  v-if="device.permission == 'Owner'"
                  clickable
                  v-close-popup
                  @click.stop="openDeleteDialog(device.id)"
                >
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiTrashCan" />
                    <div>{{ t('global.delete') }}</div>
                  </div>
                </q-item>
                <q-item
                  v-if="device.permission == 'Owner'"
                  clickable
                  v-close-popup
                  @click.stop="openShareDialog(device.id)"
                >
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiShare" />
                    <div>{{ t('device.share_device') }}</div>
                  </div>
                </q-item>
              </q-list>
            </q-menu>
          </q-card>
        </div>

        <template #loading>
          <div class="row justify-center q-my-md">
            <q-spinner-dots color="primary" size="32px" />
          </div>
        </template>
      </q-infinite-scroll>
      <div v-else class="full-width column flex-center q-pa-lg nothing-found-text">
        <q-icon :name="mdiCellphoneLink" class="q-mb-md" size="50px"></q-icon>
        {{ props.loading ? t('table.loading_label') : t('table.no_data_label') }}
      </div>
    </div>
    <q-table
      v-else
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
      class="shadow"
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
          <router-link :to="`/devices/${propsCell.row.id}`" class="text-weight-medium">
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
                <q-item
                  v-if="propsActions.row.permission == 'Owner'"
                  v-close-popup
                  clickable
                  @click.stop="openDeleteDialog(propsActions.row.id)"
                >
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiTrashCan" />
                    <div>{{ t('global.delete') }}</div>
                  </div>
                </q-item>
                <q-item
                  v-if="propsActions.row.permission == 'Owner'"
                  v-close-popup
                  clickable
                  @click="openShareDialog(propsActions.row.id)"
                >
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiShare" />
                    <div>{{ t('device.share_device') }}</div>
                  </div>
                </q-item>
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
      @on-update="emit('onChange')"
    />

    <ShareDeviceDialog v-if="deviceToShare" v-model="shareDialog" :device-id="deviceToShare" />
  </div>
</template>

<script setup lang="ts">
import { QInfiniteScroll, useQuasar, type QTableProps } from 'quasar';
import ShareDeviceDialog from './ShareDeviceDialog.vue';
import type { PropType } from 'vue';
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import {
  mdiCellphoneLink,
  mdiChartLine,
  mdiDotsVertical,
  mdiPencil,
  mdiShare,
  mdiTrashCan,
} from '@quasar/extras/mdi-v7';
import DeleteDeviceDialog from '@/components/devices/DeleteDeviceDialog.vue';
import { formatTimeToDistance } from '@/utils/date-utils';
import type { PaginationClient } from '@/models/Pagination';
import type { DevicesResponse } from '@/api/services/DeviceService';
import EditDeviceDialog from '@/components/devices/EditDeviceDialog.vue';
import StatusDot from './StatusDot.vue';
import { useRouter } from 'vue-router';

type DeviceList = NonNullable<DevicesResponse['items']>;

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
  mobileDevices: {
    type: Array as PropType<DeviceList>,
    required: false,
    default: () => [] as DeviceList,
  },
  hasMore: {
    type: Boolean,
    required: false,
    default: false,
  },
  resetKey: {
    type: Number,
    required: false,
    default: 0,
  },
});

const pagination = defineModel<PaginationClient>('pagination');

const devicesFiltered = computed(() => props.devices?.items ?? []);

const emit = defineEmits(['onChange', 'onRequest', 'onLoadMore']);

const { t } = useI18n();
const $q = useQuasar();
const router = useRouter();
const isMobile = computed(() => $q.platform.is.mobile);

const mobileItems = computed(() => {
  if (isMobile.value) {
    return props.mobileDevices;
  }

  return devicesFiltered.value;
});

const infiniteScrollRef = ref<InstanceType<typeof QInfiniteScroll>>();

watch(
  () => props.resetKey,
  () => {
    infiniteScrollRef.value?.reset();
  },
);

function onLoadMore(_index: number, done: (stop?: boolean) => void) {
  if (!props.hasMore) {
    done(true);
    return;
  }

  emit('onLoadMore', done);
}

const isDeleteDialogOpen = ref(false);
const deviceToDelete = ref<string>();
function openDeleteDialog(deviceId: string) {
  isDeleteDialogOpen.value = true;
  deviceToDelete.value = deviceId;
}

function goToDevice(deviceId: string) {
  router.push(`/devices/${deviceId}`);
}

const isUpdateDialogOpen = ref(false);
const deviceToUpdate = ref<string>();
function openUpdateDialog(deviceId: string) {
  deviceToUpdate.value = deviceId;
  isUpdateDialogOpen.value = true;
}

const shareDialog = ref(false);
const deviceToShare = ref<string>();
function openShareDialog(deviceId: string) {
  shareDialog.value = true;
  deviceToShare.value = deviceId;
}

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

<style lang="scss" scoped>
.devices-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.device-card {
  cursor: pointer;
  width: 100%;

  &__section {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  &__status {
    font-size: 14px;
  }
}
</style>
