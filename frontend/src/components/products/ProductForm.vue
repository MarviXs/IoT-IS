<template>
  <q-form @submit="onSubmit" ref="productForm">
    <q-card-section class="q-pt-none column q-gutter-md">
      <q-input v-model="product.name" :rules="nameRules" autofocus :label="t('global.name')" />
      <device-template-select v-model="product.deviceTemplate" />
      <q-input v-model="product.accessToken" class="q-mt-lg q-mb-sm" :label="t('device.access_token')">
        <template #append>
          <q-icon v-if="!product.accessToken" :name="mdiAutorenew" class="cursor-pointer" @click="generateAccessToken">
            <q-tooltip>{{ t('device.generate_access_token') }}</q-tooltip>
          </q-icon>
          <q-icon v-else :name="mdiContentCopy" class="cursor-pointer" @click="copyAccessToken">
            <q-tooltip>{{ copied ? t('device.access_token_copied') : t('device.copy_access_token') }}</q-tooltip>
          </q-icon>
        </template>
      </q-input>
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
import DeviceTemplateSelect from '@/components/device-templates/DeviceTemplateSelect.vue';
import { mdiAutorenew, mdiContentCopy } from '@quasar/extras/mdi-v7';
import { copyToClipboard } from 'quasar';
import { ProductCategorySelectData } from './ProductCategorySelect.vue';

export interface ProductFormData {
  pluCode: string;
  code: string;
  latinName: string;
  czechName: string;
  flowerLeafDescription: string;
  potDiameterPack: string;
  pricePerPiecePack: number;
  pricePerPiecePackVAT: number;
  discountedPriceWithoutVAT: number;
  retailPrice: number;
  productCategory: ProductCategorySelectData;
}

const props = defineProps<{
  loading?: boolean;
}>();

const emit = defineEmits(['onSubmit']);

const { t } = useI18n();

const product = defineModel<ProductFormData>({ required: true });
const nameRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

const deviceForm = ref();

function onSubmit() {
  if (!deviceForm.value?.validate()) {
    return;
  }
  emit('onSubmit');
}
</script>
