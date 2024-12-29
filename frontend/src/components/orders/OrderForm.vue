<template>
  <q-form @submit="onSubmit" ref="orderForm">
    <q-card-section class="q-pt-none column q-gutter-md">
      <!-- Changed customerName to customerId to match backend requirements -->
      <q-input v-model="order.customerId" :rules="customerIdRules" autofocus :label="t('order.customer_id')" />

      <q-input v-model="order.contactPhone" :rules="phoneRules" :label="t('order.contact_phone')" />

      <q-input
        v-model="order.orderDate"
        :label="t('order.order_date')"
        mask="####-##-##"
        hint="YYYY-MM-DD"
        type="date"
      />

      <q-input
        v-model="order.deliveryWeek"
        :rules="deliveryWeekRules"
        :label="t('order.delivery_week')"
        type="number"
      />

      <q-select
        v-model="order.paymentMethod"
        :options="paymentMethods"
        :label="t('order.payment_method.label')"
        :rules="paymentMethodRules"
      />
      <q-input v-model="order.note" :label="t('order.note')" type="textarea" />

      <!-- Additional fields can be added as needed based on the database model -->
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
import { isFormValid } from '@/utils/form-validation';

export interface OrderFormData {
  customerId: string; // Changed to customerId to match backend requirements
  contactPhone: string;
  deliveryWeek: number; // Added deliveryWeek to match backend requirements
  orderDate: string;
  paymentMethod: string | { label: string; value: string };
  note: string;
}

const props = defineProps<{
  loading?: boolean;
}>();

const emit = defineEmits(['onSubmit']);
const { t } = useI18n();

const order = defineModel<OrderFormData>({ required: true });

// Validation rules for each field
const customerIdRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const phoneRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const paymentMethodRules = [(val: string) => !!val || t('global.rules.required')];
const deliveryWeekRules = [(val: number) => (val && val > 0) || t('global.rules.required')];

// Payment method options (can be customized as needed)
const paymentMethods = [
  { label: t('order.payment_method.cash'), value: 'Cash' },
  { label: t('order.payment_method.card'), value: 'Card' },
  { label: t('order.payment_method.transfer'), value: 'Transfer' },
];

// Reference to the form for validation purposes
const orderForm = ref();

function onSubmit() {
  if (!orderForm.value?.validate()) {
    return;
  }
  emit('onSubmit');
}
</script>

<style lang="scss" scoped></style>
