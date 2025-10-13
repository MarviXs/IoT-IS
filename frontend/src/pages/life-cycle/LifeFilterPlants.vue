<template>
  <PageLayout :breadcrumbs="[{ label: t('lifecycle.label', 1) }]">
    <template #actions>
      <q-btn
      class="shadow"
      color="primary"
      unelevated
      no-caps
      size="15px"
      :label="t('lifecycle.add_plants')"
      :icon="mdiPlus"
      @click="navigateToAnalyzeMore"
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
        v-model:pagination="pagination"
        :columns="columns"
        @request="onRequest"
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
                          <div>{{ t('lifecycle.add_record') }}</div>
                        </div>
                      </q-item>
                      <q-item v-close-popup clickable>
                        <div class="row items-center q-gutter-sm">
                          <q-icon color="grey-9" size="24px" :name="mdiPencil" />
                          <div>{{ t('lifecycle.edit') }}</div>
                        </div>
                      </q-item>
                      <q-item v-close-popup clickable @click="deleteLifecycle(treeProp.node.id)">
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
import { computed, ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
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
import type { PlantsCollectionQueryParams } from '@/api/services/LifeCycleService';
import LifeCycleService from '@/api/services/LifeCycleService';
import type { PaginationClient, PaginationTable } from '@/models/Pagination';
import { watch } from 'vue';

const { t } = useI18n();

const pagination = ref<PaginationClient>({
  sortBy: 'name',
  descending: false,
  page: 1,
  rowsPerPage: 20,
  rowsNumber: 0,
});

watch(pagination, (newPagination) => {
  console.log('Paginácia sa zmenila:', newPagination);
  loadCollections(newPagination);
}, { immediate: true });

const router = useRouter();
const route = useRoute();

const selected = ref('');
const expandedNodes = ref<string[]>([]);
const isLoadingCollections = ref(false);
const collections = ref<CollectionNode[]>([]);
const plantId = route.params.id;

const columns = computed(() => [
{
    name: 'plantId',
    label: t('lifecycle.plant_id'),
    field: 'plantId',
    required: true,
    style: 'text-align: left;',
    headerStyle: 'text-align: center;'
  },  
{
    name: 'name',
    label: t('lifecycle.plant_name'),
    field: 'name',
    required: true,
    headerStyle: 'text-align: center;'
  },
  {
    name: 'type',
    label: t('lifecycle.plant_type'),
    field: 'type',
    required: true
  },
  {
    name: 'days',
    label: t('lifecycle.days'),
    field: 'days',
    required: true
  },
]);

async function loadCollections(paginationTable: PaginationTable) {
  const paginationQuery: PlantsCollectionQueryParams = {
    SortBy: paginationTable.sortBy,
    Descending: paginationTable.descending,
    PageNumber: paginationTable.page,
    PageSize: paginationTable.rowsPerPage,
  };
  try {
    isLoadingCollections.value = true;
    const response = await LifeCycleService.getPlantsByBoard(plantId.toString(), paginationQuery);

    collections.value = response.data?.items.map(item => ({
      id: item.id,
      plantId: item.plantId,
      name: item.name,
      type: item.type,
      days: calculateDays(item.datePlanted),
    })) || [];

    
    pagination.value.sortBy = paginationTable.sortBy;
    pagination.value.descending = paginationTable.descending;
    pagination.value.page = (response.data?.currentPage ?? 1);
    pagination.value.rowsPerPage = response.data?.pageSize ?? 20;
    pagination.value.rowsNumber = response.data?.totalCount ?? 0;
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
  loadCollections(pagination.value);
});

async function deleteLifecycle(id: string) {
  try {
    await LifeCycleService.deletePlant(id);
    await loadCollections(pagination.value);
  } catch (error) {
    console.error('Chyba pri odstraňovaní životného cyklu:', error);
  }
}

function addRecord(id: string) {
  router.push(`lifecycle/analyze/${id}`);
}

function navigateToAnalyzeMore() {
  if (plantId !== undefined) {
    router.push(`/lifecycle/analyze_more/${plantId.toString()}`);
  } else {
    console.error('Plant ID not found');
  }
}

async function onRequest(props: { pagination: PaginationTable }) {
  await loadCollections(props.pagination);
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
