<template>
  <q-form @submit="onSubmit" ref="itemContainerForm">
    <q-card-section class="q-pt-none column q-gutter-md">
      
      <q-input
        v-model="props.orderItem.name"
        :label="t('global.name')"
        :rules="nameRules"
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
  name: string;
  quantity: number | 0;
  pricePerContainer: number | 0;
  total: number |0;
}



// Prijímame len `orderId` ako prop, ak sa má položka automaticky priradiť k určitej objednávke
const props = defineProps<{
  orderId: number;
  loading?: boolean;
  orderItem: OrderItemFormData;
}>();

const emit = defineEmits(['update:modelValue', 'onSubmit']);
const { t } = useI18n();

watch(() => props.orderId, (newValue) => {
  props.orderItem.orderId = newValue;
});

// Validation rules
const nameRules = [(val: string) => !!val || t('global.rules.required')];
const quantityRules = [(val: number) => (val && val >= 0) || t('global.rules.required')];

const itemForm = ref();

async function onSubmit() {
  const isValid = await itemForm.value?.validate();
  if (isValid) {
    emit('onSubmit', props.orderItem);
  }
}
</script>

<style lang="scss" scoped></style>
