<template>
  <PageLayout :title="t('notification.label', 2)">
    <template #default>
      <div class="q-pa-md">
        <q-card class="q-ma-md shadow-7 rounded-card" style="border: solid indigo">
          <q-card-section>
            <div class="text-h5 row justify-center" style="font-weight: bold">Aktuálne nevyriešené notifikácie</div>
          </q-card-section>

          <q-card-section>
            <q-table
              :rows="store.scenarios"
              :columns="scenarioColumns"
              :loading="store.isLoadingScenarios"
              flat
              bordered
              :rows-per-page-options="[10, 20, 50]"
              class="shadow q-pa-sm"
              :no-data-label="t('table.no_data_label')"
              :loading-label="t('table.loading_label')"
              :rows-per-page-label="t('table.rows_per_page_label')"
              style="border-color: black; font-weight: bold; font-size: larger"
              separator="cell"
              color="primary"
              card-class="bg-indigo-2 text-black"
              table-class="text-black-1"
              table-header-class="text-black"
              @row-click="onRowClick"
            />
          </q-card-section>
        </q-card>

        <div class="row justify-center">
          <q-btn
            full-width
            style="color: black; font-weight: bolder"
            outline
            label="Skryť spracované notifikácie"
            :icon="clicked ? mdiArrowDownCircleOutline : mdiArrowUpCircleOutline"
            :icon-right="clicked ? mdiArrowDownCircleOutline : mdiArrowUpCircleOutline"
            class="q-pa-lg q-ma-lg text-h7"
            @click="clicked ? (clicked = false) : (clicked = true)"
          />
        </div>
        <q-card v-if="clicked" class="q-ma-md shadow-7 rounded-card" style="border: solid gray">
          <q-card-section>
            <div class="text-h5 row justify-center" style="font-weight: bold">Spracované notifikácie</div>
          </q-card-section>

          <q-card-section>
            <div class="row justify-center">
              <q-table
                :rows="filteredScenarios(ScenarioLevelEnum.NOT_SOLVED)"
                :columns="storedResolvedScenarioColumns"
                :loading="store.isLoadingScenarios"
                flat
                bordered
                :rows-per-page-options="[10, 20, 50]"
                class="shadow q-ma-sm"
                :no-data-label="t('table.no_data_label')"
                :loading-label="t('table.loading_label')"
                :rows-per-page-label="t('table.rows_per_page_label')"
                style="border-color: black; font-weight: bold; font-size: larger"
                separator="cell"
                color="primary"
                card-class="bg-red-2 text-black"
                table-class="text-grey-9"
                table-header-class="text-black"
              >
              </q-table>
              <q-table
                :rows="filteredScenarios(ScenarioLevelEnum.SOLUTION_IN_PROGRESS)"
                :columns="storedResolvedScenarioColumns"
                :loading="store.isLoadingScenarios"
                flat
                bordered
                :rows-per-page-options="[10, 20, 50]"
                class="shadow q-ma-sm"
                :no-data-label="t('table.no_data_label')"
                :loading-label="t('table.loading_label')"
                :rows-per-page-label="t('table.rows_per_page_label')"
                style="border-color: black; font-weight: bold; font-size: larger"
                separator="cell"
                color="primary"
                card-class="bg-yellow-2 text-black"
                table-class="text-grey-9"
                table-header-class="text-black"
              >
              </q-table>
              <q-table
                :rows="filteredScenarios(ScenarioLevelEnum.SOLVED)"
                :columns="storedResolvedScenarioColumns"
                :loading="store.isLoadingScenarios"
                flat
                bordered
                :rows-per-page-options="[10, 20, 50]"
                class="shadow q-ma-sm"
                :no-data-label="t('table.no_data_label')"
                :loading-label="t('table.loading_label')"
                :rows-per-page-label="t('table.rows_per_page_label')"
                style="border-color: black; font-weight: bold; font-size: larger"
                separator="cell"
                color="primary"
                card-class="bg-green-2 text-black"
                table-class="text-grey-9"
                table-header-class="text-black"
              >
              </q-table>
              <q-table
                :rows="filteredScenarios(ScenarioLevelEnum.INVALID)"
                :columns="storedResolvedScenarioColumns"
                :loading="store.isLoadingScenarios"
                flat
                bordered
                :rows-per-page-options="[10, 20, 50]"
                class="shadow q-ma-sm"
                :no-data-label="t('table.no_data_label')"
                :loading-label="t('table.loading_label')"
                :rows-per-page-label="t('table.rows_per_page_label')"
                style="border-color: black; font-weight: bold; font-size: larger"
                separator="cell"
                color="primary"
                card-class="bg-grey-4 text-black"
                table-class="text-grey-9"
                table-header-class="text-black"
              >
              </q-table>
              <q-table
                :rows="filteredScenarios(ScenarioLevelEnum.NEED_ASSISTANCE)"
                :columns="storedResolvedScenarioColumns"
                :loading="store.isLoadingScenarios"
                flat
                bordered
                :rows-per-page-options="[10, 20, 50]"
                class="shadow q-ma-sm"
                :no-data-label="t('table.no_data_label')"
                :loading-label="t('table.loading_label')"
                :rows-per-page-label="t('table.rows_per_page_label')"
                style="border-color: black; font-weight: bold; font-size: larger"
                separator="cell"
                color="primary"
                card-class="bg-purple-2 text-black"
                table-class="text-grey-9"
                table-header-class="text-black"
              >
              </q-table>
            </div>
          </q-card-section>
        </q-card>
      </div>
    </template>
  </PageLayout>
  <NotificationLevelDialog v-if="activatedScenario" v-model="isLevelDialogOpened" :scenario="activatedScenario" />
