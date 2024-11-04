<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('product.edit_product') }}</template>
    <template #default>
      <ProductForm ref="productForm" v-model="product" :loading="updatingProduct" @on-submit="updateProduct" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import ProductForm, { ProductFormData } from './ProductForm.vue';
import ProductService, { UpdateProductRequest } from '@/api/services/ProductService';
import CategoryService from '@/api/services/CategoryService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  productId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onUpdate']);

const { t } = useI18n();

const product = ref<ProductFormData>({} as ProductFormData);
async function getProduct() {
  const { data, error } = await ProductService.getProduct(props.productId);
  if (error) {
    handleError(error, t('command.toasts.load_failed'));
    return;
  }

  product.value = {
    code: data.code,
    pluCode: data.pluCode,
    latinName: data.latinName,
    czechName: data.czechName,
    flowerLeafDescription: data.flowerLeafDescription,
    potDiameterPack: data.potDiameterPack,
    pricePerPiecePack: data.pricePerPiecePack,
    pricePerPiecePackVAT: data.pricePerPiecePackVAT,
    discountedPriceWithoutVAT: data.discountedPriceWithoutVAT,
    retailPrice: data.retailPrice,
  };

  CategoryService.getCategory(data.categoryId)
    .then((response) => {
      if (response.data) {
        product.value.category = {
          id: response.data.id,
          name: response.data.name,
        };
      }
    })
    .catch((error) => {
      handleError(error, t('product.toasts.load_failed'));
    });
}

const updatingProduct = ref(false);
const productForm = ref();

async function updateProduct() {
  const updateRequest: UpdateProductRequest = {
    code: product.value.code,
    pluCode: product.value.pluCode,
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

  updatingProduct.value = true;
  const { data, error } = await ProductService.updateProduct(props.productId, updateRequest);
  updatingProduct.value = false;

  if (error) {
    handleError(error, t('product.toasts.update_failed'));
    return;
  }

  emit('onUpdate', data);
  isDialogOpen.value = false;
  toast.success(t('product.toasts.update_success'));
}

watch(
  () => isDialogOpen.value,
  (isOpen) => {
    if (isOpen) {
      product.value = {} as ProductFormData;
      getProduct();
    }
  },
  { immediate: true },
);
</script>

<style lang="scss" scoped></style>
