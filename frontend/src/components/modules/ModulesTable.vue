<template>
  <div>
    <div>
      <div class="q-mb-md row">
        <p class="modules-text">{{ t('module.label', 2) }}</p>
        <q-space></q-space>
        <q-btn
          v-if="authStore.isAdmin"
          class="shadow bg-white"
          color="primary"
          unelevated
          outline
          no-caps
          size="15px"
          :label="t('module.create_module')"
          :icon="mdiPlus"
          @click="createModuleDialog = true"
        />
      </div>
      <q-table
        :rows="collection.modules || []"
        :columns="columns"
        flat
        class="outline shadow"
        hide-pagination
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
        :rows-per-page-label="t('table.rows_per_page_label')"
        row-key="uid"
      >
        <template #no-data="{ message }">
          <div class="full-width column flex-center q-pa-lg nothing-found-text">
            <q-icon :name="mdiHubspot" class="q-mb-md" size="50px"></q-icon>
            {{ message }}
          </div>
        </template>

        <template #header="props2">
          <q-tr :props="props2">
            <q-th auto-width />
            <q-th v-for="col in props2.cols" :key="col.name" :props="props2">
              {{ col.label }}
            </q-th>
            <q-th auto-width />
          </q-tr>
        </template>
        <template #body="props2">
          <q-tr :props="props2" no-hover>
            <q-td auto-width>
              <q-btn
                color="secondary"
                round
                dense
                unelevated
                flat
                :icon="props2.expand ? mdiChevronUp : mdiChevronDown"
                @click="props2.expand = !props2.expand"
              />
            </q-td>
            <q-td v-for="col in props2.cols" :key="col.name" :props="props2">
              <template v-if="col.name === 'name'">
                <router-link :to="`/modules/${props2.row.uid}`" class="text-black text-weight-regular">
                  {{ col.value }}
                </router-link>
              </template>
              <template v-else>
                {{ col.value }}
              </template>
            </q-td>
            <q-td auto-width>
              <q-btn :icon="mdiOpenInNew" color="grey-color" flat round :to="`/modules/${props2.row.uid}`"
                ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                  {{ t('global.open') }}
                </q-tooltip>
              </q-btn>
              <q-btn
                v-if="authStore.isAdmin"
                :icon="mdiPencil"
                color="grey-color"
                flat
                round
                @click="
                  moduleToUpdate = props2.row;
                  editModuleDialog = true;
                "
                ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                  {{ t('global.edit') }}
                </q-tooltip>
              </q-btn>
              <q-btn
                v-if="authStore.isAdmin"
                :icon="mdiTrashCanOutline"
                color="grey-color"
                flat
                round
                @click="
                  moduleToUpdate = props2.row;
                  deleteModuleDialog = true;
                "
                ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                  {{ t('global.delete') }}
                </q-tooltip>
              </q-btn>
            </q-td>
          </q-tr>

          <q-tr class="bg-grey-1 no-height" :props="props2">
            <q-td colspan="100%" class="no-height border-left-primary" no-hover>
              <q-slide-transition :duration="250">
                <div v-if="props2.expand">
                  <DevicesInModuleTable
                    v-model="props2.row.devices"
                    :module="props2.row"
                    class="q-pa-lg"
                    @on-change="emit('onUpdate')"
                  />
                </div>
              </q-slide-transition>
            </q-td>
          </q-tr>
        </template>
      </q-table>
    </div>
    <CreateModuleDialog v-model="createModuleDialog" :collection="collection" @on-create="emit('onUpdate')" />
    <EditModuleDialog
      v-if="moduleToUpdate"
      v-model="editModuleDialog"
      :module="moduleToUpdate"
      @on-update="emit('onUpdate')"
    />
    <DeleteModuleDialog
      v-if="moduleToUpdate"
      v-model="deleteModuleDialog"
      :module="moduleToUpdate"
      @on-deleted="emit('onUpdate')"
    />
  </div>
</template>

<script setup lang="ts">
import { Collection } from '@/models/Collection';
import { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import CreateModuleDialog from './CreateModuleDialog.vue';
import EditModuleDialog from './EditModuleDialog.vue';
import DevicesInModuleTable from '@/components/modules/DevicesInModuleTable.vue';
import DeleteModuleDialog from './DeleteModuleDialog.vue';
import { Module } from '@/models/Module';
import { useAuthStore } from '@/stores/auth-store';
import { useI18n } from 'vue-i18n';
import {
  mdiChevronDown,
  mdiChevronUp,
  mdiHubspot,
  mdiOpenInNew,
  mdiPencil,
  mdiPlus,
  mdiTrashCanOutline,
} from '@quasar/extras/mdi-v7';

const collection = defineModel<Collection>({ required: true });
const emit = defineEmits(['onUpdate']);

const { t } = useI18n();
const authStore = useAuthStore();

const createModuleDialog = ref(false);
const deleteModuleDialog = ref(false);
const editModuleDialog = ref(false);
const moduleToUpdate = ref<Module>();

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'name',
    label: t('global.name'),
    field: 'name',
    sortable: true,
    align: 'left',
  },
  {
    name: 'devices',
    label: t('device.label', 2),
    field: (row) => row.devices.length || 0,
    sortable: true,
    align: 'right',
  },
]);
</script>

<style lang="scss" scoped>
.modules-text {
  font-size: 1.5em;
  font-weight: 600;
  margin: 0;
  padding: 0;
  color: $secondary;
}

.no-height {
  height: 0px;
  padding: 0px;
}

.border-left-primary {
  border-left: 2px solid $primary;
}
</style>
