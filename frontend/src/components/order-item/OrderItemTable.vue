<template>
  <div class="order-item-table">
    <q-table :rows="containers" :columns="parentColumns" row-key="id" flat dense separator="horizontal">
      <template #body="props">
        <q-tr :props="props" class="parent-row">
          <q-td :props="props" key="id">{{ props.row.id }}</q-td>
          <q-td :props="props" key="name">{{ props.row.name }}</q-td>
          <q-td :props="props" key="quantity">{{ props.row.quantity }}</q-td>
          <q-td :props="props" key="actions">
            <q-btn flat dense round size="sm" :icon="minusIcon" @click="decreaseQuantity(props.row.id)" />
            <q-btn flat dense round size="sm" :icon="plusIcon" @click="increaseQuantity(props.row.id)" />
          </q-td>
          <q-td :props="props" key="pricePerContainer">{{ props.row.pricePerContainer }}</q-td>
          <q-td :props="props" key="totalPrice">{{ props.row.totalPrice }}</q-td>
          <q-td>
            <div class="tw-flex tw-justify-center">
              <q-btn
                flat
                round
                size="sm"
                :icon="mdiPlusBox"
                color="grey-color"
                @click="openAddItemDialog(props.row.id)"
              >
                <q-tooltip>{{ $t('order_item.add_item') }}</q-tooltip>
              </q-btn>
              <q-btn
                flat
                round
                size="sm"
                :icon="mdiPencil"
                color="grey-color"
                @click.stop="openUpdateDialog(props.row.id)"
              >
                <q-tooltip>{{ $t('global.edit') }}</q-tooltip>
              </q-btn>
              <q-btn
                flat
                round
                size="sm"
                :icon="mdiTrashCan"
                color="grey-color"
                @click.stop="openDeleteDialog(props.row.id)"
              >
                <q-tooltip>{{ $t('global.delete') }}</q-tooltip>
              </q-btn>
            </div>
          </q-td>
        </q-tr>
        <q-tr class="nested-row">
          <q-td :colspan="parentColumns.length" class="nested-table-cell">
            <q-table
              :rows="props.row.products"
              :columns="nestedColumns"
              flat
              dense
              separator="horizontal"
              row-key="PLU"
              no-data-label="{{ $t('table.no_data_label') }}"
              hide-bottom
              class="nested-table"
            />
          </q-td>
        </q-tr>
      </template>
    </q-table>

    <!-- AddItem Dialog -->

    <AddItemToOrderDialog
      v-model="isAddItemDialogOpen"
      :orderId="currentOrderId"
      :containerId="selectedContainerId"
      @onCreate="handleItemCreated"
    />

    <!-- DeleteContainer Dialog -->
    <DeleteContainerDialog
      v-model="isDeleteDialogOpen"
      :orderId="currentOrderId"
      :containerId="selectedContainerId"
      @onDeleted="handleContainerDeleted"
    />
  </div>
</template>

<script>
import { mdiPlus, mdiMinus, mdiTrashCan, mdiPencil, mdiPlusBox } from '@quasar/extras/mdi-v7';
import AddItemToOrderDialog from '@/components/order-item/AddItemToOrderDialog.vue';
import DeleteContainerDialog from '@/components/order-item/DeleteContainerDialog.vue';
import OrderItemsService from '@/api/services/OrderItemsService';
import { useRoute } from 'vue-router';
import { toast } from 'vue3-toastify';

