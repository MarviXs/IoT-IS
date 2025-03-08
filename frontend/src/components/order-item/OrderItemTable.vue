<template>
  <div class="order-item-table">
    <q-table
      :rows="containers"
      :columns="parentColumns"
      row-key="id"
      flat
      dense
      separator="horizontal"
      row-hover
    >
      <template #body="props">
        <!-- Riadok rodičovskej tabuľky -->
        <q-tr :props="props" class="parent-row">
          <q-td :props="props" key="id">{{ props.row.id }}</q-td>
          <q-td :props="props" key="name">{{ props.row.name }}</q-td>
          <q-td :props="props" key="quantity">{{ props.row.quantity }}</q-td>
          <q-td :props="props" key="actions">
            <q-btn
              flat
              dense
              round
              size="sm"
              :icon="minusIcon"
              @click="decreaseQuantity(props.row.id)"
            />
            <q-btn
              flat
              dense
              round
              size="sm"
              :icon="plusIcon"
              @click="increaseQuantity(props.row.id)"
            />
          </q-td>
          <q-td :props="props" key="pricePerContainer">
            {{ props.row.pricePerContainer }}
          </q-td>
          <q-td :props="props" key="totalPrice">
            {{ props.row.totalPrice }}
          </q-td>
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

        <!-- Vnorená tabuľka (nested row) -->
        <q-tr class="nested-row">
          <!-- Rozšírime cez colspan tak, aby sa vnorená tabuľka zobrazila v celej šírke -->
          <q-td :colspan="parentColumns.length" class="nested-table-cell">
            <q-table
              row-hover
              :rows="props.row.products"
              :columns="nestedColumns"
              flat
              dense
              separator="horizontal"
              row-key="PLU"
              no-data-label="{{ $t('table.no_data_label') }}"
              hide-bottom
              class="nested-table"
            >
              <!-- Scoped slot pre stĺpec 'actions' -->
              <template #body-cell-actions="scope">
                <q-btn
                  flat
                  dense
                  round
                  size="sm"
                  :icon="minusIcon"
                  @click="decreaseProductQuantity(props.row.id,scope.row)"
                />
                <q-btn
                  flat
                  dense
                  round
                  size="sm"
                  :icon="plusIcon"
                  @click="increaseProductQuantity(props.row.id,scope.row)"
                />
              </template>
            </q-table>
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
  components: { AddItemToOrderDialog, DeleteContainerDialog },
  props: {
    containers: { type: Array, required: true },
    refreshTable: { type: Function, required: true },
  },
  setup() {
    const route = useRoute();
    const currentOrderId = route.params.id; // Napr. /orders/:id
    return { currentOrderId };
  },
  data() {
    return {
      plusIcon: mdiPlus,
      minusIcon: mdiMinus,
      mdiTrashCan,
      mdiPencil,
      mdiPlusBox,
      isAddItemDialogOpen: false,
      isDeleteDialogOpen: false,
      selectedContainerId: null,
      parentColumns: [
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
        // Stĺpec pre akcie
        { name: 'actions', label: '', align: 'center' },
      ],
    };
  },
  methods: {
    async increaseQuantity(containerId) {
      try {
        await OrderItemsService.increaseContainerQuantity(this.currentOrderId, containerId);
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
    handleItemCreated() {
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
      this.$emit('open-delete-dialog', containerId);
    },
    handleContainerDeleted() {
      this.isDeleteDialogOpen = false;
      this.$emit('onChange');
      this.refreshTable();
    },
    // Zvýšenie množstva na úrovni produktu (vnorený riadok)
    // Zvýšenie množstva produktu – pred volaním API vyhľadáme položku v kontajneri podľa containerId a product.plu
    async increaseProductQuantity(containerId, product) {
      try {
        // Vyhľadáme kontajner podľa containerId
        const container = this.containers.find((c) => c.id === containerId);
        if (!container) {
          throw new Error('Container not found');
        }
        // Vyhľadáme položku v kontajneri podľa product.pluCode
        const item = container.products.find(p => p.pluCode === product.pluCode);
        if (!item || !item.id) {
          throw new Error(`Order item not found for product with PLU: ${product.pluCode}`);
        }
        await OrderItemsService.increaseProductQuantity(this.currentOrderId,container.id, item.id);
        toast.success(this.$t('container.toasts.increase_success'));
        this.refreshTable();
      } catch (error) {
        console.error('Error increasing product quantity:', error);
        toast.error(this.$t('container.toasts.increase_error'));
      }
    },
    // Zníženie množstva produktu – rovnako vyhľadáme položku
    async decreaseProductQuantity(containerId, product) {
      try {
        const container = this.containers.find(c => c.id === containerId);
        if (!container) {
          throw new Error('Container not found');
        }
        const item = container.products.find(p => p.pluCode === product.pluCode);
        if (!item || !item.id) {
          throw new Error(`Order item not found for product with PLU: ${product.plu}`);
        }
        await OrderItemsService.decreaseProductQuantity(this.currentOrderId,container.id, item.id);
        toast.success(this.$t('container.toasts.decrease_success'));
        this.refreshTable();
      } catch (error) {
        console.error('Error decreasing product quantity:', error);
        toast.error(this.$t('container.toasts.decrease_error'));
      }
    },
  },
};
</script>

<style scoped>
/* Štýly pre hover a pod. Môžete ich prispôsobiť podľa potreby. */
.q-table thead {
  background-color: #f5f5f5;
  font-weight: bold;
}

/* Zvýraznenie riadku pri hoverovaní */
.q-table[row-hover] tbody tr:hover {
  background-color: #e0f7fa;
  cursor: pointer;
}

/* Ohraničenie pre vnorené tabuľky */
.nested-table {
  border: 1px solid #e0e0e0;
  margin-top: 5px;
  margin-bottom: 5px;
}
</style>
