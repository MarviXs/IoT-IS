<template>
  <PageLayout :breadcrumbs="[{ label: t('collection.label', 2) }]">
    <template #actions>
      <q-btn
        class="shadow"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('collection.create_collection')"
        :icon="mdiPlus"
        @click="openCreateCollectionDialog('')"
      />
    </template>
    <template #default>
      <q-table
        :rows="collections"
        :columns="columns"
        :loading="isLoadingCollections"
        flat
        :rows-per-page-options="[10, 20, 50]"
        class="shadow"
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
        :rows-per-page-label="t('table.rows_per_page_label')"
        row-key="id"
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiHubspot" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>
        <template #body-cell-name="props">
          <q-td :props="props">
            <q-tree
              v-model:selected="selected"
              v-model:expanded="expandedNodes"
              :nodes="[props.row]"
              selected-color="black"
              node-key="id"
              label-key="name"
              children-key="items"
              @lazy-load="lazyLoadCollection"
            >
              <template #default-header="treeProp">
                <template v-if="treeProp.node.type == 'Device'">
                  <q-icon color="grey-color" class="q-ml-xs q-mr-xs" size="1.6rem" :name="mdiMemory" />
                  <span>{{ treeProp.node.name }}</span>
                  <q-space></q-space>
                  <q-btn
                    :icon="mdiChartLine"
                    flat
                    color="grey-color"
                    round
                    :to="`/devices/${treeProp.node.id}`"
                    @click.stop
                  />
                  <q-btn :icon="mdiDotsVertical" color="grey-color" flat round @click.stop>
                    <q-menu anchor="bottom right" self="top right">
                      <q-list>
                        <q-item v-close-popup clickable @click.stop="openRemoveDeviceDialog(treeProp.node.id)">
                          <div class="row items-center q-gutter-sm">
                            <q-icon color="grey-9" size="24px" :name="mdiTrashCan" />
                            <div>Remove</div>
                          </div>
                        </q-item>
                      </q-list>
                    </q-menu>
                  </q-btn>
                </template>
                <template v-else>
                  <q-btn :icon="mdiPlus" color="grey-color" dense flat round @click.stop>
                    <q-menu>
                      <q-list>
                        <q-item v-close-popup clickable @click="openAddDeviceDialog(treeProp.node.id)">
                          <div class="row items-center q-gutter-sm">
                            <div>Add Device</div>
                          </div>
                        </q-item>
                        <q-item v-close-popup clickable @click="openCreateCollectionDialog(treeProp.node.id)">
                          <div class="row items-center q-gutter-sm">
                            <div>Add Subcollection</div>
                          </div>
                        </q-item>
                      </q-list>
                    </q-menu>
                  </q-btn>
                  <span>{{ treeProp.node.name }}</span>
                  <q-space></q-space>
                  <q-btn
                    :icon="mdiChartLine"
                    flat
                    color="grey-color"
                    round
                    :to="`/collections/${treeProp.node.id}`"
                    @click.stop
                  />
                  <q-btn :icon="mdiDotsVertical" color="grey-color" flat round @click.stop>
                    <q-menu anchor="bottom right" self="top right">
                      <q-list>
                        <q-item v-close-popup clickable @click.stop="openUpdateCollectionDialog(treeProp.node.id)">
                          <div class="row items-center q-gutter-sm">
                            <q-icon color="grey-9" size="24px" :name="mdiPencil" />
                            <div>Edit</div>
                          </div>
                        </q-item>
                        <q-item v-close-popup clickable @click.stop="openDeleteCollectionDialog(treeProp.node.id)">
                          <div class="row items-center q-gutter-sm">
                            <q-icon color="grey-9" size="24px" :name="mdiTrashCan" />
                            <div>Delete</div>
                          </div>
                        </q-item>
                      </q-list>
                    </q-menu>
                  </q-btn>
                </template>
              </template>
            </q-tree>
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
  <CreateCollectionDialog
    v-model="isCreateCollectionDialogOpen"
    :collection-parent-id="createCollectionParentId"
    @on-create="addCollectionToParent"
  />
  <UpdateCollectionDialog
    v-model="isUpdateCollectionDialogOpen"
    :collection-id="updateCollectionId"
    @on-update="handleCollectionUpdated"
  />
  <DeleteCollectionDialog
    v-model="isDeleteCollectionDialogOpen"
    :collection-id="deleteCollectionId"
    @on-deleted="handleCollectionDeleted"
  />
  <AddDeviceToCollectionDialog
    v-model="isAddDeviceDialogOpen"
    :collection-parent-id="addDeviceCollectionId"
    @on-add="handleDeviceAdded"
  />
  <RemoveDeviceFromCollectionDialog
    v-model="isRemoveDeviceDialogOpen"
    :collection-id="removeDeviceCollectionId"
    :device-id="removeDeviceId"
    @on-deleted="handleCollectionDeleted"
  />
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import {
  mdiChartLine,
  mdiHubspot,
  mdiMemory,
  mdiPencil,
  mdiPlus,
  mdiTrashCan,
  mdiDotsVertical,
} from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import {
  CreateCollectionResponse,
  DeviceCollectionQueryParams,
  UpdateCollectionResponse,
} from '@/api/types/DeviceCollection';
import DeviceCollectionService from '@/api/services/DeviceCollectionService';
import { handleError } from '@/utils/error-handler';
import CreateCollectionDialog from '@/components/collections/CreateCollectionDialog.vue';
import DeleteCollectionDialog from '@/components/collections/DeleteCollectionDialog.vue';
import UpdateCollectionDialog from '@/components/collections/UpdateCollectionDialog.vue';
import AddDeviceToCollectionDialog from '@/components/collections/AddDeviceToCollectionDialog.vue';
import RemoveDeviceFromCollectionDialog from '@/components/collections/RemoveDeviceFromCollectionDialog.vue';

