<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('product.edit_product') }}</template>
    <template #default>
      <div class="upload-container">
        <q-file filled bottom-slots v-model="file" label="Label" counter class="col-12">
          <template v-slot:prepend>
            <q-icon :name="mdiCloudUpload" @click.stop.prevent />
          </template>
          <template v-slot:append>
            <q-icon :name="mdiClose" @click.stop.prevent="file = null" class="cursor-pointer" />
          </template>
          <template v-slot:hint> Field hint </template>
        </q-file>
        <q-btn unelevated color="primary" class="" label="Process" no-caps @click="processFile" />
      </div>
      <q-table
        :rows="productsPreview"
        :columns="columns"
        :loading="previewLoading"
        flat
        :no-data-label="t('table.no_data_label')"
        :loading-label="t('table.loading_label')"
      >
      </q-table>
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

const isDialogOpen = defineModel<boolean>();

const file = ref<File | null>(null);

const { t } = useI18n();

watch(file, () => {
  if (!file.value) {
    return;
  }

  previewLoading.value = true;

  //@ts-ignore
  Papa.parse(file.value, {
    preview: 10,
    complete: (result: ParseResult<PreviewProduct>) => {
      productsPreview.value = result.data;
      previewLoading.value = false;
    },
  });
});

const previewLoading = ref(false);

interface PreviewProduct {
  pluCode: string;
  code: string;
  latinName: string;
  czechName: string;
  flowerLeafDescription: string;
  potDiameterPack: string;
  pricePerPiecePack: string;
  pricePerPiecePackVAT: string;
  discountedPriceWithoutVAT: string;
  retailPrice: string;
}

const productsPreview = ref<PreviewProduct[]>([]);

function processFile() {
  if (!file.value) {
    return;
  }
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

<style scoped>
.upload-container {
  display: flex;
  flex-direction: row;
  gap: 1rem;
  align-items: center;
}

.upload-container .q-button {
  max-height: 4rem;
}
</style>
