<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('company.add_company') }}</template>
    <template #default>
      <CompanyForm ref="companyForm" v-model="company" @on-submit="createCompany" :loading="creatingCompany" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import CompanyForm, { CompanyFormData } from './CompanyForm.vue';
import CompanyService, { CreateCompanyParams } from '@/api/services/CompanyService';

const isDialogOpen = defineModel<boolean>();
const emit = defineEmits(['onCreate']);

const { t } = useI18n();

const company = ref<CompanyFormData>({
  title: '',
  title2: '',
  ic: '',
  dic: '',
  street: '',
  psc: '',
  city: '',
});
const companyForm = ref();

const creatingCompany = ref(false);

async function createCompany() {
  const createRequest: CreateCompanyParams = {
    title: company.value.title,
    title2: company.value.title2,
    ic: company.value.ic,
    dic: company.value.dic,
    street: company.value.street,
    psc: company.value.psc,
    city: company.value.city,
  };

  creatingCompany.value = true;
  const { data, error } = await CompanyService.createCompany(createRequest);
  creatingCompany.value = false;
/*
  if (error) {
    handleError(error, t('company.toasts.create_failed'));
    return;
  }*/

  emit('onCreate', data);
  isDialogOpen.value = false;

  toast.success(t('company.toasts.create_success'));
}
</script>

<style lang="scss" scoped></style>
