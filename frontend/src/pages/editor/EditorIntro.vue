<template>
    <PageLayout :breadcrumbs="[{ label: t('greenhouse.label', 2) }]">
      <template #actions>
        <q-btn
          class="shadow"
          color="primary"
          unelevated
          no-caps
          size="15px"
          :label="t('greenhouse.add_greenhouse')"
          :icon="mdiPlus"
          to="/editor"
        />
      </template>
  
      <template #default>
        <q-table
          :rows="greenhouses"
          :loading="isLoadingGreenhouses"
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
              <q-icon :name="mdiHubspot" class="q-mb-md" size="50px" />
              {{ message }}
            </div>
          </template>
  
          <template #body-cell-name="props">
            <q-td :props="props" @click="onRowClick(props.row)">
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
                    @click="navigateToGreenhouse(treeProp.node.id)"
                    color="blue"
                    size="1.2rem"
                  />
                  <span
                    @click="navigateToGreenhouse(treeProp.node.id)"
                    class="cursor-pointer text-blue"
                  >
                    {{ treeProp.node.name }}
                  </span>
                  <q-space />
                  <q-btn :icon="mdiDotsVertical" color="grey-color" flat round>
                    <q-menu anchor="bottom right" self="top right">
                      <q-list>
                        <q-item v-close-popup clickable>
                          <div class="row items-center q-gutter-sm">
                            <q-icon color="grey-9" size="24px" :name="mdiPencil" />
                            <div>{{ t('greenhouse.edit') }}</div>
                          </div>
                        </q-item>
                        <q-item v-close-popup clickable @click="deleteGreenhouse(treeProp.node.id)">
                          <div class="row items-center q-gutter-sm">
                            <q-icon color="grey-9" size="24px" :name="mdiTrashCan" />
                            <div>{{ t('greenhouse.delete') }}</div>
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
import GreenHouseService from '@/api/services/GreenHouseService';

const { t } = useI18n();
const router = useRouter();

const selected = ref('');
const expandedNodes = ref<string[]>([]);
const isLoadingGreenhouses = ref(false);
const greenhouses = ref<GreenhouseNode[]>([]);

const columns = computed(() => [
  {
    name: 'name',
    label: t('greenhouse.name'),
    field: 'name',
    required: true,
    style: 'text-align: left;',
    headerStyle: 'text-align: center;',
  },
  {
    name: 'width',
    label: t('greenhouse.width'),
    field: 'width',
    required: true,
  },
  {
    name: 'height',
    label: t('greenhouse.height'),
    field: 'height',
    required: true,
  },
  {
    name: 'createdAt',
    label: t('greenhouse.created_at'),
    field: 'createdAt',
    required: true,
  },
]);

async function loadGreenhouses() {
  try {
    isLoadingGreenhouses.value = true;
    const response = await GreenHouseService.getGreenHouses({});
    greenhouses.value = response.data?.items.map(item => ({
      id: item.id,
      name: item.name,
      width: item.width / 1000,
      height: item.depth / 1000,
      createdAt: item.dateCreated.substring(0, 10),
    })) || [];
  } catch (error) {
    console.error('Chyba pri načítavaní skleníkov:', error);
  } finally {
    isLoadingGreenhouses.value = false;
  }
}

function navigateToGreenhouse(id: string) {
  router.push(`/editor/${id}`);
}

function onRowClick(row: GreenhouseNode) {
  //navigateToGreenhouse(row.id);
}

function addRecord(id: string) {
  router.push(`/greenhouses/${id}/add-record`);
}

async function deleteGreenhouse(id: string) {
  try {
    await GreenHouseService.deleteGreenHouse(id);
    await loadGreenhouses();
  } catch (error) {
    console.error('Chyba pri odstraňovaní sklenníka:', error);
  }
}

interface GreenhouseNode {
  id: string;
  name: string;
  width: number;
  height: number;
  createdAt: string;
}

onMounted(() => {
  loadGreenhouses();
});
</script>
  
  <style scoped>
  .cursor-pointer {
    cursor: pointer;
  }
  .text-blue {
    color: blue;
  }
  </style>
  