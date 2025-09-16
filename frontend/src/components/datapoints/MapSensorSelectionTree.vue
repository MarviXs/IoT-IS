<template>
  <div>
    <q-expansion-item switch-toggle-side>
      <template #header>
        <div class="text-weight-medium text-subtitle1 flex items-center">{{ t('device.sensors') }}</div>
      </template>
      <div v-if="sensorsTree.items" class="column q-gutter-y-sm q-px-lg q-pb-md q-pt-md">
        <q-tree
          v-model:expanded="expanded"
          :nodes="[sensorsTree]"
          node-key="id"
          label-key="name"
          children-key="items"
          :no-nodes-label="t('device.no_sensors')"
          default-expand-all
          :class="noChildren ? 'no-children' : ''"
        >
          <template #default-header="prop">
            <div v-if="prop.node.type == 'Sensor'" class="flex items-center">
              <q-radio :val="prop.node.id" v-model="selectedSensorLocal" color="primary" @click.stop class="q-mr-sm" />
              <span class="text-weight-medium text-primary cursor-pointer">
                {{ prop.node.name }} ({{ prop.node.sensor.unit }})
              </span>
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
import type { SensorNode } from '@/models/SensorNode';
import { extractNodeKeys } from '@/utils/sensor-nodes';
import { computed, ref, watch } from 'vue';
import type { PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiMemory } from '@quasar/extras/mdi-v7';

const selectedSensor = defineModel<string | null>('selectedSensor');
const selectedSensorLocal = ref<string | null>(selectedSensor.value ?? null);

// Keep local and parent model in sync
watch(selectedSensorLocal, (val) => {
  selectedSensor.value = val;
});
watch(selectedSensor, (val) => {
  selectedSensorLocal.value = val;
});
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
