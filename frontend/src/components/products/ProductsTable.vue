<template>
  <q-table
    v-model:pagination="pagination"
    :rows="products"
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
        <q-btn :icon="mdiPencil" color="grey-color" flat round @click.stop="openUpdateDialog(propsActions.row.id)"
          ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.edit') }}
          </q-tooltip>
        </q-btn>
        <q-btn :icon="mdiTrashCan" color="grey-color" flat round @click.stop="openDeleteDialog(propsActions.row.id)"
          ><q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.delete') }}
          </q-tooltip>
        </q-btn>
        <q-btn
          icon="cloud_download"
          color="grey-color"
          flat
          round
          @click.stop="downloadProductAsDocx(propsActions.row)"
        >
          <q-tooltip content-style="font-size: 11px" :offset="[0, 4]">
            {{ t('global.download') }}
          </q-tooltip>
        </q-btn>
      </q-td>
    </template>
  </q-table>
  <EditProductDialog
    v-if="productToUpdate"
    v-model="isUpdateDialogOpen"
    :product-id="productToUpdate"
    @on-update="emit('onChange')"
  />
  <DeleteProductDialog
    v-if="productToDelete"
    v-model="isDeleteDialogOpen"
    :product-id="productToDelete"
    @on-deleted="emit('onChange')"
  />
</template>

<script setup lang="ts">
import { QTableProps } from 'quasar';
import { PropType, computed, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { mdiFileSearchOutline, mdiPencil, mdiTrashCan } from '@quasar/extras/mdi-v7';
import { PaginationClient } from '@/models/Pagination';
import EditProductDialog from './EditProductDialog.vue';
import DeleteProductDialog from './DeleteProductDialog.vue';
import { Document, Packer, Paragraph, TextRun, AlignmentType, PageOrientation } from 'docx';

const props = defineProps({
  loading: {
    type: Boolean,
    required: true,
  },
});

const products = defineModel<Array<any>>('products', {
  type: Array as PropType<Array<any>>,
  default: [],
});

const pagination = defineModel<PaginationClient>('pagination');

const emit = defineEmits(['onChange', 'onRequest']);

const { t } = useI18n();

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'code',
    label: t('product.code'),
    field: 'code',
    align: 'left',
    sortable: true,
  },
  {
    name: 'pluCode',
    label: t('product.plu_code'),
    field: 'pluCode',
    align: 'left',
    sortable: true,
  },
  {
    name: 'czechName',
    label: t('product.czech_name'),
    field: 'czechName',
    sortable: true,
    align: 'left',
  },
  {
    name: 'retailPrice',
    label: t('product.retail_price'),
    field: 'retailPrice',
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

const isUpdateDialogOpen = ref(false);
const productToUpdate = ref<string>();
function openUpdateDialog(productId: string) {
  productToUpdate.value = productId;
  isUpdateDialogOpen.value = true;
}

const isDeleteDialogOpen = ref(false);
const productToDelete = ref<string>();
function openDeleteDialog(productId: string) {
  isDeleteDialogOpen.value = true;
  productToDelete.value = productId;
}

function downloadProductAsDocx(product: any) {
  const doc = new Document({
    sections: [
      {
        properties: {
          page: {
            size: {
              orientation: PageOrientation.PORTRAIT,
              width: 1984, // 7 cm width (567 * 7)
              height: 1728, // 3 cm height
            },
            margin: {
              top: 0,
              bottom: 0,
              left: 0,
              right: 0,
            },
          },
        },
        children: [
          new Paragraph({
            alignment: AlignmentType.CENTER,
            children: [
              new TextRun({
                text: `${product.latinName ?? ''} – ${product.czechName ?? ''}`,
                bold: true,
                size: 14, 
              }),
            ],
            spacing: { after: 100 },
          }),
          new Paragraph({
            alignment: AlignmentType.LEFT,
            children: [
              new TextRun({ text: `EAN: ${product.eanCode ?? ''}`, size: 14 }),
            ],
          }),
          new Paragraph({
            alignment: AlignmentType.LEFT,
            children: [
              new TextRun({ text: `PLU: ${product.pluCode ?? ''}`, size: 14 }),
            ],
          }),
          new Paragraph({
            alignment: AlignmentType.LEFT,
            children: [
              new TextRun({ text: `Variety: ${product.variety ?? ''}`, size: 14 }),
            ],
          }),
        ],
      },
    ],
  });

  Packer.toBlob(doc).then((blob) => {
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `produkt_${product.code || 'data'}.docx`;
    link.click();
  });
}


</script>
