<template>
  <q-form @submit="onSubmit" ref="formRef">
    <q-card-section class="q-pt-none column q-gutter-md">
      <q-input v-model="localOrder.customerId" :rules="customerIdRules" :label="t('order.customer_id')" />
      <q-input v-model="localOrder.contactPhone" :rules="phoneRules" :label="t('order.contact_phone')" />
      <q-input
        v-model="localOrder.orderDate"
        :label="t('order.order_date')"
        mask="####-##-##"
        hint="YYYY-MM-DD"
        type="date"
      />
      <q-input
        v-model="localOrder.deliveryWeek"
        :rules="deliveryWeekRules"
        :label="t('order.delivery_week')"
        type="number"
      />
      <q-select
        v-model="localOrder.paymentMethod"
        :options="paymentMethods"
        :label="t('order.payment_method.label')"
        :rules="paymentMethodRules"
      />
      <q-input v-model="localOrder.note" :label="t('order.note')" type="textarea" />
    </q-card-section>

    <q-card-actions align="right" class="text-primary">
      <q-btn flat :label="t('global.cancel')" no-caps @click="$emit('cancel')" />
      <q-btn
        unelevated
        color="primary"
        :label="t('global.save')"
        type="submit"
        no-caps
        padding="6px 20px"
        :loading="loading"
      />
    </q-card-actions>
  </q-form>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

interface PaymentMethodOption {
  label: string;
  value: string;
}

interface UpdateOrderFormData {
  customerId: number;
  contactPhone: string;
  orderDate: string;
  deliveryWeek: number;
  paymentMethod: string | PaymentMethodOption;
  note: string;
}

const props = defineProps<{
  order: {
    id: number;
    customerName: string;
    orderDate: string;
    deliveryWeek: number;
    paymentMethod: string;
    contactPhone: string;
    note: string;
    customerId: number;
  };
  loading?: boolean;
}>();

const emit = defineEmits(['on-submit', 'cancel']);

const { t } = useI18n();

const formRef = ref();

const localOrder = ref({
  customerId: props.order.customerId || 0,
  contactPhone: props.order.contactPhone,
  orderDate: props.order.orderDate.slice(0, 10), // ak je vo formáte ISO, odstránime časť
  deliveryWeek: props.order.deliveryWeek,
  paymentMethod: props.order.paymentMethod,
  note: props.order.note,
});

// Keď sa zmení prop order, aktualizuj localOrder
watch(
  () => props.order,
  (newVal) => {
    localOrder.value = {
      customerId: newVal.customerId || 0,
      contactPhone: newVal.contactPhone,
      orderDate: newVal.orderDate.slice(0, 10),
      deliveryWeek: newVal.deliveryWeek,
      paymentMethod: newVal.paymentMethod,
      note: newVal.note,
    };
  },
  { immediate: true },
);

// Validation rules
const customerIdRules = [(val: number) => (val && val > 0) || t('global.rules.required')];
const phoneRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const paymentMethodRules = [(val: string) => !!val || t('global.rules.required')];
const deliveryWeekRules = [(val: number) => (val && val > 0) || t('global.rules.required')];

// Payment method options
const paymentMethods = [
  { label: t('order.payment_method.cash'), value: 'Cash' },
  { label: t('order.payment_method.card'), value: 'Card' },
  { label: t('order.payment_method.transfer'), value: 'Transfer' },
];

function onSubmit() {
  if (!formRef.value?.validate()) {
    return;
  }
  // Emit updated data
  emit('on-submit', {
    customerId: localOrder.value.customerId,
    contactPhone: localOrder.value.contactPhone,
    paymentMethod: localOrder.value.paymentMethod,
    deliveryWeek: localOrder.value.deliveryWeek,
    orderDate: localOrder.value.orderDate,
    note: localOrder.value.note,
  });
}
</script>
