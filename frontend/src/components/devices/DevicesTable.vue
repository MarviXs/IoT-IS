<template>
  <div>
    <div v-if="isMobile">
      <div class="row items-center q-gutter-sm q-mb-sm">
        <q-select
          v-model="mobileSortBy"
          dense
          filled
          bg-color="white"
          emit-value
          map-options
          :options="mobileSortOptions"
          :label="t('global.sort_by')"
          class="col shadow"
        />
        <q-btn
          flat
          round
          :icon="mobileDescending ? mdiArrowDown : mdiArrowUp"
          :aria-label="t('global.sort_toggle')"
          @click="toggleMobileSortDirection"
          class="bg-white shadow"
        >
          <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ mobileDescending ? t('global.sort_descending') : t('global.sort_ascending') }}
          </q-tooltip>
        </q-btn>
      </div>
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
                <StatusDot :status="device.connectionState" />
                <span class="q-ml-sm">{{ getStatusLabel(device.connectionState) }}</span>
              </div>
              <div v-if="isSyncedFromHub(device)" class="device-card__synced row items-center text-caption text-grey-7">
                <q-icon :name="mdiLock" size="16px" class="q-mr-xs" />
                <span>{{ t('device.synced_from_hub') }}</span>
              </div>
              <div class="device-card__activity text-caption text-grey-7">
                {{ t('device.last_activity') }}: {{ formatTimeToDistance(device.lastSeen) }}
              </div>
              <div v-if="showOwner" class="device-card__owner text-caption text-grey-7">
                {{ t('global.owner') }}: {{ device.ownerEmail ?? '—' }}
              </div>
            </q-card-section>
            <q-menu context-menu touch-position>
              <q-list>
                <q-item
                  v-if="canMutateDevice(device)"
                  clickable
                  v-close-popup
                  @click.stop="openUpdateDialog(device.id)"
                >
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiPencil" />
                    <div>{{ t('global.edit') }}</div>
                  </div>
                </q-item>
                <q-item
                  v-if="canMutateDevice(device)"
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
                  v-if="canMutateDevice(device)"
                  clickable
                  v-close-popup
                  @click.stop="openShareDialog(device.id)"
                >
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiShare" />
                    <div>{{ t('device.share_device') }}</div>
                  </div>
                </q-item>
                <q-item v-else-if="canUnshareDevice(device)" clickable v-close-popup @click.stop="unshareDevice(device.id)">
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiShareOff" />
                    <div>{{ t('device.unshare_me') }}</div>
                  </div>
                </q-item>
                <q-item v-if="canChangeOwner(device)" clickable v-close-popup @click.stop="openChangeOwnerDialog(device)">
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiAccountSwitch" />
                    <div>{{ t('device.change_owner') }}</div>
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
          <q-badge v-if="isSyncedFromHub(propsCell.row)" class="q-ml-sm" color="grey-6" text-color="white">
            {{ t('device.synced_from_hub') }}
          </q-badge>
        </q-td>
      </template>

      <template #body-cell-lastActivity="propsContact">
        <q-td auto-width :props="propsContact">
          {{ formatTimeToDistance(propsContact.row.lastSeen) }}
          <q-tooltip v-if="propsContact.row.lastSeen" content-style="font-size: 11px" :offset="[0, 4]">
            {{ new Date(propsContact.row.lastSeen).toLocaleString() }}
          </q-tooltip>
        </q-td>
      </template>

      <template #body-cell-owner="propsOwner">
        <q-td :props="propsOwner">
          {{ propsOwner.row.ownerEmail ?? '—' }}
        </q-td>
      </template>

      <template #body-cell-status="propsStatus">
        <q-td auto-width :props="propsStatus">
          <StatusDot :status="propsStatus.row.connectionState" />
        </q-td>
      </template>

      <template #body-cell-actions="propsActions">
        <q-td auto-width :props="propsActions">
          <q-btn :icon="mdiChartLine" color="grey-color" flat round :to="`/devices/${propsActions.row.id}`"
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.open') }}
            </q-tooltip>
          </q-btn>
          <q-btn
            v-if="canMutateDevice(propsActions.row)"
            :icon="mdiPencil"
            color="grey-color"
            flat
            round
            @click.stop="openUpdateDialog(propsActions.row.id)"
            ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
              {{ t('global.edit') }}
            </q-tooltip>
          </q-btn>
          <q-btn v-if="hasMenuActions(propsActions.row)" :icon="mdiDotsVertical" color="grey-color" flat round>
            <q-menu anchor="bottom right" self="top right">
              <q-list>
                <q-item
                  v-if="canMutateDevice(propsActions.row)"
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
                  v-if="canMutateDevice(propsActions.row)"
                  v-close-popup
                  clickable
                  @click="openShareDialog(propsActions.row.id)"
                >
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiShare" />
                    <div>{{ t('device.share_device') }}</div>
                  </div>
                </q-item>
                <q-item v-else-if="canUnshareDevice(propsActions.row)" v-close-popup clickable @click="unshareDevice(propsActions.row.id)">
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiShareOff" />
                    <div>{{ t('device.unshare_me') }}</div>
                  </div>
                </q-item>
                <q-item v-if="canChangeOwner(propsActions.row)" v-close-popup clickable @click="openChangeOwnerDialog(propsActions.row)">
                  <div class="row items-center q-gutter-sm">
                    <q-icon color="grey-9" size="24px" :name="mdiAccountSwitch" />
                    <div>{{ t('device.change_owner') }}</div>
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
    <ChangeDeviceOwnerDialog
      v-if="deviceToChangeOwner"
      v-model="isChangeOwnerDialogOpen"
      :device-id="deviceToChangeOwner.deviceId"
      :current-owner-id="deviceToChangeOwner.ownerId"
      :current-owner-email="deviceToChangeOwner.ownerEmail"
      @on-changed="emit('onChange')"
    />
  </div>
