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
                      <q-item v-close-popup clickable @click="addRecord(treeProp.node.plantId)">
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
    name: 'plantId',
    label: t('Plant ID'),
    field: 'plantId',
    required: true,
    style: 'text-align: left;',
    headerStyle: 'text-align: center;'
  },  
{
    name: 'name',
    label: t('Plant Name'),
    field: 'name',
    required: true,
    headerStyle: 'text-align: center;'
  },
  {
    name: 'type',
    label: t('Plant Type'),
    field: 'type',
    required: true
  },
  {
    name: 'days',
    label: t('Days'),
    field: 'days',
    required: true
  },
]);

async function loadCollections() {
  try {
    isLoadingCollections.value = true;
    const queryParams = {}; // Parametre
    const response = await LifeCycleService.getLifeCycles(queryParams);

    collections.value = response.data?.items.map(item => ({
      id: item.id,
      plantId: item.plantId,
      name: item.name,
      type: item.type,
      days: calculateDays(item.datePlanted),
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
  plantId: string;
  name: string;
  type: string;
  days: number;
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
