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

const isDialogOpen = defineModel<boolean>();

const props = defineProps({
  productId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onUpdate']);

const { t } = useI18n();

// Inicializácia objektu produktu s novými vlastnosťami
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
    eanCode: null,
    latinName: data.latinName,
    czechName: data.czechName,
    flowerLeafDescription: data.flowerLeafDescription,
    variety: data.variety,
    potDiameterPack: data.potDiameterPack,
    pricePerPiecePack: data.pricePerPiecePack,
    discountedPriceWithoutVAT: data.discountedPriceWithoutVAT,
    retailPrice: data.retailPrice,
    category: data.category as ProductFormData['category'],
    supplier: data.supplier as ProductFormData['supplier'],
    vatCategory: data.vatCategory as ProductFormData['vatCategory'],
    cCode: data.cCode,
    country: data.country,
    city: data.city,
    greenhouseNumber: data.greenhouseNumber,
    // Nové vlastnosti
    heightCm: data.heightCm,
    seedsPerThousandPlants: data.seedsPerThousandPlants,
    seedsPerThousandPots: data.seedsPerThousandPots,
    sowingPeriod: data.sowingPeriod,
    germinationTemperatureC: data.germinationTemperatureC,
    germinationTimeDays: data.germinationTimeDays,
    cultivationTimeSowingToPlant: data.cultivationTimeSowingToPlant,
    seedsMioHa: data.seedsMioHa,
    seedSpacingCM: data.seedSpacingCM,
    cultivationTimeVegetableWeek: data.cultivationTimeVegetableWeek,
    bulbPlantingRequirementSqM: data.bulbPlantingRequirementSqM,
    bulbPlantingPeriod: data.bulbPlantingPeriod,
    bulbPlantingDistanceCm: data.bulbPlantingDistanceCm,
    cultivationTimeForBulbsWeeks: data.cultivationTimeForBulbsWeeks,
    numberOfBulbsPerPot: data.numberOfBulbsPerPot,
    plantSpacingCm: data.plantSpacingCm,
    potSizeCm: data.potSizeCm,
    cultivationTimeFromYoungPlant: data.cultivationTimeFromYoungPlant,
    cultivationTemperatureC: data.cultivationTemperatureC,
    naturalFloweringMonth: data.naturalFloweringMonth,
    flowersInFirstYear: data.flowersInFirstYear,
    growthInhibitorsUsed: data.growthInhibitorsUsed,
    plantingDensity: data.plantingDensity
  };
}

const updatingProduct = ref(false);
const productForm = ref();

async function updateProduct() {
  const updateRequest: UpdateProductRequest = {
    pluCode: product.value.pluCode!,
    eanCode: product.value.eanCode,
    code: product.value.code,
    latinName: product.value.latinName,
    czechName: product.value.czechName,
    flowerLeafDescription: product.value.flowerLeafDescription,
    variety: product.value.variety,
    potDiameterPack: product.value.potDiameterPack,
    pricePerPiecePack: product.value.pricePerPiecePack,
    discountedPriceWithoutVAT: product.value.discountedPriceWithoutVAT,
    retailPrice: product.value.retailPrice,
    categoryId: product.value.category!.id,
    vatCategoryId: product.value.vatCategory!.id,
    supplierId: product.value.supplier!.id,
    cCode: product.value.cCode,
    country: product.value.country,
    city: product.value.city,
    greenhouseNumber: product.value.greenhouseNumber,
    // Nové vlastnosti:
    heightCm: product.value.heightCm,
    seedsPerThousandPlants: product.value.seedsPerThousandPlants,
    seedsPerThousandPots: product.value.seedsPerThousandPots,
    sowingPeriod: product.value.sowingPeriod,
    germinationTemperatureC: product.value.germinationTemperatureC,
    germinationTimeDays: product.value.germinationTimeDays,
    cultivationTimeSowingToPlant: product.value.cultivationTimeSowingToPlant,
    seedsMioHa: product.value.seedsMioHa,
    seedSpacingCM: product.value.seedSpacingCM,
    cultivationTimeVegetableWeek: product.value.cultivationTimeVegetableWeek,
    bulbPlantingRequirementSqM: product.value.bulbPlantingRequirementSqM,
    bulbPlantingPeriod: product.value.bulbPlantingPeriod,
    bulbPlantingDistanceCm: product.value.bulbPlantingDistanceCm,
    cultivationTimeForBulbsWeeks: product.value.cultivationTimeForBulbsWeeks,
    numberOfBulbsPerPot: product.value.numberOfBulbsPerPot,
    plantSpacingCm: product.value.plantSpacingCm,
    potSizeCm: product.value.potSizeCm,
    cultivationTimeFromYoungPlant: product.value.cultivationTimeFromYoungPlant,
    cultivationTemperatureC: product.value.cultivationTemperatureC,
    naturalFloweringMonth: product.value.naturalFloweringMonth,
    flowersInFirstYear: product.value.flowersInFirstYear,
    growthInhibitorsUsed: product.value.growthInhibitorsUsed,
    plantingDensity: product.value.plantingDensity
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
  { immediate: true }
);
</script>

<style lang="scss" scoped>
/* Prípadné doplnkové štýly */
</style>
