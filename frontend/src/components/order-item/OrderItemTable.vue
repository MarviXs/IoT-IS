<template>
  <div class="order-item-table">
    <q-table
      :rows="containers"
      :columns="parentColumns"
      row-key="id"
      flat
      dense
      separator="horizontal"
    >
      <template #body="props">
        <q-tr :props="props" class="parent-row">
          <q-td :props="props" key="id">{{ props.row.id }}</q-td>
          <q-td :props="props" key="name">{{ props.row.name }}</q-td>
          <q-td :props="props" key="quantity">{{ props.row.quantity }}</q-td>
          <q-td :props="props" key="actions" class="action-column">
            <div class="product-quantity-actions">
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
            </div>
          </q-td>
          <q-td :props="props" key="oneContainer">{{ calculateoneContainer(props.row) }}</q-td>
          <q-td :props="props" key="total">{{ calculateTotal(props.row) }}</q-td>
          <q-td class="final-actions">
            <div class="product-quantity-actions">
            <q-btn
              flat
              round
              size="sm"
              :icon="mdiPlusBox"
              color="grey-color"
              @click="addItem(props.row.id)"
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
            >
            </q-table>
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </div>
</template>

<script>
import { mdiPlus, mdiMinus, mdiTrashCan, mdiPencil, mdiPlusBox } from '@quasar/extras/mdi-v7';

export default {
  name: "OrderItemTable",
  props: {
    containers: {
      type: Array,
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
      parentColumns: [
        //{ name: "id", label: this.$t('order_item.id'), field: "id", align: "left" },
        { name: "name", label: this.$t('global.name'), field: "name", align: "left" },
        { name: "quantity", label: this.$t('order_item.quantity'), field: "quantity", align: "center" },
        { name: "actions", label: "", align: "center" },
        { name: "oneContainer", label: this.$t('order_item.oneContainer'), align: "right" },
        { name: "total", label: this.$t('order_item.total'), align: "right" },
        { name: "finalActions", label: this.$t('global.actions'), align: "center" },
      ],
      nestedColumns: [
        { name: "pluCode", label: this.$t('product.plu_code'), field: "pluCode", align: "left" },
        { name: "latinName", label: this.$t('product.latin_name'), field: "latinName", align: "left" },
        { name: "czechName", label: this.$t('product.czech_name'), field: "czechName", align: "left" },
        { name: "variety", label: this.$t('product.variety'), field: "variety", align: "left" },
        { name: "potDiameterPack", label: this.$t('product.pot_diameter_pack'), field: "potDiameterPack", align: "left" },
        { name: "quantity", label: this.$t('order_item.quantity'), field: "quantity", align: "right" },
        { name: "pricePerPiecePack", label: this.$t('product.price_per_piece_pack'), field: "pricePerPiecePack", align: "right" },
        { name: "pricePerPiecePackVAT", label: this.$t('product.price_per_piece_pack_vat'), field: "pricePerPiecePackVAT", align: "right" },
],
    };
  },
  methods: {
    increaseQuantity(containerId) {
      const container = this.containers.find((c) => c.id === containerId);
      if (container) container.quantity += 1;
    },
    decreaseQuantity(containerId) {
      const container = this.containers.find((c) => c.id === containerId);
      if (container && container.quantity > 0) container.quantity -= 1;
    },
    addItem(containerId) {
      alert(this.$t('order_item.toasts.add_success'));
    },
    openUpdateDialog(containerId) {
      alert(this.$t('global.edit'));
    },
    openDeleteDialog(containerId) {
      alert(this.$t('order_item.toasts.delete_success'));
    },
    calculateoneContainer(container) {
      if (!container.products.length) return "0.00";
      const total = container.products.reduce((sum, product) => sum + parseFloat(product.cenaSDPH || 0), 0);
      return (total / container.products.length).toFixed(2);
    },
    calculateTotal(container) {
      if (!container.products.length) return "0.00";
      return (
        container.quantity *
        container.products.reduce((sum, product) => sum + parseFloat(product.cenaSDPH || 0) * product.pocet, 0)
      ).toFixed(2);
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
  gap: 8px;
}

.final-actions {
  display: flex;
  justify-content: center;
  gap: 12px;
}

.q-btn {
  font-size: 16px;
}
</style>
