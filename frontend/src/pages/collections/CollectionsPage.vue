<template>
  <PageLayout :title="t('collection.label', 2)">
    <template #actions>
      <SearchBar v-model="filter" />
      <q-btn
        v-if="authStore.isAdmin"
        class="shadow"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('collection.create_collection')"
        :icon="mdiPlus"
        @click="createCollectionDialog = true"
      />
    </template>
    <template #default>
      <q-table
        :rows="filteredCollections"
        :columns="columns"
        :loading="store.collections.isLoading"
        flat
        :rows-per-page-options="[10, 20, 50]"
        class="shadow"
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

        <template #header="props">
          <q-tr :props="props">
            <q-th auto-width />
            <q-th v-for="col in props.cols" :key="col.name" :props="props">
              {{ col.label }}
            </q-th>
            <q-th auto-width />
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" no-hover>
            <q-td auto-width>
              <q-btn
                color="secondary"
                round
                dense
                unelevated
                flat
                :icon="props.expand ? mdiChevronUp : mdiChevronDown"
                @click="props.expand = !props.expand"
              />
            </q-td>
            <q-td v-for="col in props.cols" :key="col.name" :props="props">
              <template v-if="col.name === 'name'">
                <router-link :to="`/collections/${props.row.uid}`" class="text-black text-weight-regular">
                  {{ col.value }}
                </router-link>
              </template>
              <template v-else>
                {{ col.value }}
              </template>
            </q-td>
            <q-td auto-width>
              <q-btn :icon="mdiOpenInNew" color="grey-color" flat round :to="`/collections/${props.row.uid}`"
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
                  collectionToUpdate = props.row;
                  editCollectionDialog = true;
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
                  collectionToUpdate = props.row;
                  deleteCollectionDialog = true;
                "
                ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
                  {{ t('global.delete') }}
                </q-tooltip>
              </q-btn>
            </q-td>
          </q-tr>
          <q-tr class="bg-grey-1 no-height" :props="props">
            <q-td colspan="100%" class="no-height border-left-primary" no-hover>
              <q-slide-transition :duration="250">
                <div v-if="props.expand">
                  <ModulesTable
                    v-model="props.row"
                    class="q-pa-lg"
                    @update:model-value="store.updateCollection"
                    @on-update="store.collections.refresh"
                  />
                </div>
              </q-slide-transition>
            </q-td>
          </q-tr>
        </template>
      </q-table>
    </template>
  </PageLayout>
  <CreateCollectionDialog v-model="createCollectionDialog" @on-create="store.collections.refresh" />
  <EditCollectionDialog
    v-if="collectionToUpdate"
    v-model="editCollectionDialog"
    :collection="collectionToUpdate"
    @on-update="store.collections.refresh"
  />
  <DeleteCollectionDialog
    v-if="collectionToUpdate"
    v-model="deleteCollectionDialog"
    :collection="collectionToUpdate"
    @on-deleted="store.collections.refresh"
  />
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { Collection } from '@/models/Collection';
import { computed, ref } from 'vue';
import CreateCollectionDialog from '@/components/collections/CreateCollectionDialog.vue';
import EditCollectionDialog from '@/components/collections/EditCollectionDialog.vue';
import ModulesTable from '@/components/modules/ModulesTable.vue';
import DeleteCollectionDialog from '@/components/collections/DeleteCollectionDialog.vue';
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
} from '@quasar/extras/mdi-v6';
import PageLayout from '@/layouts/PageLayout.vue';
import { useCollectionStore } from '@/stores/collection-store';
import SearchBar from '@/components/core/SearchBar.vue';

const { t } = useI18n();

const authStore = useAuthStore();
const store = useCollectionStore();
store.collections.refresh();

const createCollectionDialog = ref(false);
const deleteCollectionDialog = ref(false);
const editCollectionDialog = ref(false);
const collectionToUpdate = ref<Collection>();
const filter = ref('');

const filteredCollections = computed(() => {
  const lowerFilter = filter.value.toLowerCase();
  return (
    store.collections.data?.filter(
      ({ name, modules }) =>
        name.toLowerCase().includes(lowerFilter) ||
        modules?.some(
          ({ name, devices }) =>
            name.toLowerCase().includes(lowerFilter) ||
            devices?.some((device) => device.name.toLowerCase().includes(lowerFilter)),
        ),
    ) ?? []
  );
});

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'name',
    label: t('global.name'),
    field: 'name',
    sortable: true,
    align: 'left',
  },
  {
    name: 'modules',
    label: t('module.label', 2),
    field: (row) => row.modules.length || 0,
    sortable: true,
    align: 'right',
  },
]);
</script>

<style lang="scss" scoped>
.no-height {
  height: 0px;
  padding: 0px;
}

.border-left-primary {
  border-left: 2px solid $accent;
}
</style>