</template>

<script setup lang="ts">
import PageLayout from '@/layouts/PageLayout.vue';
import { QTableProps } from 'quasar';
import { useScenarioStore } from '@/stores/scenario-store';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiArrowDownCircleOutline, mdiArrowUpCircleOutline } from '@quasar/extras/mdi-v7';
import { Scenario } from '@/models/Scenario';
import { mdiMessageTextFast, mdiClipboardListOutline } from '@quasar/extras/mdi-v7';
import NotificationLevelDialog from '@/components/notifications/NotificationLevelDialog.vue';
import { StoredResolvedScenario } from '@/models/StoredResolvedScenario';
import ScenarioLevelEnum from '@/models/enums/ScenarioLevelEnum';
import ScenarioActionTypeEnum from '@/models/enums/ScenarioActionTypeEnum';

const { t } = useI18n();
const store = useScenarioStore();
store.getScenariosWithMessage();
store.getResolvedScenariosWithMessage();
const clicked = ref(false);
const activatedScenario = ref<Scenario>();

const scenarioColumns = computed<QTableProps['columns']>(() => [
  {
    name: 'notification',
    label: t('notification.label', 1),
    field: (row: Scenario) => {
      if (typeof row.messageAndTriggerTime === 'object' && row.messageAndTriggerTime !== null) {
        const keys = Object.keys(row.messageAndTriggerTime);
        return keys.length > 0 ? keys[0] : ''; // Return the first key or an empty string
      } else {
        console.error('messageAndTriggerTime is not an object:', row.messageAndTriggerTime);
        return ''; // Return an empty string if messageAndTriggerTime is not an object
      }
    },
    sortable: true,
    align: 'left',
  },
  {
    name: 'name',
    label: 'Názov scenáru',
    field: 'name',
    sortable: true,
    align: 'left',
  },
  {
    name: 'number',
    label: t('notification.number'),
    field: (row: Scenario) => {
      if (typeof row.messageAndTriggerTime === 'object' && row.messageAndTriggerTime !== null) {
        const keys = Object.keys(row.messageAndTriggerTime);
        if (keys.length > 0) {
          const firstKey = keys[0];
          const valueArray = row.messageAndTriggerTime[firstKey];
          return Array.isArray(valueArray) ? valueArray.length : 0; // Return the length of the array
        }
      }
      return 0; // Return 0 if messageAndTriggerTime is not an object or has no keys
    },
    align: 'center',
    sortable: true,
  },
]);

const storedResolvedScenarioColumns = computed<QTableProps['columns']>(() => [
  {
    name: 'notification',
    label: t('notification.label', 1),
    field: (row: StoredResolvedScenario) => {
      if (typeof row.messageAndTriggerTime === 'object' && row.messageAndTriggerTime !== null) {
        const keys = Object.keys(row.messageAndTriggerTime);
        return keys.length > 0 ? keys[0] : ''; // Return the first key or an empty string
      } else {
        console.error('messageAndTriggerTime is not an object:', row.messageAndTriggerTime);
        return ''; // Return an empty string if messageAndTriggerTime is not an object
      }
    },
    sortable: true,
    align: 'left',
  },
  {
    name: 'level',
    label: t('notification.level'),
    field: 'level',
    sortable: true,
    align: 'center',
  },
  {
    name: 'number',
    label: t('notification.number'),
    field: (row: Scenario) => {
      if (typeof row.messageAndTriggerTime === 'object' && row.messageAndTriggerTime !== null) {
        const keys = Object.keys(row.messageAndTriggerTime);
        if (keys.length > 0) {
          const firstKey = keys[0];
          const valueArray = row.messageAndTriggerTime[firstKey];
          return Array.isArray(valueArray) ? valueArray.length : 0; // Return the length of the array
        }
      }
      return 0; // Return 0 if messageAndTriggerTime is not an object or has no keys
    },
    align: 'center',
    sortable: true,
  },
]);

const isLevelDialogOpened = ref(false);

const onRowClick = (evt: Event, scenarioRow: Scenario) => {
  isLevelDialogOpened.value = true;
  activatedScenario.value = scenarioRow;
};

const filteredScenarios = (level: ScenarioLevelEnum) => {
  return store.resolvedScenarios.filter((scenario) => scenario.level === level);
};
</script>

<style lang="scss" scoped></style>
