<template>
  <PageLayout :breadcrumbs="[{ label: 'Plants Overview' }]">
    <template #actions>
      <q-btn
        class="shadow"
        color="primary"
        text-color="white"
        unelevated
        no-caps
        size="15px"
        :label="'Add Record'"
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
      </q-table>
    </template>
  </PageLayout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import LifeCycleService from '@/api/services/LifeCycleService';
import PageLayout from '@/layouts/PageLayout.vue';
import { mdiPlus } from '@quasar/extras/mdi-v7';

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
}[]>([]);

// Table columns
const columns = ref([
  { name: 'id', required: true, label: 'Plant ID', align: 'left', field: 'plantId', sortable: true },
  { name: 'health', required: true, label: 'Health', align: 'left', field: 'health', sortable: true },
  { name: 'leafCount', required: true, label: 'Leaf Count', align: 'right', field: 'leafCount', sortable: true },
  { name: 'date', required: true, label: 'Date', align: 'left', field: 'analysisDate', sortable: true },
  { name: 'progress', required: true, label: 'Progress', align: 'left', field: 'progress', sortable: false },
]);

function formatDate(date: string | null): string {
  if (!date) return 'Invalid date';
  const parsedDate = new Date(date);
  if (isNaN(parsedDate.getTime())) {
    return 'Invalid date';
  }
  
  const options: Intl.DateTimeFormatOptions = { year: 'numeric', month: 'long', day: 'numeric' };
  return parsedDate.toLocaleDateString('en-US', options);
}

const route = useRoute();
const plantId = route.params.id;

const router = useRouter();
let routerPID: string | undefined;


onMounted(async () => {
  try {
    const response = await LifeCycleService.getLifeCyclesByPlantId(plantId as string);
    plants.value = response.data?.map((item) => ({
      ...item,
      analysisDate: item.analysisDate ? item.analysisDate : null,
    })) ?? [];
    routerPID = plants.value[0].plantId;
  } catch (error) {
    console.error('Error fetching plants data:', error);
  }
});

function addRecord() {
  router.push(`analyze/${routerPID}`);
}
</script>

<style scoped>
.q-btn.flat {
  margin: 0;
  padding: 0.5rem 1rem;
}
</style>
