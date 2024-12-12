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
  code: '',
  pluCode: undefined,
  latinName: '',
  czechName: undefined,
  flowerLeafDescription: undefined,
  variety: '',
  potDiameterPack: undefined,
  pricePerPiecePack: undefined,
  discountedPriceWithoutVAT: undefined,
  retailPrice: undefined,
  category: undefined,
  supplier: undefined,
  vatCategory: undefined,
});
const productForm = ref();

const creatingProduct = ref(false);

async function createProduct() {
  const createRequest: CreateProductParams = {
    code: product.value.code!,
    pluCode: product.value.pluCode,
    latinName: product.value.latinName,
    czechName: product.value.czechName,
    flowerLeafDescription: product.value.flowerLeafDescription,
    potDiameterPack: product.value.potDiameterPack,
    pricePerPiecePack: product.value.pricePerPiecePack,
    discountedPriceWithoutVAT: product.value.discountedPriceWithoutVAT,
    retailPrice: product.value.retailPrice,
    variety: product.value.variety,
    categoryId: product.value.category?.id!,
    vatCategoryId: product.value.vatCategory?.id!,
    supplierId: product.value.supplier?.id!,
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
