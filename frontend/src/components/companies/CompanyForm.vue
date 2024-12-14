<template>
  <q-form @submit="onSubmit" ref="companyForm">
    <q-card-section class="q-pt-none column q-gutter-md">
      <q-input v-model="company.title" class="col-12" :label="t('company.name')" clearable :rules="titleRules" />
      <q-input v-model="company.ic" class="col-12" :label="t('company.IC')" clearable :rules="icRules" />
      <q-input v-model="company.dic" class="col-12" :label="t('company.DIC')" clearable />
      <q-input v-model="company.street" class="col-12" :label="t('company.street')" clearable />
      <q-input v-model="company.psc" class="col-12" :label="t('company.psc')" clearable />
      <q-input v-model="company.city" class="col-12" :label="t('company.city')" clearable />
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

export interface CompanyFormData {
  title: string;
  ic: string;
  dic: string | null | undefined;
  street: string | null | undefined;
  psc: string | null | undefined;
  city: string | null | undefined;
}

const props = defineProps<{
  loading?: boolean;
}>();

const emit = defineEmits(['onSubmit']);

const { t } = useI18n();

const company = defineModel<CompanyFormData>({ default: () => ({ title: '', ic: '' }) });

const companyForm = ref();

function onSubmit() {
  if (!companyForm.value?.validate()) {
    return;
  }
  emit('onSubmit');
}

const titleRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const icRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
</script>
