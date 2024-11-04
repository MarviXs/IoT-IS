<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('product.add_product') }}</template>
    <template #default>
      <ProductForm ref="productForm" v-model="product" @on-submit="createProduct" :loading="creatingProduct" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import ProductForm, { ProductFormData } from './ProductForm.vue';
import ProductService, { CreateProductParams } from '@/api/services/ProductService';

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onCreate']);

const { t } = useI18n();

const product = ref<ProductFormData>({
  pluCode: '',
  code: '',
  latinName: '',
  czechName: undefined,
  flowerLeafDescription: undefined,
  potDiameterPack: undefined,
  pricePerPiecePack: undefined,
  pricePerPiecePackVAT: undefined,
  discountedPriceWithoutVAT: undefined,
  retailPrice: undefined,
  category: undefined,
});
const productForm = ref();

const creatingProduct = ref(false);

async function createProduct() {
  const createRequest: CreateProductParams = {
    pluCode: product.value.pluCode,
    code: product.value.code,
    latinName: product.value.latinName,
    czechName: product.value.czechName,
    flowerLeafDescription: product.value.flowerLeafDescription,
    potDiameterPack: product.value.potDiameterPack,
    pricePerPiecePack: product.value.pricePerPiecePack,
    pricePerPiecePackVAT: product.value.pricePerPiecePackVAT,
    discountedPriceWithoutVAT: product.value.discountedPriceWithoutVAT,
    retailPrice: product.value.retailPrice,
    categoryId: product.value.category!.id,
  };

  creatingProduct.value = true;
  const { data, error } = await ProductService.createProduct(createRequest);
  creatingProduct.value = false;

  if (error) {
    handleError(error, t('device.toasts.create_failed'));
    return;
  }

  emit('onCreate', data);
  isDialogOpen.value = false;

  toast.success(t('device.toasts.create_success'));
}
</script>

<style lang="scss" scoped></style>
