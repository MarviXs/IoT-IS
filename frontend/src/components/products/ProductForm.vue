<template>
  <q-form @submit="onSubmit" ref="productForm">
    <q-card-section class="q-pt-none column q-gutter-md">
      <q-input v-model="product.code" class="col-12" :label="t('product.code')" clearable />
      <q-input
        v-model="product.pluCode"
        class="col-12"
        :label="t('product.plu_code')"
        clearable
        :rules="pluCodeRules"
      />
      <q-input
        v-model="product.latinName"
        class="col-12"
        :label="t('product.latin_name')"
        clearable
        :rules="latinNameRules"
      />
      <q-input v-model="product.czechName" class="col-12" :label="t('product.czech_name')" clearable />
      <q-input
        v-model="product.flowerLeafDescription"
        class="col-12"
        :label="t('product.flower_leaf_description')"
        type="number"
        clearable
      />
      <q-input
        v-model="product.potDiameterPack"
        class="col-12"
        :label="t('product.pot_diameter_pack')"
        type="number"
        clearable
      />
      <q-input
        v-model="product.pricePerPiecePack"
        class="col-12"
        :label="t('product.price_per_piece_pack')"
        type="number"
        clearable
      />
      <q-input
        v-model="product.pricePerPiecePackVAT"
        class="col-12"
        :label="t('product.price_per_piece_pack_vat')"
        type="number"
        clearable
      />
      <q-input
        v-model="product.discountedPriceWithoutVAT"
        class="col-12"
        :label="t('product.discounted_price_without_vat')"
        type="number"
        clearable
      />
      <q-input
        v-model="product.retailPrice"
        class="col-12"
        :label="t('product.retail_price')"
        type="number"
        clearable
      />
      <CategorySelect v-model="product.category" />
    </q-card-section>
    <q-card-actions align="right" class="text-primary">
      <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
      <q-btn
        unelevated
        color="primary"
        :label="t('global.save')"
        type="submit"
        no-caps
        padding="6px 20px"
        :loading="props.loading"
      />
    </q-card-actions>
  </q-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import CategorySelect, { CategorySelectData } from '../categories/CategorySelect.vue';

export interface ProductFormData {
  pluCode: string;
  code: string;
  latinName: string;
  czechName: string | null | undefined;
  flowerLeafDescription: string | null | undefined;
  potDiameterPack: string | null | undefined;
  pricePerPiecePack: number | null | undefined;
  pricePerPiecePackVAT: number | null | undefined;
  discountedPriceWithoutVAT: number | null | undefined;
  retailPrice: number | null | undefined;
  category?: CategorySelectData;
}

const props = defineProps<{
  loading?: boolean;
}>();

const emit = defineEmits(['onSubmit']);

const { t } = useI18n();

const product = defineModel<ProductFormData>({ default: () => ({ pluCode: '', code: '' }) });

const productForm = ref();

function onSubmit() {
  if (!productForm.value?.validate()) {
    return;
  }
  emit('onSubmit');
}

const pluCodeRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const latinNameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
</script>
