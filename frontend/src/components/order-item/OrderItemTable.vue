<template>
  <div class="order-item-table">
    <!-- Hlavná tabuľka: zobrazujú sa iba kontajnery, ktoré nemajú názov "práca" -->
    <q-table
      :rows="nonWorkContainers"
      :columns="parentColumns"
      row-key="id"
      flat
      dense
      separator="horizontal"
      row-hover
    >
      <template #body="props">
        <!-- Riadok hlavnej tabuľky (parent row) -->
        <q-tr :props="props" class="parent-row">
          <q-td :props="props" key="id">{{ props.row.id }}</q-td>
          <q-td :props="props" key="name">{{ props.row.name }}</q-td>
          <q-td :props="props" key="quantity">{{ props.row.quantity }}</q-td>
          <q-td :props="props" key="actions">
            <q-btn flat dense round size="sm" :icon="minusIcon" @click="decreaseQuantity(props.row.id)" />
            <q-btn flat dense round size="sm" :icon="plusIcon" @click="increaseQuantity(props.row.id)" />
          </q-td>
          <q-td :props="props" key="pricePerContainer">
            {{ props.row.pricePerContainer }}
          </q-td>
          <q-td :props="props" key="totalPrice">
            {{ props.row.totalPrice * props.row.discount }}
          </q-td>
          <q-td>
            <div class="actions-container">
              <q-btn flat round size="sm" :icon="mdiPlusBox" color="grey-color" @click="openAddItemDialog(props.row.id)">
                <q-tooltip>{{ $t('order_item.add_item') }}</q-tooltip>
              </q-btn>
              <q-btn flat round size="sm" :icon="mdiTrashCan" color="grey-color" @click.stop="openDeleteDialog(props.row.id)">
                <q-tooltip>{{ $t('global.delete') }}</q-tooltip>
              </q-btn>
            </div>
          </q-td>
        </q-tr>

        <!-- Vnorená tabuľka pre produkty v danom kontejnere (ak sú definované) -->
        <q-tr class="nested-row">
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
              <template #body-cell-actions="scope">
                <q-btn flat dense round size="sm" :icon="minusIcon" @click="decreaseProductQuantity(props.row.id, scope.row)" />
                <q-btn flat dense round size="sm" :icon="plusIcon" @click="increaseProductQuantity(props.row.id, scope.row)" />
              </template>
            </q-table>
          </q-td>
        </q-tr>
      </template>
    </q-table>

    <!-- Samostatná tabuľka pre produkty kontajnera s názvom "práca" -->
    <div v-if="workOrderContainer" class="work-order-section" style="margin-top: 20px;">
      <!-- Tabuľka s mapovanými stĺpcami -->
      <q-table
        :rows="workOrderRows"
        :columns="workOrderColumns"
        row-key="pluCode"
        flat
        dense
        separator="horizontal"
        no-data-label="{{ $t('table.no_data_label') }}"
      >
        <template #body-cell-actions="scope">
          <q-btn flat dense round size="sm" :icon="minusIcon" @click="decreaseProductQuantity(workOrderContainer.id, scope.row)" />
          <q-btn flat dense round size="sm" :icon="plusIcon" @click="increaseProductQuantity(workOrderContainer.id, scope.row)" />
        </template>
      </q-table>
    </div>

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
      // Stĺpce pre vnorenú tabuľku (ak sú zobrazené v kontejnere, ktorý nie je "práca")
      nestedColumns: [
        { name: 'pluCode', label: this.$t('product.plu_code'), field: 'pluCode', align: 'left' },
        { name: 'latinName', label: this.$t('product.latin_name'), field: 'latinName', align: 'left' },
        { name: 'czechName', label: this.$t('product.czech_name'), field: 'czechName', align: 'left' },
        { name: 'variety', label: this.$t('product.variety'), field: 'variety', align: 'left' },
        { name: 'potDiameterPack', label: this.$t('product.pot_diameter_pack'), field: 'potDiameterPack', align: 'left' },
        { name: 'quantity', label: this.$t('order_item.quantity'), field: 'quantity', align: 'right' },
        { name: 'pricePerPiecePack', label: this.$t('product.price_per_piece_pack'), field: 'pricePerPiecePack', align: 'right' },
        { name: 'pricePerPiecePackVAT', label: this.$t('product.price_per_piece_pack_vat'), field: 'pricePerPiecePackVAT', align: 'right' },
        { name: 'actions', label: '', align: 'center' },
      ],
      // Stĺpce pre tabuľku produktov v kontejnere "práca"
      workOrderColumns: [
        { name: 'workOrder', label: this.$t('order_item.work_order'), align: 'left', field: row => row.pluCode, },
        { name: 'name', label: this.$t('order_item.name'), align: 'left', field: row => row.name },
        { name: 'hours', label: this.$t('order_item.hours'), field: 'quantity', align: 'right' },
        { name: 'pricePerHour', label: this.$t('order_item.price_per_hour'), field: 'pricePerPiecePack', align: 'right' },
        { name: 'priceExclVAT', label: this.$t('order_item.price_excl_vat'), field: 'pricePerPiecePackVAT', align: 'right' },
        { name: 'actions', label: " ", align: 'right' },
      ],
    };
  },
  computed: {
    // Hlavná tabuľka zobrazuje iba kontajnery, ktoré nie sú "práca" (porovnanie bez ohľadu na veľkosť písmen)
    nonWorkContainers() {
      return this.containers.filter(c => c.name.toLowerCase() !== 'práca');
    },
    // Nájde kontajner s názvom "práca" (ak existuje)
    workOrderContainer() {
      return this.containers.find(c => c.name.toLowerCase() === 'práca') || null;
    },
    // Pre tabuľku pracovných objednávok mapujeme produkty z kontajnera "práca"
    workOrderRows() {
      if (!this.workOrderContainer || !this.workOrderContainer.products) {
        return [];
      }
      // Pre každý produkt z kontejneru "práca" vytvoríme nový objekt s požadovanými polami
      return this.workOrderContainer.products.map(product => ({
        pluCode: product.pluCode,
        // Ak existuje CzechName, použijeme ho, inak LatinName
        name: product.czechName || product.latinName,
        quantity: product.quantity, // hodnota orderItem.quantity
        pricePerPiecePack: product.pricePerPiecePack, // Price Per Hour
        pricePerPiecePackVAT: product.pricePerPiecePackVAT, // Price Excl. VAT
      }));
    },
  },
  methods: {
    async increaseQuantity(containerId) {
      try {
        await OrderItemsService.increaseContainerQuantity(this.currentOrderId, containerId);
        toast.success(this.$t('container.toasts.increase_success'));
        this.refreshTable();
        this.$emit('refresh-summary');
      } catch (error) {
        console.error('Error increasing quantity:', error);
        toast.error(this.$t('container.toasts.increase_error'));
      }
    },
    async decreaseQuantity(containerId) {
      try {
        const container = this.containers.find(c => c.id === containerId);
        if (container && container.quantity > 0) {
          await OrderItemsService.decreaseContainerQuantity(this.currentOrderId, containerId);
          toast.success(this.$t('container.toasts.decrease_success'));
          this.refreshTable();
          this.$emit('refresh-summary');
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
      this.$emit('refresh-summary');
    },
    handleItemCreated() {
      this.isAddItemDialogOpen = false;
      this.currentOrderId = null;
      this.refreshTable();
      this.$emit('refresh-summary');
    },
    openUpdateDialog() {
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
      this.$emit('refresh-summary');
    },
    async increaseProductQuantity(containerId, product) {
      try {
        const container = this.containers.find(c => c.id === containerId);
        if (!container) {
          throw new Error('Container not found');
        }
        const item = container.products.find(p => p.pluCode === product.pluCode);
        if (!item || !item.id) {
          throw new Error(`Order item not found for product with PLU: ${product.pluCode}`);
        }
        await OrderItemsService.increaseProductQuantity(this.currentOrderId, container.id, item.id);
        toast.success(this.$t('container.toasts.increase_success'));
        this.refreshTable();
        this.$emit('refresh-summary');
      } catch (error) {
        console.error('Error increasing product quantity:', error);
        toast.error(this.$t('container.toasts.increase_error'));
      }
    },
    async decreaseProductQuantity(containerId, product) {
      try {
        const container = this.containers.find(c => c.id === containerId);
        if (!container) {
          throw new Error('Container not found');
        }
        const item = container.products.find(p => p.pluCode === product.pluCode);
        if (!item || !item.id) {
          throw new Error(`Order item not found for product with PLU: ${product.pluCode}`);
        }
        await OrderItemsService.decreaseProductQuantity(this.currentOrderId, container.id, item.id);
        toast.success(this.$t('container.toasts.decrease_success'));
        this.refreshTable();
        this.$emit('refresh-summary');
      } catch (error) {
        console.error('Error decreasing product quantity:', error);
        toast.error(this.$t('container.toasts.decrease_error'));
      }
    },
  },
};
</script>

<style scoped>
.q-table thead {
  background-color: #f5f5f5;
  font-weight: bold;
}
.q-table[row-hover] tbody tr:hover {
  background-color: #e0f7fa;
  cursor: pointer;
}
.nested-table {
  border: 1px solid #e0e0e0;
  margin-top: 5px;
  margin-bottom: 5px;
}
.work-order-section {
  border-top: 1px solid #ccc;
  padding-top: 10px;
}
.actions-container {
  display: flex;
  justify-content: center;
  gap: 4px;
}
.work-order-info p {
  margin: 0.2em 0;
}
</style>
