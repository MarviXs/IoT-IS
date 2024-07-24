<template>
  <PageLayout
    v-if="dataPointTagTree"
    :title="dataPointTagTree.name"
    :previous-title="t('collection.label')"
    previous-route="/collections"
  >
    <div class="row q-col-gutter-x-lg q-col-gutter-y-xl justify-between">
      <div class="col-12 col-xl-3">
        <sensor-selection-tree
          v-model:tickedNodes="tickedNodes"
          :data-point-tag-tree="dataPointTagTree"
          class="shadow container q-pa-lg full-height"
        ></sensor-selection-tree>
      </div>
      <div class="col-12 col-xl-9">
        <data-point-chart
          v-model:tickedNodes="tickedNodes"
          :data-point-tags="dataPointTags"
          class="bg-white shadow q-pa-lg"
          @refresh="refresh"
        ></data-point-chart>
      </div>
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import SensorSelectionTree from '@/components/datapoints/SensorSelectionTree.vue';
import DataPointChart from '@/components/datapoints/DataPointChart.vue';
import CollectionService from '@/services/CollectionService';
import { ref } from 'vue';
import { DataPointTagNode } from '@/models/DataPointTagNode';
import { collectionToDataPointTagNode, extractNodeKeys, nodeToDataPointTags } from '@/utils/sensor-nodes';
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import PageLayout from '@/layouts/PageLayout.vue';
import { useAsyncData } from '@/composables/useAsyncData';
import { useCollectionStore } from '@/stores/collection-store';
import { Collection } from '@/models/Collection';

const { t } = useI18n();
const route = useRoute();
const store = useCollectionStore();

const tickedNodes = ref<string[]>();
const dataPointTagTree = ref<DataPointTagNode>();

const { refresh } = useAsyncData(getCollection, t('collection.toasts.load_failed'));

async function getCollection() {
  let collection = store.getModuleById(route.params.id.toString());
  if (collection) setTree(collection);

  collection = await CollectionService.getCollection(route.params.id.toString());
  setTree(collection);
  return collection;
}

function setTree(collection: Collection) {
  dataPointTagTree.value = collectionToDataPointTagNode(collection);
  tickedNodes.value = extractNodeKeys(dataPointTagTree.value);
}

const dataPointTags = computed(() => {
  if (!dataPointTagTree.value) {
    return [];
  }
  return nodeToDataPointTags(dataPointTagTree.value);
});
</script>

<style lang="scss" scoped></style>
