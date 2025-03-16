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
            >
              <template #default-header="treeProp">
                <q-icon
                  :name="mdiChartLine"
                  class="q-mr-sm"
                  @click="navigateToLifecycle(treeProp.node.id)"
                  color="blue"
                  size="1.2rem"
                />
                <span
                  @click="navigateToLifecycle(treeProp.node.id)"
                  class="cursor-pointer text-blue"
                >
                  {{ treeProp.node.name }}
                </span>
                <q-space></q-space>
                <q-btn :icon="mdiDotsVertical" color="grey-color" flat round>
                  <q-menu anchor="bottom right" self="top right">
                    <q-list>
                      <q-item v-close-popup clickable @click="addRecord(treeProp.node.plantBoardId)">
                        <div class="row items-center q-gutter-sm">
                          <q-icon color="grey-9" size="24px" :name="mdiSeedPlusOutline" />
                          <div>Add record</div>
                        </div>
                      </q-item>
                      <q-item v-close-popup clickable>
                        <div class="row items-center q-gutter-sm">
                          <q-icon color="grey-9" size="24px" :name="mdiPencil" />
                          <div>Edit</div>
                        </div>
                      </q-item>
                      <q-item v-close-popup clickable @click="deleteLifecycle(treeProp.node.id)">
                        <div class="row items-center q-gutter-sm">
                          <q-icon color="grey-9" size="24px" :name="mdiTrashCan" />
                          <div>Delete</div>
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
    label: t('Planting board ID'),
    field: 'plantBoardId',
    required: true,
    style: 'text-align: left;',
    headerStyle: 'text-align: center;'
  },  
{
    name: 'rows',
    label: t('Board Rows'),
    field: 'rows',
    required: true,
    headerStyle: 'text-align: center;'
  },
  {
    name: 'cols',
    label: t('Board Columns'),
    field: 'cols',
    required: true
  },
  {
    name: 'createdAt',
    label: t('Created date'),
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

function calculateDays(datePlanted: string): number {
  const date = new Date(datePlanted);
  const now = new Date();
  const diffTime = Math.abs(now.getTime() - date.getTime());
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24)); // Konvertujte na dni
}

function navigateToLifecycle(id: string) {
  router.push(`/lifecycle/${id}`);
}

interface CollectionNode {
  id: string;
  plantBoardId: string;
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
    await LifeCycleService.deletePlant(id);
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
