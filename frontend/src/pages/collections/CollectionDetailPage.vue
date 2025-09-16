<template>
  <PageLayout
    :breadcrumbs="[{ label: 'Collections', to: '/collections' }, { label: collection?.name || 'Collection' }]"
  >
    <div class="row q-col-gutter-x-lg q-col-gutter-y-lg justify-between">
      <div class="col-12">
        <sensor-selection-tree
          v-model:tickedNodes="tickedNodes"
          :sensors-tree="sensorTree"
          class="shadow container full-height"
        ></sensor-selection-tree>
      </div>
      <div class="col-12">
        <DataPointChart
          v-if="sensorTree"
          ref="dataPointChart"
          v-model:tickedNodes="tickedNodes"
          class="bg-white shadow q-pa-lg"
          :sensors="sensors"
          @refresh="getCollection"
        ></DataPointChart>
      </div>
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import SensorSelectionTree from '@/components/datapoints/SensorSelectionTree.vue';
import CollectionService from '@/api/services/DeviceCollectionService';
import { computed, ref } from 'vue';
import { collectionToTreeNode, extractNodeKeys, treeNodeToSensors } from '@/utils/sensor-nodes';
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import type { SensorNode } from '@/models/SensorNode';
import type { DeviceCollectionWithSensorsResponse } from '@/api/services/DeviceCollectionService';
import DataPointChart from '@/components/datapoints/DataPointChartJS.vue';

const { t } = useI18n();
const route = useRoute();

const tickedNodes = ref<string[]>();
const sensorTree = ref<SensorNode>();

const collection = ref<DeviceCollectionWithSensorsResponse>();

async function getCollection() {
  const { data, error } = await CollectionService.getCollectionWithSensors(route.params.id.toString());
  if (error) {
    console.error(error);
    return;
  }
  collection.value = data;
  setTree(data);
}
getCollection();

function setTree(collection: DeviceCollectionWithSensorsResponse) {
  sensorTree.value = collectionToTreeNode(collection);
  tickedNodes.value = extractNodeKeys(sensorTree.value);
}

const sensors = computed(() => {
  if (!collection.value || !sensorTree.value) {
    return [];
  }
  return treeNodeToSensors(sensorTree.value);
});
</script>

<style lang="scss" scoped></style>