const { t } = useI18n();

const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});

const selected = ref('');
const expandedNodes = ref<string[]>([]);
const collections = ref<CollectionNode[]>([]);
const isLoadingCollections = ref(false);

interface CollectionNode {
  id: string;
  name: string;
  items: CollectionNode[];
  type: 'Device' | 'SubCollection';
  lazy: boolean;
}

async function getCollections(paginationTable: PaginationTable) {
  const paginationQuery: DeviceCollectionQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };

  isLoadingCollections.value = true;

  const { data, error } = await DeviceCollectionService.getCollections(paginationQuery);
  isLoadingCollections.value = false;

  if (error) {
    handleError(error, 'Loading templates failed');
    return;
  }

  collections.value = data.items.map((item) => ({
    id: item.id,
    name: item.name,
    lazy: true,
    selectable: false,
    type: 'SubCollection',
    items: [],
  }));
  pagination.value.sortBy = paginationTable.sortBy;
  pagination.value.descending = paginationTable.descending;
  pagination.value.page = data.currentPage;
  pagination.value.rowsPerPage = data.pageSize;
  pagination.value.rowsNumber = data.totalCount ?? 0;
}
getCollections(pagination.value);

async function lazyLoadCollection({
  node,
  key,
  done,
  fail,
}: {
  node: CollectionNode;
  key: string | number;
  done: (children: CollectionNode[]) => void;
  fail: (error: unknown) => void;
}) {
  const { data, error } = await DeviceCollectionService.getCollection(node.id, 1);

  if (error) {
    fail(error);
    handleError(error, 'Loading collection failed');
    return;
  }

  const items = data.items.map((item) => ({
    id: item.id,
    name: item.name,
    type: item.type,
    items: [],
    selectable: item.type === 'Device',
    lazy: item.type === 'SubCollection',
  }));

  done(items);
}

// Create collection
const isCreateCollectionDialogOpen = ref(false);
const createCollectionParentId = ref('');
function openCreateCollectionDialog(collectionId: string) {
  createCollectionParentId.value = collectionId;
  isCreateCollectionDialogOpen.value = true;
}
async function addCollectionToParent(collection: CreateCollectionResponse, parentId: string) {
  const parent = findItem(parentId, collections.value, false);

  if (parent) {
    parent.items.push({
      id: collection.id,
      name: collection.name,
      type: 'SubCollection',
      items: [],
      lazy: false,
    });
  } else {
    collections.value.push({
      id: collection.id,
      name: collection.name,
      type: 'SubCollection',
      items: [],
      lazy: false,
    });
  }
}

// Update collection
const isUpdateCollectionDialogOpen = ref(false);
const updateCollectionId = ref('');
function openUpdateCollectionDialog(collectionId: string) {
  updateCollectionId.value = collectionId;
  isUpdateCollectionDialogOpen.value = true;
}
async function handleCollectionUpdated(collection: UpdateCollectionResponse) {
  const item = findItem(collection.id, collections.value, false);
  if (item) {
    item.name = collection.name;
  }
}

// Delete collection
const isDeleteCollectionDialogOpen = ref(false);
const deleteCollectionId = ref('');
function openDeleteCollectionDialog(collectionId: string) {
  deleteCollectionId.value = collectionId;
  isDeleteCollectionDialogOpen.value = true;
}
async function handleCollectionDeleted(collectionId: string) {
  const parent = findItem(collectionId, collections.value, true);
  if (parent) {
    parent.items = parent.items.filter((item) => item.id !== collectionId);
  } else {
    collections.value = collections.value.filter((item) => item.id !== collectionId);
  }
}

// Add device to collection
const isAddDeviceDialogOpen = ref(false);
const addDeviceCollectionId = ref('');
function openAddDeviceDialog(collectionId: string) {
  addDeviceCollectionId.value = collectionId;
  isAddDeviceDialogOpen.value = true;
}
async function handleDeviceAdded(collectionId: string, deviceId: string) {
  const parent = findItem(collectionId, collections.value, false);
  const { data, error } = await DeviceCollectionService.getCollection(collectionId, 1);
  if (parent) {
    const device = data?.items.find((item) => item.id === deviceId);
    if (device) {
      parent.items.push({
        id: device.id,
        name: device.name,
        type: device.type,
        items: [],
        lazy: false,
      });
    }
  }
}

// Remove device from collection
const isRemoveDeviceDialogOpen = ref(false);
const removeDeviceCollectionId = ref('');
const removeDeviceId = ref('');
function openRemoveDeviceDialog(deviceId: string) {
  const collectionId = findItem(deviceId, collections.value, true)?.id;
  if (!collectionId) {
    return;
  }

  removeDeviceCollectionId.value = collectionId;
  removeDeviceId.value = deviceId;
  isRemoveDeviceDialogOpen.value = true;
}

const findItem = (id: string, items: CollectionNode[], findParent = false): CollectionNode | null => {
  for (const item of items) {
    if (findParent) {
      if (item.items.some((child) => child.id === id)) {
        return item;
      } else {
        const parent = findItem(id, item.items, true);
        if (parent) {
          return parent;
        }
      }
    } else {
      if (item.id === id) {
        return item;
      } else {
        const found = findItem(id, item.items, false);
        if (found) {
          return found;
        }
      }
    }
  }
  return null;
};

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'name',
    label: t('global.name'),
    field: 'name',
    sortable: true,
    align: 'left',
  },
]);
</script>

<style lang="scss" scoped></style>