</template>

<script setup lang="ts">
import { QInfiniteScroll, useQuasar, type QTableProps } from 'quasar';
import ShareDeviceDialog from './ShareDeviceDialog.vue';
import type { PropType } from 'vue';
import { computed, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import {
  mdiAccountSwitch,
  mdiArrowDown,
  mdiArrowUp,
  mdiCellphoneLink,
  mdiChartLine,
  mdiDotsVertical,
  mdiLock,
  mdiPencil,
  mdiShare,
  mdiShareOff,
  mdiTrashCan,
} from '@quasar/extras/mdi-v7';
import DeleteDeviceDialog from '@/components/devices/DeleteDeviceDialog.vue';
import { formatTimeToDistance } from '@/utils/date-utils';
import type { PaginationClient } from '@/models/Pagination';
import type { DevicesResponse } from '@/api/services/DeviceService';
import type { AdminDevicesResponse } from '@/api/services/AdminDeviceService';
import EditDeviceDialog from '@/components/devices/EditDeviceDialog.vue';
import StatusDot from './StatusDot.vue';
import ChangeDeviceOwnerDialog from '@/components/devices/ChangeDeviceOwnerDialog.vue';

type DeviceConnectionState = 'Online' | 'Degraded' | 'Offline';
type DeviceConnectionStateValue = DeviceConnectionState | 0 | 1 | 2;
import { useRouter } from 'vue-router';
import DeviceSharingService from '@/api/services/DeviceSharingService';
import { useAuthStore } from '@/stores/auth-store';
import { storeToRefs } from 'pinia';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';

type DeviceListItem = NonNullable<DevicesResponse['items']>[number] &
  Partial<NonNullable<AdminDevicesResponse['items']>[number]>;
type DeviceList = DeviceListItem[];
type DeviceListResponse =
  | (Omit<DevicesResponse, 'items'> & { items?: DeviceListItem[] })
  | (Omit<AdminDevicesResponse, 'items'> & { items?: DeviceListItem[] });

const props = defineProps({
  devices: {
    type: Object as PropType<DeviceListResponse | null>,
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
  adminView: {
    type: Boolean,
    required: false,
    default: false,
  },
});

const pagination = defineModel<PaginationClient>('pagination');

const devicesFiltered = computed<DeviceList>(() => props.devices?.items ?? []);

const emit = defineEmits(['onChange', 'onRequest', 'onLoadMore']);

const { t } = useI18n();
const $q = useQuasar();
const router = useRouter();
const isMobile = computed(() => $q.platform.is.mobile);
const showOwner = computed(() => props.adminView);

const authStore = useAuthStore();
const { user } = storeToRefs(authStore);

const mobileItems = computed(() => {
  if (isMobile.value) {
    return props.mobileDevices;
  }

  return devicesFiltered.value;
});

const mobileSortOptions = computed(() =>
  columns.value
    .filter((column) => column.sortable && column.name !== 'actions')
    .map((column) => ({
      label: column.label ?? column.name,
      value: column.name,
    })),
);

const mobileSortBy = computed({
  get: () => pagination.value?.sortBy ?? 'name',
  set: (value: string) => {
    if (!pagination.value || value === pagination.value.sortBy) {
      return;
    }

    pagination.value.sortBy = value;
    pagination.value.page = 1;
    emit('onRequest', { pagination: pagination.value });
  },
});

const mobileDescending = computed(() => pagination.value?.descending ?? false);

function toggleMobileSortDirection() {
  if (!pagination.value) {
    return;
  }

  pagination.value.descending = !pagination.value.descending;
  pagination.value.page = 1;
  emit('onRequest', { pagination: pagination.value });
}

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

async function unshareDevice(deviceId: string) {
  const email = user.value?.email;
  if (!email) {
    toast.error(t('device.toasts.share.unshare_failed'));
    return;
  }

  const { error } = await DeviceSharingService.unshareDevice({ email }, deviceId);

  if (error) {
    handleError(error, t('device.toasts.share.unshare_failed'));
    return;
  }

  toast.success(t('device.toasts.share.unshare_success'));
  emit('onChange');
}

const isChangeOwnerDialogOpen = ref(false);
const deviceToChangeOwner = ref<{
  deviceId: string;
  ownerId: string;
  ownerEmail?: string;
} | null>(null);

function openChangeOwnerDialog(device: DeviceListItem) {
  if (!canChangeOwner(device)) {
    return;
  }

  if (!device.ownerId) {
    return;
  }

  deviceToChangeOwner.value = {
    deviceId: device.id,
    ownerId: device.ownerId,
    ownerEmail: device.ownerEmail ?? undefined,
  };
  isChangeOwnerDialogOpen.value = true;
}

watch(isChangeOwnerDialogOpen, (open) => {
  if (!open) {
    deviceToChangeOwner.value = null;
  }
});

function getStatusLabel(state?: DeviceConnectionStateValue) {
  if (state === 1 || state === 'Degraded') {
    return t('device.degraded');
  }
  if (state === 0 || state === 'Online') {
    return t('device.connected');
  }
  return t('device.disconnected');
}

function isSyncedFromHub(device: DeviceListItem): boolean {
  return device.isSyncedFromHub === true;
}

function canMutateDevice(device: DeviceListItem): boolean {
  return !isSyncedFromHub(device) && (device.permission == 'Owner' || props.adminView);
}

function canUnshareDevice(device: DeviceListItem): boolean {
  return !isSyncedFromHub(device) && !props.adminView && device.permission !== 'Owner';
}

function canChangeOwner(device: DeviceListItem): boolean {
  return props.adminView && !isSyncedFromHub(device);
}

function hasMenuActions(device: DeviceListItem): boolean {
  return canMutateDevice(device) || canUnshareDevice(device) || canChangeOwner(device);
}

// const getPermissions = (device: Device) => {
//   if (DeviceService.isOwner(device)) {
//     return t('global.owner');
//   }
//   return t('global.shared');
// };

const columns = computed<QTableProps['columns']>(() => {
  const baseColumns: QTableProps['columns'][number][] = [
    {
      name: 'status',
      label: t('device.status'),
      field: 'connected',
      align: 'center',
      sortable: true,
    },
    {
      name: 'name',
      label: t('global.name'),
      field: 'name',
      sortable: true,
      align: 'left',
    },
  ];

  if (props.adminView) {
    baseColumns.push({
      name: 'owner',
      label: t('global.owner'),
      field: 'ownerEmail',
      align: 'left',
      sortable: false,
    });
  }

    baseColumns.push(
      {
        name: 'lastActivity',
        label: t('device.last_activity'),
        field: 'lastSeen',
        align: 'left',
        sortable: true,
      },
    {
      name: 'actions',
      label: '',
      field: '',
      align: 'center',
      sortable: false,
    },
  );

  return baseColumns;
});
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

  &__owner {
    margin-top: 4px;
  }

  &__synced {
    margin-top: 2px;
  }
}
</style>
