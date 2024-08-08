<template>
  <div>
    <q-expansion-item switch-toggle-side>
      <template #header>
        <div class="text-weight-medium text-subtitle1 flex items-center">{{ t('device.sensors') }}</div>
      </template>
      <div v-if="sensorsTree.items" class="column q-gutter-y-sm q-px-lg q-pb-md q-pt-md">
        <q-tree
          v-model:ticked="tickedNodes"
          v-model:expanded="expanded"
          :nodes="[sensorsTree]"
          node-key="id"
          label-key="name"
          children-key="items"
          tick-strategy="leaf"
          :no-nodes-label="t('device.no_sensors')"
          default-expand-all
          :class="noChildren ? 'no-children' : ''"
        >
          <template #default-header="prop">
            <div
              v-if="prop.node.type == 'Sensor'"
              class="text-weight-medium text-primary cursor-pointer"
              @click="prop.ticked = !prop.ticked"
              @mousedown.prevent
            >
              <!-- eslint-disable-next-line @intlify/vue-i18n/no-raw-text -->
              {{ prop.node.name }} ({{ prop.node.sensor.unit }})
            </div>
            <div v-else-if="prop.node.type == 'Device'" class="flex items-center">
              <q-icon color="grey-color" class="q-ml-xs q-mr-xs" size="1.5rem" :name="mdiMemory" />
              {{ prop.node.name }}
            </div>
            <div v-else>
              {{ prop.node.name }}
            </div>
          </template>
        </q-tree>
      </div>
    </q-expansion-item>
  </div>
</template>

<script setup lang="ts">
import { SensorNode } from '@/models/SensorNode';
import { extractNodeKeys } from '@/utils/sensor-nodes';
import { computed } from 'vue';
import { PropType, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiMemory } from '@quasar/extras/mdi-v7';

const tickedNodes = defineModel<string[]>('tickedNodes');
const props = defineProps({
  sensorsTree: {
    type: Object as PropType<SensorNode>,
    required: false,
    default: () => ({}) as SensorNode,
  },
});

const { t } = useI18n();

const expanded = ref<string[]>(extractNodeKeys(props.sensorsTree));

const noChildren = computed(() => {
  return props.sensorsTree.items?.every((node) => node.sensor) ?? false;
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
