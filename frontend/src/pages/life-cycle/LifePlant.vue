<template>
  <PageLayout :breadcrumbs="[{ label: t('lifecycle.label') }]">
    <template #actions>
      <q-btn
        class="shadow"
        color="primary"
        text-color="white"
        unelevated
        no-caps
        size="15px"
        :label="t('lifecycle.add_record')"
        :icon="mdiPlus"
        @click="addRecord()"
      />
    </template>
    <template #default>
      <q-table
        :rows="plants"
        :columns="columns"
        row-key="id"
        flat
        bordered
        class="bg-white shadow"
      >
        <template v-slot:body-cell-date="props">
          <q-td :props="props">
            {{ formatDate(props.row.analysisDate) }}
          </q-td>
        </template>
        <template v-slot:body-cell-progress="props">
          <q-td :props="props">
            <q-linear-progress
              :value="props.row.progress / 100"
              size="16px"
              color="green"
              track-color="grey-3"
              rounded
            >
              {{ props.row.progress }}%
            </q-linear-progress>
          </q-td>
        </template>
        <template v-slot:body="props">
          <q-tr :props="props" @click="logImageName(props.row)">
            <q-td v-for="col in columns" :key="col.name">
              {{ typeof col.field === 'function' ? col.field(props.row) : props.row[col.field] }}
            </q-td>
          </q-tr>
        </template>
      </q-table>

      <q-dialog v-model="showImageDialog">
      <q-card style="width: 100%; ">
        <q-card-section>
          <div class="text-h6">{{ t('lifecycle.plant_image') }}</div>
        </q-card-section>
        <q-card-section>
          <q-img v-if="plantImage" style="width: 100%; " :src="plantImage" fit="contain">
            <div
              v-if="showImageDialog"
              class="square-overlay"
              :style="{ top: `${squareTop}px`, left: `${squareLeft}px`, width: '50px', height: '50px' }"
            ></div>
          </q-img>
          <div v-else class="text-red">{{ t('lifecycle.image_not_found') }}</div>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat label="Close" color="primary" @click="showImageDialog = false" />
        </q-card-actions>
      </q-card>
    </q-dialog>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import LifeCycleService from '@/api/services/LifeCycleService';
import PageLayout from '@/layouts/PageLayout.vue';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import { useI18n } from 'vue-i18n';
import axios from 'axios';
import { CONFIG } from '@/config';
import type { QTableProps } from 'quasar';

const { t } = useI18n();
const showImageDialog = ref(false);
const plantImage = ref<string | null>(null);

// Breadcrumbs for navigation
const breadcrumbs = ref([
  { label: 'Home', to: '/' },
  { label: 'Plants', to: '/plants' },
]);

const plants = ref<{
  plantId: string;
  leafCount?: number | null;
  width?: number | null;
  height?: number | null;
  area?: number | null;
  disease?: string | null;
  health?: string | null;
  analysisDate?: string | null;
  imageName?: string | null;
}[]>([]);

// Table columns
const columns = ref<QTableProps['columns']>([
  { name: 'id', required: true, label: t('lifecycle.plant_id'), align: 'left', field: 'plantId', sortable: true },
  { name: 'disease', required: true, label: t('lifecycle.disease'), align: 'left', field: 'disease', sortable: true },
  { name: 'area', required: true, label: t('lifecycle.size'), align: 'left', field: 'area', sortable: true },
  { name: 'leafCount', required: true, label: t('lifecycle.leafcount'), align: 'left', field: 'leafCount', sortable: true },
  { name: 'date', required: true, label: t('lifecycle.date'), align: 'left', field: 'analysisDate', sortable: true },
  { name: 'progress', required: true, label: t('lifecycle.progress'), align: 'left', field: 'progress', sortable: false },
]);

function formatDate(date: string | null): string {
  if (!date) return 'Invalid date';
  const parsedDate = new Date(date);
  if (isNaN(parsedDate.getTime())) {
    return 'Invalid date';
  }
  
  const year = parsedDate.getUTCFullYear();
  const month = String(parsedDate.getUTCMonth() + 1).padStart(2, '0'); // Mesiace sú od 0 do 11
  const day = String(parsedDate.getUTCDate()).padStart(2, '0');

  return `${year}/${month}/${day}`;
}

const route = useRoute();
const plantId = route.params.id;

const router = useRouter();
let routerPID: string | undefined;
let squareTop: number = 0;
let squareLeft: number = 0;


onMounted(async () => {
  try {
    const response = await LifeCycleService.getLifeCyclesByPlantId(plantId as string);
    plants.value = response.data?.map((item) => ({
      ...item,
      analysisDate: item.analysisDate ? formatDate(item.analysisDate) : null,
    })) ?? [];
    routerPID = plants.value[0].plantId;
  } catch (error) {
    console.error('Error fetching plants data:', error);
  }
});

function addRecord() {
  router.push(`analyze/${routerPID}`);
}

async function logImageName(row: any) {
  console.log('ImageName:', row.imageName);
  squareTop = row.height * 0.6 - 50;
  //squareTop = 100;
  squareLeft = row.width * 0.58 - 50;
  //squareLeft = 100;

  console.log('Square Top:', squareTop);
  console.log('Square Left:', squareLeft);

  if (!row.imageName) {
    console.warn('No image name available');
    return;
  }

  try {
    const response = await axios.get(`ht${CONFIG.API_BASE_URL}/image/${row.imageName}`);
    plantImage.value = response.data.plantImage; // Base64 string
    showImageDialog.value = true; // Open dialog
  } catch (error) {
    console.error('Error fetching image:', error);
    plantImage.value = null;
    showImageDialog.value = true;
  }
}
</script>

<style scoped>
.q-btn.flat {
  margin: 0;
  padding: 0.5rem 1rem;
}
.square-overlay {
  position: absolute;
  border: 3px solid yellow;
}
</style>
