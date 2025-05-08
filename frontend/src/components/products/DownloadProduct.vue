<template>
    <dialog-common v-model="isDialogOpen">
      <template #title>{{ t('product.label_data') }}</template>
      <template #default>
        <div v-if="loading">
          {{ t('common.loading') }}
        </div>
        <div v-else-if="product">
          <p><strong>{{ t('product.latin_name') }}:</strong> {{ product.latinName }}</p>
          <p><strong>{{ t('product.czech_name') }}:</strong> {{ product.czechName }}</p>
          <p><strong>{{ t('product.ean_code') }}:</strong> {{ product.eanCode }}</p>
          <p><strong>{{ t('product.plu_code') }}:</strong> {{ product.pluCode }}</p>
          <p><strong>{{ t('product.variety') }}:</strong> {{ product.variety }}</p>
        </div>
        <div v-else>
          {{ t('product.label_data_not_found') }}
        </div>
      </template>
    </dialog-common>
  </template>
  
  <script setup lang="ts">
  import { ref, watch } from 'vue';
  import { useI18n } from 'vue-i18n';
  import { handleError } from '@/utils/error-handler';
  import DialogCommon from '@/components/core/DialogCommon.vue';
  import ProductService from '@/api/services/ProductService';
  
  const { t } = useI18n();
  
  const isDialogOpen = defineModel<boolean>();
  
  const props = defineProps({
    productId: {
      type: String,
      required: true
    }
  });
  
  const product = ref<null | {
    latinName: string;
    czechName: string;
    eanCode: string;
    pluCode: string;
    variety: string;
  }>(null);
  
  const loading = ref(false);
  
  async function loadProductData() {
    loading.value = true;
    const { data, error } = await ProductService.getProduct(props.productId);
    loading.value = false;
  
    if (error) {
      handleError(error, t('command.toasts.load_failed'));
      return;
    }
  
    product.value = {
      latinName: data.latinName,
      czechName: data.czechName,
      eanCode: data.eanCode,
      pluCode: data.pluCode,
      variety: data.variety
    };
  }
  
  watch(
    () => isDialogOpen.value,
    (isOpen) => {
      if (isOpen) {
        loadProductData();
      } else {
        product.value = null;
      }
    }
  );
  </script>
  
  <style scoped lang="scss">
  p {
    margin: 0.5rem 0;
  }
  </style>
  