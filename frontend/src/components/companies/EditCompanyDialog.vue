<template>
  <dialog-common v-model="isDialogOpen">
    <template #title>{{ t('company.edit_company') }}</template>
    <template #default>
      <CompanyForm ref="companyForm" v-model="company" :loading="updatingCompany" @on-submit="updateCompany" />
    </template>
  </dialog-common>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { handleError } from '@/utils/error-handler';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import DialogCommon from '@/components/core/DialogCommon.vue';
import CompanyForm, { CompanyFormData } from './CompanyForm.vue';
import CompanyService, { UpdateCompanyRequest } from '@/api/services/CompanyService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  companyId: {
    type: String,
    required: true,
  },
});
const emit = defineEmits(['onUpdate']);

const { t } = useI18n();

const company = ref<CompanyFormData>({
  title: '',
  ic: '',
  dic: undefined,
  street: undefined,
  psc: undefined,
  city: undefined,
});
const companyForm = ref();

async function getCompany() {
  const { data, error } = await CompanyService.getCompany(props.companyId);
  if (error) {
    handleError(error, t('company.toasts.load_failed'));
    return;
  }

  company.value = {
    title: data.title,
    ic: data.ic,
    dic: data.dic,
    street: data.street,
    psc: data.psc,
    city: data.city,
  };
}

const updatingCompany = ref(false);

async function updateCompany() {
  const updateRequest: UpdateCompanyRequest = {
    title: company.value.title,
    ic: company.value.ic,
    dic: company.value.dic,
    street: company.value.street,
    psc: company.value.psc,
    city: company.value.city,
  };

  updatingCompany.value = true;
  const { data, error } = await CompanyService.updateCompany(props.companyId, updateRequest);
  updatingCompany.value = false;

  if (error) {
    handleError(error, t('company.toasts.update_failed'));
    return;
  }

  emit('onUpdate', data);
  isDialogOpen.value = false;
  toast.success(t('company.toasts.update_success'));
}

watch(
  () => isDialogOpen.value,
  (isOpen) => {
    if (isOpen) {
      company.value = {} as CompanyFormData;
      getCompany();
    }
  },
  { immediate: true },
);
</script>
