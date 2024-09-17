<template>
  <div class="row q-col-gutter-xl">
    <div class="col-12 col-md-6">
      <div class="table-name text-secondary">{{ t('recipe.recipe_steps') }}</div>
      <VueDraggable
        v-if="recipe.steps"
        v-model="recipe.steps"
        :animation="200"
        handle=".handle"
        draggable="tr"
        target="tbody"
      >
        <q-table
          :rows="recipe.steps"
          :columns="stepColumns"
          flat
          :no-data-label="t('recipe.no_commands_added')"
          :loading-label="t('table.loading_label')"
          :rows-per-page-label="t('table.rows_per_page_label')"
          :rows-per-page-options="[0, 20, 50, 100]"
          hide-pagination
          class="shadow"
        >
          <template #header-cell-drag="propsDrag">
            <q-th auto-width :props="propsDrag" class="drag-th" />
          </template>

          <template #body-cell-drag="propsDrag">
            <q-td auto-width :props="propsDrag">
              <div>
                <q-icon :name="mdiDrag" size="28px" class="handle drag-icon" />
              </div>
            </q-td>
          </template>

          <template #body-cell-step="propsStep">
            <q-td :props="propsStep">
              <div v-if="!isStepSubrecipe(propsStep.row)">{{ propsStep.row.command.displayName }}</div>
              <div v-else>
                <q-tree :nodes="[subrecipeToNodes(propsStep.row, true)]" node-key="id" @lazy-load="lazyLoadSubrecipe">
                  <template #default-header="treeProp">
                    <span>{{ treeProp.node.label }}</span>
                    <template v-if="treeProp.node.cycles > 1 && !treeProp.node.root">
                      <span class="text-grey-8">&nbsp;({{ treeProp.node.cycles }}x)</span>
                    </template>
                  </template>
                </q-tree>
              </div>
            </q-td>
          </template>

          <template #body-cell-cycles="propsCycles">
            <q-td auto-width :props="propsCycles">
              <div>
                <q-input
                  v-model="propsCycles.row.cycles"
                  dense
                  outlined
                  type="number"
                  :min="1"
                  style="min-width: 65px"
                />
              </div>
            </q-td>
          </template>

          <template #body-cell-remove="propsRemove">
            <q-td auto-width :props="propsRemove">
              <div>
                <q-btn
                  dense
                  :icon="mdiClose"
                  color="red"
                  flat
                  round
                  :disable="props.loading"
                  @click="removeStep(propsRemove.row)"
                />
              </div>
            </q-td>
          </template>
        </q-table>
      </VueDraggable>
    </div>
    <div class="col-12 col-md-6">
      <div class="table-name text-secondary">
        {{ t('recipe.available_steps') }}
      </div>
      <q-tabs
        v-model="selectedStepType"
        inline-label
        no-caps
        align="justify"
        indicator-color="primary"
        active-color="primary"
        class="text-secondary bottom-outline bg-white rounded-top"
      >
        <q-tab name="commands" class="outline-bottom-grey" :icon="mdiCodeTags" :label="t('command.label', 2)" />
        <q-tab
          name="subrecipes"
          class="outline-bottom-grey"
          :icon="mdiBookMultipleOutline"
          :label="t('recipe.subrecipe', 2)"
        />
      </q-tabs>
      <CommandStepsTable v-if="selectedStepType === 'commands'" v-model="recipe" :device-template-id="deviceTemplateId">
      </CommandStepsTable>
      <SubrecipeStepsTable
        v-if="selectedStepType === 'subrecipes'"
        v-model="recipe"
        :device-template-id="deviceTemplateId"
      >
      </SubrecipeStepsTable>
    </div>
  </div>
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiClose, mdiDrag, mdiCodeTags, mdiBookMultipleOutline } from '@quasar/extras/mdi-v7';
import { VueDraggable } from 'vue-draggable-plus';
import { RecipeStep } from '@/api/types/RecipeStep';
import { RecipeResponse } from '@/api/types/Recipe';
import CommandStepsTable from '@/components/recipes/CommandStepsTable.vue';
import SubrecipeStepsTable from '@/components/recipes/SubrecipeStepsTable.vue';
import { subrecipeToNodes, lazyLoadSubrecipe } from '@/utils/subrecipe-nodes';

const { t } = useI18n();

const recipe = defineModel<RecipeResponse>({ required: true });
const props = defineProps({
  deviceTemplateId: {
    type: String,
    required: true,
  },
  loading: {
    type: Boolean,
    default: false,
  },
});

const selectedStepType = ref('commands');

function isStepSubrecipe(step: RecipeStep) {
  return !!step.subrecipe;
}

function removeStep(step: RecipeStep) {
  if (!recipe.value.steps) return;

  const index = recipe.value.steps.findIndex((s) => s === step);
  recipe.value.steps.splice(index, 1);
}

const stepColumns = computed<QTableProps['columns']>(() => [
  {
    name: 'drag',
    label: '',
    field: '',
    sortable: false,
    align: 'center',
  },
  {
    name: 'step',
    label: t('job.step'),
    field: 'step',
    sortable: false,
    align: 'left',
  },
  {
    name: 'cycles',
    label: t('job.cycle', 2),
    field: 'cycles',
    sortable: false,
    align: 'center',
  },
  {
    name: 'remove',
    label: '',
    field: '',
    sortable: false,
    align: 'center',
  },
]);
</script>

<style lang="scss">
.table-name {
  font-size: 1.1rem;
  font-weight: 500;
  margin-bottom: 10px;
}

.drag-icon {
  cursor: move;
}

.q-table__top {
  padding: 0 !important;
}

.bottom-outline {
  border-bottom: 1px solid #e0e0e0;
}

.rounded-top {
  border-top-left-radius: 5px;
  border-top-right-radius: 5px;
}
</style>
