<template>
  <q-table
    v-model:pagination="pagination"
    :rows="rows"
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
    <template #body-cell-note="propsRow">
      <q-td :props="propsRow">
        <span class="text-body2">{{ truncateNote(propsRow.row.note) }}</span>
      </q-td>
    </template>

    <template #body-cell-deviceName="propsRow">
      <q-td :props="propsRow">{{ propsRow.row.deviceName || '—' }}</q-td>
    </template>

    <template #body-cell-startedBy="propsRow">
      <q-td :props="propsRow">{{ propsRow.row.startedBy || '—' }}</q-td>
    </template>

    <template #body-cell-ranJobName="propsRow">
      <q-td :props="propsRow">{{ propsRow.row.ranJobName || propsRow.row.ranJobId || '—' }}</q-td>
    </template>

    <template #body-cell-startedAt="propsRow">
      <q-td :props="propsRow">{{ formatDateTime(propsRow.row.startedAt) }}</q-td>
    </template>

    <template #body-cell-finishedAt="propsRow">
      <q-td :props="propsRow">{{ formatDateTime(propsRow.row.finishedAt) }}</q-td>
    </template>

    <template #body-cell-actions="propsActions">
      <q-td auto-width :props="propsActions">
        <q-btn
          v-if="canEdit(propsActions.row.ownerId)"
          :icon="mdiPencil"
          color="grey-color"
          flat
          round
          @click.stop="emit('onEdit', propsActions.row.id)"
        >
          <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.edit') }}
          </q-tooltip>
        </q-btn>
        <q-btn
          v-if="canEdit(propsActions.row.ownerId)"
          :icon="mdiTrashCan"
          color="grey-color"
          flat
          round
          @click.stop="emit('onDelete', propsActions.row.id)"
        >
          <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.delete') }}
          </q-tooltip>
        </q-btn>
      </q-td>
    </template>

    <template #no-data="{ message }">
      <div class="full-width column flex-center q-pa-lg nothing-found-text">
        <q-icon :name="mdiFileSearchOutline" class="q-mb-md" size="50px" />
        {{ message }}
      </div>
    </template>
  </q-table>
</template>

<script setup lang="ts">
import type { QTableProps } from 'quasar';
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiFileSearchOutline, mdiPencil, mdiTrashCan } from '@quasar/extras/mdi-v7';
import type { PaginationClient } from '@/models/Pagination';
import type { ExperimentsResponse } from '@/api/services/ExperimentService';
import type { PropType } from 'vue';
import { useAuthStore } from '@/stores/auth-store';

const props = defineProps({
  experiments: {
    type: Object as PropType<ExperimentsResponse>,
    required: false,
    default: null,
  },
  loading: {
    type: Boolean,
    required: true,
  },
});

const pagination = defineModel<PaginationClient>('pagination');

const emit = defineEmits(['onRequest', 'onEdit', 'onDelete']);

const { t, locale } = useI18n();
const authStore = useAuthStore();

const rows = computed(() => props.experiments?.items ?? []);

function formatDateTime(value?: string | null) {
  if (!value) {
    return '—';
  }

  return new Date(value).toLocaleString(locale.value);
}

function truncateNote(value?: string | null) {
  if (!value) {
    return '—';
  }

  const maxLength = 30;
  if (value.length <= maxLength) {
    return value;
  }

  return `${value.slice(0, maxLength)}...`;
}

function canEdit(ownerId?: string) {
  if (!ownerId) {
    return false;
  }

  return authStore.isAdmin || authStore.user?.id === ownerId;
}

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'note',
    label: t('experiment.note'),
    field: 'note',
    align: 'left',
    sortable: true,
  },
  {
    name: 'deviceName',
    label: t('experiment.device'),
    field: 'deviceName',
    align: 'left',
    sortable: false,
  },
  {
    name: 'ranJobName',
    label: t('experiment.job'),
    field: 'ranJobName',
    align: 'left',
    sortable: false,
  },
  {
    name: 'startedAt',
    label: t('experiment.started_at'),
    field: 'startedAt',
    align: 'left',
    sortable: true,
  },
  {
    name: 'finishedAt',
    label: t('experiment.finished_at'),
    field: 'finishedAt',
    align: 'left',
    sortable: true,
  },
  {
    name: 'startedBy',
    label: t('experiment.started_by'),
    field: 'startedBy',
    align: 'left',
    sortable: false,
  },
  {
    name: 'actions',
    label: '',
    field: '',
    align: 'center',
    sortable: false,
  },
]);
</script>
