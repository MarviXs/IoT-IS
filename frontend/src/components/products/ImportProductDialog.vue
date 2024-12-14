<template>
  <dialog-common v-model="isDialogOpen" min-width="40rem" @hide="init">
    <template #title>{{ t('product.edit_product') }}</template>
    <template #default>
      <div class="tw-flex tw-gap-4 tw-justify-between tw-items-baseline tw-ml-2">
        <q-file filled bottom-slots v-model="file" label="Label" counter class="col-12">
          <template v-slot:prepend>
            <q-icon :name="mdiCloudUpload" @click.stop.prevent />
          </template>
          <template v-slot:append>
            <q-icon :name="mdiClose" @click.stop.prevent="file = null" class="cursor-pointer" />
          </template>
        </q-file>
        <ProductCategorySelect v-model="category" />
      </div>
      <q-table
        :rows="productsPreview"
        :columns="columns"
        :loading="previewLoading"
        flat
        hide-pagination
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
      >
      </q-table>
      <div class="tw-flex tw-justify-end">
        <q-btn flat :label="t('global.cancel')" no-caps @click="closeDialog" />
        <q-btn unelevated color="primary" class="tw-align" label="Process" no-caps @click="processFile" />
      </div>
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import { mdiCloudUpload, mdiClose } from '@quasar/extras/mdi-v7';
import { QTableProps } from 'quasar';
//@ts-ignore
import Papa from 'papaparse';
import type { ParseResult } from 'node_modules/@types/papaparse';
import ProductCategorySelect from './ProductCategorySelect.vue';
import { CategorySelectData } from '../categories/CategorySelect.vue';
import ProductService, { ProductRequest } from '@/api/services/ProductService';

const isDialogOpen = defineModel<boolean>();

const file = ref<File | null>(null);
const category = ref<CategorySelectData | null>(null);

const { t } = useI18n();

function closeDialog() {
  init();
  isDialogOpen.value = false;
}

function init() {
  file.value = null;
  productsPreview.value = [];
  category.value = null;
}

watch(file, () => {
  if (!file.value) {
    return;
  }

  previewLoading.value = true;

  //@ts-ignore
  Papa.parse(file.value, {
    preview: 5,
    complete: (result: ParseResult<String>) => {
      productsPreview.value = result.data;
      previewLoading.value = false;
    },
  });
});

const previewLoading = ref(false);

const productsPreview = ref<Array<String>>([]);

function processFile() {
  if (!file.value) {
    return;
  }

  //@ts-ignore
  Papa.parse(file.value, {
    complete: (result: ParseResult<String>) => {
      ProductService.createProductsFromList({
        products: result.data.map((product) => ({
          pluCode: product[0],
          code: product[1],
          latinName: product[2],
          czechName: product[3],
          flowerLeafDescription: product[4],
          potDiameterPack: product[5],
          pricePerPiecePack: parseNumber(product[6]),
          pricePerPiecePackVAT: parseNumber(product[7]),
          discountedPriceWithoutVAT: parseNumber(product[8]),
          retailPrice: parseNumber(product[9]),
        })) as Array<ProductRequest>,
        categoryId: category.value!.id,
      });
    },
  });
}

function parseNumber(value: String) {
  if (!value) {
    return null;
  }
  const match = value.match(/[\d,]+/);
  return match ? parseFloat(match[0].replace(',', '.')) : null;
}

const columns = computed<QTableProps['columns']>(() => [
  {
    name: 'pluCode',
    label: t('product.plu_code'),
    field: (row: Array<string>) => row[0],
    align: 'left',
  },
  {
    name: 'code',
    label: t('product.code'),
    field: (row: Array<string>) => row[1],
    align: 'left',
  },
  {
    name: 'latinName',
    label: t('product.latin_name'),
    field: (row: Array<string>) => row[2],
    align: 'left',
  },
  {
    name: 'czechName',
    label: t('product.czech_name'),
    field: (row: Array<string>) => row[3],
    align: 'left',
  },
  {
    name: 'flowerLeafDescription',
    label: t('product.flower_leaf_description'),
    field: (row: Array<string>) => row[4],
    align: 'left',
  },
  {
    name: 'potDiameterPack',
    label: t('product.pot_diameter_pack'),
    field: (row: Array<string>) => row[5],
    align: 'left',
  },
  {
    name: 'pricePerPiecePack',
    label: t('product.price_per_piece_pack'),
    field: (row: Array<string>) => row[6],
    align: 'left',
  },
  {
    name: 'pricePerPiecePackVAT',
    label: t('product.price_per_piece_pack_vat'),
    field: (row: Array<string>) => row[7],
    align: 'left',
  },
  {
    name: 'discountedPriceWithoutVAT',
    label: t('product.discounted_price_without_vat'),
    field: (row: Array<string>) => row[8],
    align: 'left',
  },
  {
    name: 'retailPrice',
    label: t('product.retail_price'),
    field: (row: Array<string>) => row[9],
    align: 'left',
  },
]);
</script>
