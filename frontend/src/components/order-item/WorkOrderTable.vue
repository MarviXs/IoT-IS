<template>
  <div class="work-order-table">
    <q-table
      :rows="orders"
      :columns="columns"
      row-key="id"
      flat
      dense
      separator="horizontal"
      row-hover
    >
      <template #body="props">
        <q-tr :props="props" class="parent-row">
          <q-td :props="props" key="workOrder">{{ props.row.workOrder }}</q-td>
          <q-td :props="props" key="note">{{ props.row.note }}</q-td>
          <q-td :props="props" key="hours" align="right">{{ props.row.hours }}</q-td>
          <q-td :props="props" key="pricePerHour" align="right">
            {{ props.row.pricePerHour }}
          </q-td>
          <q-td :props="props" key="priceExclVAT" align="right">
            {{ props.row.priceExclVAT }}
          </q-td>
          <q-td :props="props" key="actions" align="center">
            <div class="actions">
              <q-btn flat round dense size="sm" :icon="mdiPencil" color="grey-color" @click="editWorkOrder(props.row.id)">
                <q-tooltip>{{ $t('global.edit') }}</q-tooltip>
              </q-btn>

              <q-btn flat round dense size="sm" :icon="mdiTrashCan" color="grey-color" @click="deleteWorkOrder(props.row.id)">
                <q-tooltip>{{ $t('global.delete') }}</q-tooltip>
              </q-btn>
            </div>
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiPencil, mdiTrashCan } from '@quasar/extras/mdi-v7';


const { t } = useI18n();

// Definícia typu pre pracovný záznam
interface WorkOrderItem {
  id: string;
  workOrder: string;
  note: string;
  hours: number;
  pricePerHour: number;
  priceExclVAT: number;
}

// Ukážkové dáta – v reálnom projekte by ste ich načítali cez API
const orders = ref<WorkOrderItem[]>([
  { id: '1', workOrder: 'WO-1001', note: 'Urgent task', hours: 8, pricePerHour: 25, priceExclVAT: 200 },
  { id: '2', workOrder: 'WO-1002', note: 'Routine maintenance', hours: 6, pricePerHour: 25, priceExclVAT: 150 },
  { id: '3', workOrder: 'WO-1003', note: 'Installation', hours: 10, pricePerHour: 30, priceExclVAT: 300 },
]);

const columns: { name: string; label: string; field: string; align: "left" | "right" | "center" }[] = [
  { name: 'workOrder', label: t('order_item.work_order'), field: 'workOrder', align: 'left' },
  { name: 'note', label: t('order_item.note'), field: 'note', align: 'left' },
  { name: 'hours', label: t('order_item.hours'), field: 'hours', align: 'right' },
  { name: 'pricePerHour', label: t('order_item.price_per_hour'), field: 'pricePerHour', align: 'right' },
  { name: 'priceExclVAT', label: t('order_item.price_excl_vat'), field: 'priceExclVAT', align: 'right' },
  {
    name: 'actions', align: 'center',
    field: '',
    label: ''
  },
];

// Príklady metód pre editáciu a mazanie
function editWorkOrder(id: string) {
  // Implementujte editáciu podľa potreby
  console.log('Edit work order with id:', id);
}

function deleteWorkOrder(id: string) {
  // Implementujte vymazanie podľa potreby
  console.log('Delete work order with id:', id);
}
</script>

<style scoped>
.q-table{
  width: 100%;
}

.q-table thead {
  background-color: #f5f5f5;
  font-weight: bold;
}

/* Zvýraznenie riadku pri hoverovaní */
.q-table[row-hover] tbody tr:hover {
  background-color: #e0f7fa;
  cursor: pointer;
}

.work-order-table {
  margin: 16px;
}

.actions q-btn {
  margin: 0 4px;
}

</style>
