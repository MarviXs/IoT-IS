<template>
  <div>
    <div class="text-weight-medium text-h6">{{ t('device.sensors') }}</div>
    <div v-if="sensorsTree.children" class="column q-mt-sm q-gutter-y-sm">
      <q-tree
        v-model:ticked="tickedNodes"
        v-model:expanded="expanded"
        :nodes="sensorsTree.children"
        node-key="id"
        label-key="name"
        tick-strategy="leaf"
        :no-nodes-label="t('device.no_sensors')"
        default-expand-all
        :class="noChildren ? 'no-children' : ''"
      >
        <template #default-header="prop">
          <div
            v-if="prop.node.dataPointTag"
            class="text-weight-medium text-primary cursor-pointer"
            @click="prop.ticked = !prop.ticked"
            @mousedown.prevent
          >
            <!-- eslint-disable-next-line @intlify/vue-i18n/no-raw-text -->
            {{ prop.node.name }} ({{ prop.node.dataPointTag.unit }})
          </div>
          <div v-else>
            {{ prop.node.name }}
          </div>
        </template>
      </q-tree>
    </div>
  </div>
</template>

<script setup lang="ts">
import { SensorNode } from '@/models/SensorNode';
import { extractNodeKeys } from '@/utils/sensor-nodes';
import { computed } from 'vue';
import { PropType, ref } from 'vue';
import { useI18n } from 'vue-i18n';

const tickedNodes = defineModel<string[]>('tickedNodes');
const props = defineProps({
  sensorsTree: {
    type: Object as PropType<SensorNode>,
    required: true,
  },
});

const { t } = useI18n();

const expanded = ref<string[]>(extractNodeKeys(props.sensorsTree));

const noChildren = computed(() => {
  return props.sensorsTree.children?.every((node) => node.sensor) ?? false;
});
</script>

<style lang="scss">
.no-children {
  .q-tree__node-header {
    padding-left: 0 !important;
    padding-bottom: 0.5rem;
  }
}
</style>
