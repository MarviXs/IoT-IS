<template>
  <q-form @submit="onSubmit" ref="productForm">
    <q-card-section class="q-pt-none column q-gutter-md">
      <!-- Základné informácie -->
      <q-input v-model="product.code" class="col-12" :label="t('product.code')" clearable />
      <q-input v-model="product.pluCode" class="col-12" :label="t('product.pluCode')" clearable />
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
        clearable
      />
      <q-input v-model="product.variety" class="col-12" :label="t('product.variety')" clearable />

      <q-input
        v-model="product.potDiameterPack"
        class="col-12"
        :label="t('product.pot_diameter_pack')"
        type="number"
        clearable
      >
      </q-input>

      <q-input
        v-model="product.pricePerPiecePack"
        class="col-12"
        :label="t('product.price_per_piece_pack')"
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
      <SuppliersSelect v-model="product.supplier" />
      <VATCategoriesSelect v-model="product.vatCategory" />

      <!-- Kolaps pre Additional Details -->
      <q-expansion-item
        v-model="isAdditionalOpen"
        expand-separator
        :expand-icon="mdiChevronDown"
        switch-toggle-side
        class="q-mt-md"
        :label="t('product.additional_details')"
      >
        <!-- Height (cm) -->
        <q-input v-model="product.heightCm" class="col-12" :label="t('product.height_cm')" clearable>
          <template #prepend>
            <img src="/icons/volmary/height-cm.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Seeds per Thousand Plants -->
        <q-input
          v-model="product.seedsPerThousandPlants"
          class="col-12"
          :label="t('product.seeds_per_thousand_plants')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/seed-requirement-per-1000-plants.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Seeds per Thousand Pots -->
        <q-input
          v-model="product.seedsPerThousandPots"
          class="col-12"
          :label="t('product.seeds_per_thousand_pots')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/seed-requirement-per-1000-pots.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Sowing Period -->
        <q-input v-model="product.sowingPeriod" class="col-12" :label="t('product.sowing_period')" clearable>
          <template #prepend>
            <img src="/icons/volmary/sowing-period-month.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Germination Temperature (°C) -->
        <q-input
          v-model="product.germinationTemperatureC"
          class="col-12"
          :label="t('product.germination_temperature_c')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/germination-temperature-c.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Germination Time (days) -->
        <q-input
          v-model="product.germinationTimeDays"
          class="col-12"
          :label="t('product.germination_time_days')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/germination-time-days.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Cultivation Time Sowing to Plant -->
        <q-input
          v-model="product.cultivationTimeSowingToPlant"
          class="col-12"
          :label="t('product.cultivation_time_sowing_to_plant')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/cultivation-time-from-sowing-to-young-plants-weeks.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Seeds (Mio/Ha) -->
        <q-input v-model="product.seedsMioHa" class="col-12" :label="t('product.seeds_mio_ha')" clearable>
          <template #prepend>
            <img src="/icons/volmary/seed-requirement-per-ha.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Seed Spacing (cm) -->
        <q-input v-model="product.seedSpacingCM" class="col-12" :label="t('product.seed_spacing_cm')" clearable>
          <template #prepend>
            <img src="/icons/volmary/seed-spacing.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Cultivation Time for Vegetables (weeks) -->
        <q-input
          v-model="product.cultivationTimeVegetableWeek"
          class="col-12"
          :label="t('product.cultivation_time_vegetable_week')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/cultivation-time-for-vegetables-weeks.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Bulb Planting Requirement (sq.m) -->
        <q-input
          v-model="product.bulbPlantingRequirementSqM"
          class="col-12"
          :label="t('product.bulb_planting_requirement_sq_m')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/bulb-planting-requirement-sqm.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Bulb Planting Period -->
        <q-input
          v-model="product.bulbPlantingPeriod"
          class="col-12"
          :label="t('product.bulb_planting_period')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/bulb-planting-period-month.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Bulb Planting Distance (cm) -->
        <q-input
          v-model="product.bulbPlantingDistanceCm"
          class="col-12"
          :label="t('product.bulb_planting_distance_cm')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/bulb-planting-distance-cm.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Cultivation Time for Bulbs (weeks) -->
        <q-input
          v-model="product.cultivationTimeForBulbsWeeks"
          class="col-12"
          :label="t('product.cultivation_time_for_bulbs_weeks')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/cultivation-time-for-bulbs-weeks.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Number of Bulbs per Pot -->
        <q-input
          v-model="product.numberOfBulbsPerPot"
          class="col-12"
          :label="t('product.number_of_bulbs_per_pot')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/number-of-bulbs-per-pot.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Plant Spacing (cm) -->
        <q-input v-model="product.plantSpacingCm" class="col-12" :label="t('product.plant_spacing_cm')" clearable>
          <template #prepend>
            <img src="/icons/volmary/plant-spacing-in-cm.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Pot Size (cm) -->
        <q-input v-model="product.potSizeCm" class="col-12" :label="t('product.pot_size_cm')" clearable>
          <template #prepend>
            <img src="/icons/volmary/pot-size-cm.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Cultivation Time from Young Plant -->
        <q-input
          v-model="product.cultivationTimeFromYoungPlant"
          class="col-12"
          :label="t('product.cultivation_time_from_young_plant')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/cultivation-time-from-young-plant-weeks.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Cultivation Temperature (°C) -->
        <q-input
          v-model="product.cultivationTemperatureC"
          class="col-12"
          :label="t('product.cultivation_temperature_c')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/cultivation-temperature-c.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Natural Flowering Month -->
        <q-input
          v-model="product.naturalFloweringMonth"
          class="col-12"
          :label="t('product.natural_flowering_month')"
          clearable
        >
          <template #prepend>
            <img src="/icons/volmary/natural-flowering-time-months.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Planting Density -->
        <q-input v-model="product.plantingDensity" class="col-12" :label="t('product.planting_density')" clearable>
          <template #prepend>
            <img src="/icons/volmary/planting-density.svg" class="icon-img" />
          </template>
        </q-input>

        <!-- Checkboxy pre booleovské vlastnosti -->
        <div class="q-mt-md">
          <div class="row items-center q-mb-sm">
            <img src="/icons/volmary/flowering-in-first-year.svg" class="icon-img" style="margin-right: 8px" />
            <q-checkbox
              v-model="product.flowersInFirstYear"
              :label="t('product.flowers_in_first_year')"
              color="primary"
            />
          </div>
          <div class="row items-center">
            <img src="/icons/volmary/use-of-growth-inhibitors.svg" class="icon-img" style="margin-right: 8px" />
            <q-checkbox
              v-model="product.growthInhibitorsUsed"
              :label="t('product.growth_inhibitors_used')"
              color="primary"
            />
          </div>
        </div>
      </q-expansion-item>
    </q-card-section>

    <q-card-actions align="right" class="text-primary">
      <q-btn v-close-popup flat :label="t('global.cancel')" no-caps color="primary" />
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
import SuppliersSelect, { SupplierSelectData } from '../suppliers/SuppliersSelect.vue';
import VATCategoriesSelect, { VATCategorySelectData } from '../vatCategories/VATCategoriesSelect.vue';
import { mdiChevronDown } from '@quasar/extras/mdi-v7';

