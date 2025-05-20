<template>
  <PageLayout :breadcrumbs="[{ label: t('lifecycle.label', 2) }]">
    <template #actions>
      <q-btn
        class="shadow"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('lifecycle.add_plant')"
        :icon="mdiPlus"
        to="/lifecycle/analyze"
      />
      <q-btn
        class="shadow"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('lifecycle.add_plants')"
        :icon="mdiPlus"
        to="/lifecycle/analyze_more"
      />
    </template>
    <template #default>
      <q-table
        :rows="collections"
        :loading="isLoadingCollections"
        flat
        :rows-per-page-options="[10, 20, 50]"
        class="shadow"
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
        :rows-per-page-label="t('table.rows_per_page_label')"
        row-key="id"
        :columns="columns"
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiHubspot" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>
        <template #body-cell-plantBoardId="props">
          <q-td :props="props">
            <span
              @click="onPlantBoardClick(props.row)"
              class="cursor-pointer text-blue"
            >
              {{ props.row.plantBoardId }}
            </span>
          </q-td>
        </template>
        <template #body-cell-name="props">
          <q-td :props="props">
            <q-tree
              v-model:selected="selected"
              v-model:expanded="expandedNodes"
              :nodes="[props.row]"
              selected-color="black"
              node-key="id"
              label-key="plantBoardId"
              children-key="items"
            >
              <template #default-header="treeProp">
                <q-space></q-space>
                <q-btn :icon="mdiDotsVertical" color="grey-color" flat round>
                  <q-menu anchor="bottom right" self="top right">
                    <q-list>
                      <q-item v-close-popup clickable @click="addRecord(treeProp.node.plantBoardId)">
                        <div class="row items-center q-gutter-sm">
                          <q-icon color="grey-9" size="24px" :name="mdiSeedPlusOutline" />
                          <div>{{ t('lifecycle.add_record') }}</div>
                        </div>
                      </q-item>
                      <q-item v-close-popup clickable>
                        <div class="row items-center q-gutter-sm">
                          <q-icon color="grey-9" size="24px" :name="mdiPencil" />
                          <div>{{ t('lifecycle.edit') }}</div>
                        </div>
                      </q-item>
                      <q-item v-close-popup clickable @click="deleteLifecycle(treeProp.node.plantBoardId)">
                        <div class="row items-center q-gutter-sm">
                          <q-icon color="grey-9" size="24px" :name="mdiTrashCan" />
                          <div>{{ t('lifecycle.delete') }}</div>
                        </div>
                      </q-item>
                    </q-list>
                  </q-menu>
                </q-btn>
              </template>
            </q-tree>
          </q-td>
        </template>
      </q-table>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { computed, ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import {
  mdiHubspot,
  mdiPlus,
  mdiChartLine,
  mdiPencil,
  mdiTrashCan,
  mdiDotsVertical,
  mdiSeedPlusOutline
} from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import LifeCycleService from '@/api/services/LifeCycleService';

const { t } = useI18n();
const router = useRouter();

const selected = ref('');
const expandedNodes = ref<string[]>([]);
const isLoadingCollections = ref(false);
const collections = ref<CollectionNode[]>([]);

const columns = computed(() => [  
  {
    name: 'plantBoardId',
    label: t('lifecycle.plant_board_id'),
    field: 'plantBoardId',
    required: true,
    style: 'text-align: left;',
    headerStyle: 'text-align: center;'
  }, 
  {
    name: 'name',
    required: true,
    label: '',
    align: 'left',
    field: '',
  }, 
  {
    name: 'rows',
    label: t('lifecycle.rows'),
    field: 'rows',
    required: true,
  },
  {
    name: 'cols',
    label: t('lifecycle.columns'),
    field: 'cols',
    required: true
  },
  {
    name: 'createdAt',
    label: t('lifecycle.create_date'),
    field: 'createdAt',
    required: true
  },
]);

async function loadCollections() {
  try {
    isLoadingCollections.value = true;
    const queryParams = {}; // Parametre
    const response = await LifeCycleService.getPlantBoards(queryParams);

    collections.value = response.data?.items.map(item => ({
      id: item.id,
      plantBoardId: item.plantBoardId,
      name: item.id,
      rows: item.rows,
      cols: item.cols,
      createdAt: item.createdAt.substring(0, 10),
    })) || [];
  } catch (error) {
    console.error('Chyba pri načítavaní životných cyklov:', error);
  } finally {
    isLoadingCollections.value = false;
  }
}

function navigateToLifecycle(id: string) {
  router.push(`/lifeboard/${id}`);
}

function onRowClick(row: CollectionNode) {
  router.push(`/lifeboard/${row.id}`);
}

function onPlantBoardClick(row: CollectionNode) {
  router.push(`/lifeboard/${row.plantBoardId}`);
}

interface CollectionNode {
  id: string;
  plantBoardId: string;
  name: string;
  rows: number;
  cols: number;
  createdAt: string;
}

// Načítanie dát pri mountovaní komponentu
onMounted(() => {
  loadCollections();
});

async function deleteLifecycle(id: string) {
  try {
    await LifeCycleService.deletePlantboard(id);
    await loadCollections();
  } catch (error) {
    console.error('Chyba pri odstraňovaní životného cyklu:', error);
  }
}

function addRecord(id: string) {
  router.push(`lifecycle/analyze/${id}`);
}
</script>

<style scoped>
.cursor-pointer {
  cursor: pointer;
}
.text-blue {
  color: blue;
}
</style>
