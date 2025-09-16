<template>
  <q-table
    v-model:pagination="pagination"
    :rows="companies"
    :columns="columns"
    :loading="props.loading"
    flat
    :rows-per-page-options="[10, 20, 50]"
    :no-data-label="t('table.no_data_label')"
    :loading-label="t('table.loading_label')"
    :rows-per-page-label="t('table.rows_per_page_label')"
    binary-state-sort
    @request="emit('onRequest', $event)"
  >
    <template #no-data="{ message }">
      <div class="full-width column flex-center q-pa-lg nothing-found-text">
        <q-icon :name="mdiFileSearchOutline" class="q-mb-md" size="50px"></q-icon>
        {{ message }}
      </div>
    </template>
    <template #body-cell-actions="propsActions">
      <q-td auto-width :props="propsActions">
        <q-btn :icon="mdiPencil" color="grey-color" flat round @click.stop="openUpdateDialog(propsActions.row.id)">
          <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.edit') }}
          </q-tooltip>
        </q-btn>
        <q-btn :icon="mdiTrashCan" color="grey-color" flat round @click.stop="openDeleteDialog(propsActions.row.id)">
          <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.delete') }}
          </q-tooltip>
        </q-btn>
      </q-td>
    </template>
  </q-table>
  <EditCompanyDialog
    v-if="companyToUpdate"
    v-model="isUpdateDialogOpen"
    :company-id="companyToUpdate"
    @on-update="emit('onChange')"
  />
  <DeleteCompanyDialog
    v-if="companyToDelete"
    v-model="isDeleteDialogOpen"
    :company-id="companyToDelete"
    @on-deleted="emit('onChange')"
  />
</template>

<script setup lang="ts">
import type { QTableProps } from 'quasar';
import { computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiFileSearchOutline, mdiPencil, mdiTrashCan } from '@quasar/extras/mdi-v7';
import type { PaginationClient } from '@/models/Pagination';
import EditCompanyDialog from './EditCompanyDialog.vue';
import DeleteCompanyDialog from './DeleteCompanyDialog.vue';
import type { PropType } from 'vue';

const props = defineProps({
  loading: {
    type: Boolean,
    required: true,
  },
});

const companies = defineModel<Array<any>>('companies', {
  type: Array as PropType<Array<any>>,
  default: [],
});

const pagination = defineModel<PaginationClient>('pagination');

const emit = defineEmits(['onChange', 'onRequest']);

const { t } = useI18n();

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'title',
    label: t('company.name'),
    field: 'title',
    align: 'left',
    sortable: true,
  },
  {
    name: 'ic',
    label: t('company.IC'),
    field: 'ic',
    align: 'left',
    sortable: true,
  },
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);

const isUpdateDialogOpen = ref(false);
const companyToUpdate = ref<string>();
function openUpdateDialog(companyId: string) {
  companyToUpdate.value = companyId;
  isUpdateDialogOpen.value = true;
}

const isDeleteDialogOpen = ref(false);
const companyToDelete = ref<string>();
function openDeleteDialog(companyId: string) {
  isDeleteDialogOpen.value = true;
  companyToDelete.value = companyId;
}
</script>