export interface ProductFormData {
  code: string | null | undefined;
  pluCode: string | null | undefined;
  latinName: string;
  czechName: string | null | undefined;
  flowerLeafDescription: string | null | undefined;
  variety: string;
  potDiameterPack: string | null | undefined;
  pricePerPiecePack: number | null | undefined;
  discountedPriceWithoutVAT: number | null | undefined;
  retailPrice: number | null | undefined;
  category: CategorySelectData | undefined;
  supplier: SupplierSelectData | undefined;
  vatCategory: VATCategorySelectData | undefined;
  // Nové vlastnosti
  heightCm: string | null | undefined;
  seedsPerThousandPlants: string | null | undefined;
  seedsPerThousandPots: string | null | undefined;
  sowingPeriod: string | null | undefined;
  germinationTemperatureC: string | null | undefined;
  germinationTimeDays: string | null | undefined;
  cultivationTimeSowingToPlant: string | null | undefined;
  seedsMioHa: string | null | undefined;
  seedSpacingCM: string | null | undefined;
  cultivationTimeVegetableWeek: string | null | undefined;
  bulbPlantingRequirementSqM: string | null | undefined;
  bulbPlantingPeriod: string | null | undefined;
  bulbPlantingDistanceCm: string | null | undefined;
  cultivationTimeForBulbsWeeks: string | null | undefined;
  numberOfBulbsPerPot: string | null | undefined;
  plantSpacingCm: string | null | undefined;
  potSizeCm: string | null | undefined;
  cultivationTimeFromYoungPlant: string | null | undefined;
  cultivationTemperatureC: string | null | undefined;
  naturalFloweringMonth: string | null | undefined;
  flowersInFirstYear: boolean | null | undefined;
  growthInhibitorsUsed: boolean | null | undefined;
  plantingDensity: string | null | undefined;
}

const props = defineProps<{
  loading?: boolean;
}>();

const emit = defineEmits(['onSubmit']);
const { t } = useI18n();

const product = defineModel<ProductFormData>({
  default: () => ({
    code: '',
    pluCode: '',
    latinName: '',
    czechName: '',
    flowerLeafDescription: '',
    variety: '',
    potDiameterPack: '',
    pricePerPiecePack: null,
    discountedPriceWithoutVAT: null,
    retailPrice: null,
    category: undefined,
    supplier: undefined,
    vatCategory: undefined,
    // Nové polia
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
    plantingDensity: '',
  }),
});

const isAdditionalOpen = ref(false);
const productForm = ref();

function onSubmit() {
  if (!productForm.value?.validate()) {
    return;
  }
  emit('onSubmit');
}

const latinNameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
</script>

<style scoped>
.icon-img {
  width: 32px;
  height: 32px;
  object-fit: contain;
}
</style>
