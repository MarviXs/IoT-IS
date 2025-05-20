<template>
  <q-form @submit.prevent="onSubmit" ref="containerForm">
    <q-card-section class="q-pt-none column q-gutter-md">
      <q-input
        :model-value="containerData.name"
        @update:model-value="(val) => updateContainerData('name', val)"
        :label="t('order_item.container_name')"
        :rules="nameRules"
        outlined
        dense
      />

      <q-input
        :model-value="containerData.quantity"
        @update:model-value="(val) => updateContainerData('quantity', Number(val))"
        :label="t('order_item.quantity')"
        type="number"
        :rules="quantityRules"
        outlined
        dense
      />
    </q-card-section>

    <q-card-actions align="right" class="text-primary">
      <q-btn flat :label="t('global.cancel')" no-caps @click="cancel" />
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
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';

interface AddOrderContainerRequest {
  orderId: string;
  name: string;
  quantity: number;
  pricePerContainer: number;
}

const props = defineProps<{
  containerData: AddOrderContainerRequest;
  loading?: boolean;
}>();

const emit = defineEmits(['update:containerData', 'on-submit', 'cancel']);
const { t } = useI18n();

const containerForm = ref<HTMLFormElement>();

// Validation rules
const nameRules = [(val: string) => (val && val.trim().length > 0) || t('global.rules.required')];
const quantityRules = [(val: number) => (val && val > 0) || t('global.rules.required')];

function updateContainerData(key: keyof AddOrderContainerRequest, value: unknown) {
  const newData = { ...props.containerData, [key]: value };
  emit('update:containerData', newData);
}

function onSubmit() {
  // Skúsime validovať formulár cez QForm API
  const form = containerForm.value;
  if (form) {
    // QForm validácia - Quasar automaticky validuje inputy via rules
    // form.validate() vráti true/false, ak je všetko ok, tak true
    // Ale QForm v Quasare nevracia priamo boolean, treba skontrolovať chyby.
    // V Quasare stačí if (!form.validate()) { return }
    (form as any).validate().then((valid: boolean) => {
      if (!valid) return;
      emit('on-submit');
    });
  } else {
    emit('on-submit');
  }
}

function cancel() {
  emit('cancel');
}
</script>