export default {
  name: 'OrderItemTable',
  setup() {
    const route = useRoute();
    const currentOrderId = route.params.id; // Získanie orderId z URL

    return {
      currentOrderId,
    };
  },
  components: { AddItemToOrderDialog },
  props: {
    containers: {
      type: Array,
      required: true,
    },
    refreshTable: {
      type: Function, // Externá funkcia na obnovu tabuľky
      required: true,
    },
  },
  data() {
    return {
      plusIcon: mdiPlus,
      minusIcon: mdiMinus,
      mdiTrashCan: mdiTrashCan,
      mdiPencil: mdiPencil,
      mdiPlusBox: mdiPlusBox,
      isAddItemDialogOpen: false,
      selectedOrderId: null,
      parentColumns: [
        //{ name: "id", label: this.$t('order_item.id'), field: "id", align: "left" },
        { name: 'name', label: this.$t('global.name'), field: 'name', align: 'left' },
        { name: 'quantity', label: this.$t('order_item.quantity'), field: 'quantity', align: 'center' },
        { name: 'actions', label: '', align: 'center' },
        { name: 'pricePerContainer', label: this.$t('order_item.oneContainer'), align: 'right' },
        { name: 'totalPrice', label: this.$t('order_item.total'), align: 'right' },
        { name: 'finalActions', label: this.$t('global.actions'), align: 'center' },
      ],
      nestedColumns: [
        { name: 'pluCode', label: this.$t('product.plu_code'), field: 'pluCode', align: 'left' },
        { name: 'latinName', label: this.$t('product.latin_name'), field: 'latinName', align: 'left' },
        { name: 'czechName', label: this.$t('product.czech_name'), field: 'czechName', align: 'left' },
        { name: 'variety', label: this.$t('product.variety'), field: 'variety', align: 'left' },
        {
          name: 'potDiameterPack',
          label: this.$t('product.pot_diameter_pack'),
          field: 'potDiameterPack',
          align: 'left',
        },
        { name: 'quantity', label: this.$t('order_item.quantity'), field: 'quantity', align: 'right' },
        {
          name: 'pricePerPiecePack',
          label: this.$t('product.price_per_piece_pack'),
          field: 'pricePerPiecePack',
          align: 'right',
        },
        {
          name: 'pricePerPiecePackVAT',
          label: this.$t('product.price_per_piece_pack_vat'),
          field: 'pricePerPiecePackVAT',
          align: 'right',
        },
      ],
    };
  },
  methods: {
    async increaseQuantity(containerId) {
      try {
        await OrderItemsService.increaseContainerQuantity(this.currentOrderId, containerId);
        const container = this.containers.find((c) => c.id === containerId);
        if (container) {
        }
        toast.success(this.$t('container.toasts.increase_success'));
        this.refreshTable();
      } catch (error) {
        console.error('Error increasing quantity:', error);
        toast.error(this.$t('container.toasts.increase_error'));
      }
    },
    async decreaseQuantity(containerId) {
      try {
        const container = this.containers.find((c) => c.id === containerId);
        if (container && container.quantity > 0) {
          await OrderItemsService.decreaseContainerQuantity(this.currentOrderId, containerId);
          toast.success(this.$t('container.toasts.decrease_success'));
          this.refreshTable();
        } else {
          toast.error(this.$t('container.toasts.minimum_quantity'));
        }
      } catch (error) {
        console.error('Error decreasing quantity:', error);
        toast.error(this.$t('container.toasts.decrease_error'));
      }
    },
    openAddItemDialog(containerId) {
      this.selectedContainerId = containerId;
      this.isAddItemDialogOpen = true;
      this.refreshTable();
    },
    handleItemCreated(newItem) {
      this.isAddItemDialogOpen = false;
      this.currentOrderId = null;
      this.refreshTable();
    },
    openUpdateDialog(containerId) {
      alert(this.$t('global.edit'));
    },
    openDeleteDialog(containerId) {
      this.selectedContainerId = containerId;
      this.isDeleteDialogOpen = true;
      this.$emit('open-delete-dialog', containerId); // Emitovanie udalosti pre rodiča
    },
    handleContainerDeleted() {
      this.isDeleteDialogOpen = false;
      this.$emit('onChange'); // Môžete znova načítať tabuľku, aby ste aktualizovali dáta
      this.refreshTable();
    },
  },
};
</script>

<style scoped>
.action-column {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 0;
}

.product-quantity-actions {
  display: flex;
  align-items: center;
}

.final-actions {
  display: flex;
  justify-content: center;
  gap: 12px;
}

.q-btn {
  font-size: 16px;
}

/* Zvýraznenie hlavičky tabuľky */
.q-table thead {
  background-color: #f5f5f5;
  font-weight: bold;
}

/* Striedanie farieb pre riadky tabuľky */
.q-table tbody .parent-row:nth-child(odd) {
  background-color: #ffffff;
}

.q-table tbody .parent-row:nth-child(even) {
  background-color: #f9f9f9;
}

/* Zvýraznenie riadku pri hoverovaní */
.q-table tbody .parent-row:hover {
  background-color: #e0f7fa; /* Svetlomodrá farba */
  cursor: pointer;
}

/* Ohraničenie pre vnorené tabuľky */
.nested-table {
  border: 1px solid #e0e0e0;
  margin-top: 5px;
  margin-bottom: 5px;
}

/* Zvýraznenie vnorených riadkov */
.q-table .nested-row {
  background-color: #fdfdfd;
}

/* Akcie v stĺpci sú zarovnané a prehľadné */
.action-column,
.final-actions {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
}

/* Pridanie jemného ohraničenia medzi bunkami */
.q-table td {
  border-bottom: 1px solid #ddd;
  padding: 10px;
}
.q-table--dense .q-table thead tr,
.q-table--dense .q-table tbody tr,
.q-table--dense .q-table tbody td {
  height: 30px;
}
</style>
