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
        @click="isCreateCollectionDialogOpen = true"
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
                  <q-btn :icon="mdiChartLine" flat color="grey-color" round @click.stop="console.log('add')" />
                  <q-btn :icon="mdiPencil" flat color="grey-color" round @click.stop="console.log('add')" />
                  <q-btn :icon="mdiTrashCan" flat color="grey-color" round @click.stop="console.log('add')" />
                </template>
                <template v-else>
                  <q-btn dense :icon="mdiPlus" flat color="grey-color" round @click.stop="console.log('add')" />
                  <span>{{ treeProp.node.name }}</span>
                  <q-space></q-space>
                  <q-btn :icon="mdiChartLine" flat color="grey-color" round @click.stop="console.log('add')" />
                  <q-btn :icon="mdiPencil" flat color="grey-color" round @click.stop="console.log('add')" />
                  <q-btn
                    :icon="mdiTrashCan"
                    flat
                    color="grey-color"
                    round
                    @click.stop="openDeleteCollectionDialog(treeProp.node.id)"
                  />
                </template>
              </template>
            </q-tree>
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
  <CreateCollectionDialog v-model="isCreateCollectionDialogOpen" @on-create="addCollectionToParent" />
  <DeleteCollectionDialog
    v-model="isDeleteCollectionDialogOpen"
    :collection-id="deleteCollectionId"
    @on-deleted="handleCollectionDeleted"
  />
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiChartLine, mdiHubspot, mdiMemory, mdiPencil, mdiPlus, mdiTrashCan } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import { PaginationClient, PaginationTable } from '@/models/Pagination';
import { DeviceCollectionQueryParams } from '@/api/types/DeviceCollection';
import DeviceCollectionService from '@/api/services/DeviceCollectionService';
import { handleError } from '@/utils/error-handler';
import CreateCollectionDialog from '@/components/collections/CreateCollectionDialog.vue';
import DeleteCollectionDialog from '@/components/collections/DeleteCollectionDialog.vue';

const { t } = useI18n();
const isCreateCollectionDialogOpen = ref(false);

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

async function addCollectionToParent(collectionId: string) {
  const { data, error } = await DeviceCollectionService.getCollection(collectionId, 0);
  const parent = findItem(collectionId, collections.value);
  if (error) {
    return;
  }
  if (parent) {
    parent.items.push({
      id: data.id,
      name: data.name,
      items: [],
      lazy: true,
    });
  } else {
    collections.value.push({
      id: data.id,
      name: data.name,
      items: [],
      lazy: true,
    });
  }
}

const findItem = (id: string, items: CollectionNode[], findParent = false): CollectionNode | null => {
  for (const item of items) {
    if (item.id === id) return findParent ? null : item;
    const found = findItem(id, item.items, findParent);
    if (found) return findParent ? item : found;
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
