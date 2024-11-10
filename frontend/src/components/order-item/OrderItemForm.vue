<template>
  <q-form @submit="onSubmit" ref="itemForm">
    <q-card-section class="q-pt-none column q-gutter-md">
      <q-input
        v-model="props.orderItem.productNumber"
        @input="console.log('Product Number:', orderItem.productNumber)"
        :label="t('order_item.product_number')"
        :rules="productNumberRules"
      />
      <q-input
        v-model="props.orderItem.varietyName"
        :label="t('order_item.variety_name')"
        :rules="varietyNameRules"
      />
      <q-input
        v-model="props.orderItem.quantity"
        :label="t('order_item.quantity')"
        type="number"
        :rules="quantityRules"
      />
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
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

export interface OrderItemFormData {
  orderId: number;
  productNumber: string;
  varietyName: string;
  quantity: number;
}

// Prijímame len `orderId` ako prop, ak sa má položka automaticky priradiť k určitej objednávke
const props = defineProps<{
  orderId: number;
  loading?: boolean;
  orderItem: OrderItemFormData; // Pridajte orderItem ako vstupný parameter
}>();



const emit = defineEmits(['update:modelValue', 'onSubmit']);

const { t } = useI18n();

// Directly reference props.modelValue to initialize `item`
// Inicializácia `orderItem` s počiatočnými hodnotami


// Watch for changes in props.orderId to keep item.orderId updated

watch(() => props.orderId, (newValue) => {
  props.orderItem.orderId = newValue;
});

// Validation rules
const productNumberRules = [(val: string) => !!val || t('global.rules.required')];
const varietyNameRules = [(val: string) => !!val || t('global.rules.required')];
const quantityRules = [(val: number) => (val && val > 0) || t('global.rules.required')];

const itemForm = ref();

/// Function to handle form submission
async function onSubmit() {
  // Validate form before submission
  const isValid = await itemForm.value?.validate();
  if (isValid) {
    // Emit the order item data back to the parent component
    emit('onSubmit', props.orderItem);
  }
}
</script>

<style lang="scss" scoped></style>
