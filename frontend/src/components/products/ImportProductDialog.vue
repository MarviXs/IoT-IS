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
import type { QTableProps } from 'quasar';
import Papa, { type ParseResult } from 'papaparse';
import ProductCategorySelect from './ProductCategorySelect.vue';
import type { CategorySelectData } from '../categories/CategorySelect.vue';
import type { ProductRequest } from '@/api/services/ProductService';
import ProductService from '@/api/services/ProductService';

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

  Papa.parse(file.value, {
    preview: 5,
    complete: (result: ParseResult<string[]>) => {
      productsPreview.value = result.data;
      previewLoading.value = false;
    },
  });
});

const previewLoading = ref(false);

const productsPreview = ref<string[][]>([]);

function processFile() {
  if (!file.value) {
    return;
  }

  Papa.parse(file.value, {
    complete: (result: ParseResult<string[]>) => {
      const products = result.data
        .map((productRow) => toProductRequest(productRow))
        .filter((product): product is ProductRequest => product !== null);

      void ProductService.createProductsFromList({
        products,
      });
    },
  });
}

function toProductRequest(row: string[]): ProductRequest | null {
  const [
    _pluCode,
    code,
    latinName,
    czechName,
    flowerLeafDescription,
    potDiameterPack,
    pricePerPiecePackRaw,
    pricePerPiecePackVATRaw,
    discountedPriceWithoutVATRaw,
    retailPriceRaw,
    categoryNameRaw,
    supplierId,
    variety,
    vatCategoryId,
  ] = row;

  const categoryName = categoryNameRaw || category.value?.name;

  if (!latinName || !categoryName || !supplierId || !variety || !vatCategoryId) {
    return null;
  }

  return {
    code: code || null,
    latinName,
    czechName: czechName || null,
    flowerLeafDescription: flowerLeafDescription || null,
    potDiameterPack: potDiameterPack || null,
    pricePerPiecePack: parseNumber(pricePerPiecePackRaw),
    pricePerPiecePackVAT: parseNumber(pricePerPiecePackVATRaw),
    discountedPriceWithoutVAT: parseNumber(discountedPriceWithoutVATRaw),
    retailPrice: parseNumber(retailPriceRaw),
    categoryName,
    supplierId,
    variety,
    vatCategoryId,
  } satisfies ProductRequest;
}

function parseNumber(value: string | undefined) {
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
    field: (row: string[]) => row[0],
    align: 'left',
  },
  {
    name: 'code',
    label: t('product.code'),
    field: (row: string[]) => row[1],
    align: 'left',
  },
  {
    name: 'latinName',
    label: t('product.latin_name'),
    field: (row: string[]) => row[2],
    align: 'left',
  },
  {
    name: 'czechName',
    label: t('product.czech_name'),
    field: (row: string[]) => row[3],
    align: 'left',
  },
  {
    name: 'flowerLeafDescription',
    label: t('product.flower_leaf_description'),
    field: (row: string[]) => row[4],
    align: 'left',
  },
  {
    name: 'potDiameterPack',
    label: t('product.pot_diameter_pack'),
    field: (row: string[]) => row[5],
    align: 'left',
  },
  {
    name: 'pricePerPiecePack',
    label: t('product.price_per_piece_pack'),
    field: (row: string[]) => row[6],
    align: 'left',
  },
  {
    name: 'pricePerPiecePackVAT',
    label: t('product.price_per_piece_pack_vat'),
    field: (row: string[]) => row[7],
    align: 'left',
  },
  {
    name: 'discountedPriceWithoutVAT',
    label: t('product.discounted_price_without_vat'),
    field: (row: string[]) => row[8],
    align: 'left',
  },
  {
    name: 'retailPrice',
    label: t('product.retail_price'),
    field: (row: string[]) => row[9],
    align: 'left',
  },
]);
</script>
