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

// Rozšírený interface pre produkt, vrátane nových vlastností
// Uistite sa, že tento interface (ProductFormData) je rovnaký, aký používate v ProductForm.vue
const product = ref<ProductFormData>({
  code: '',
  pluCode: undefined,
  eanCode: undefined,
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
  cCode: undefined,
  country: undefined,
  city: undefined,
  greenhouseNumber: undefined,
  // Nové vlastnosti – predvolené hodnoty (pre text sú prázdny reťazec, pre booleans false)
  heightCm: '',
  seedsPerThousandPlants: '',
  seedsPerThousandPots: '',
  sowingPeriod: '',
  germinationTemperatureC: '',
  germinationTimeDays: '',
  cultivationTimeSowingToPlant: '',
  seedsMioHa: '',
  seedSpacingCM: '',
  cultivationTimeVegetableWeek: '',
  bulbPlantingRequirementSqM: '',
  bulbPlantingPeriod: '',
  bulbPlantingDistanceCm: '',
  cultivationTimeForBulbsWeeks: '',
  numberOfBulbsPerPot: '',
  plantSpacingCm: '',
  potSizeCm: '',
  cultivationTimeFromYoungPlant: '',
  cultivationTemperatureC: '',
  naturalFloweringMonth: '',
  flowersInFirstYear: false,
  growthInhibitorsUsed: false,
  plantingDensity: ''
});

const productForm = ref();
const creatingProduct = ref(false);

async function createProduct() {
  // Vytvoríme CreateProductParams vrátane nových polí
  const createRequest: CreateProductParams = {
    code: product.value.code!,
    pluCode: product.value.pluCode,
    eanCode: product.value.eanCode,
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
