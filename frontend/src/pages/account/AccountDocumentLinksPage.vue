<template>
  <page-layout :breadcrumbs="[{ label: t('account.document_links') }]">
    <q-card class="shadow q-mb-lg">
      <q-tabs dense class="text-grey" active-color="primary" indicator-color="primary" align="left">
        <q-route-tab
          :to="{ path: '/account' }"
          style="min-width: 130px"
          name="account"
          :icon="mdiAccount"
          :label="t('account.label')"
          no-caps
        />
        <q-route-tab
          :to="{ path: '/account/document-links' }"
          style="min-width: 130px"
          name="document-links"
          :icon="mdiFileDocument"
          :label="t('account.document_links')"
          no-caps
        />
      </q-tabs>
    </q-card>

    <div class="q-mb-md row items-center q-gutter-sm justify-end">
      <q-select
        filled
        use-input
        input-debounce="0"
        v-model="filter"
        :options="documentTitles"
        class="col-grow col-lg-auto"
        :label="t('account.search_document')"
        @filter="onFilter"
        @update:model-value="scrollToCard"
        :behavior="'menu'"
        use-chips
        new-value-mode="add"
      >
      </q-select>
    </div>

    <document-link-card
      id="quotation-sheet"
      :document-header="t('account.quotation_sheet')"
      :loading="loading"
      :document-type="EDocumentIdentifier.QuotationSheet"
      :file-name="
        userTemplates.find((template) => template.identifier == EDocumentIdentifier[EDocumentIdentifier.QuotationSheet])
          ?.fileName || ''
      "
      @onSubmit="handleDocumentLinkSubmit"
    />

    <document-link-card
      id="price-list"
      :document-header="t('account.invoice')"
      :loading="loading"
      :document-type="EDocumentIdentifier.Invoice"
      :file-name="
        userTemplates.find((template) => template.identifier == EDocumentIdentifier[EDocumentIdentifier.Invoice])
          ?.fileName || ''
      "
      @onSubmit="handleDocumentLinkSubmit"
    />

    <document-link-card
      id="offer-sheet"
      :document-header="t('account.order')"
      :loading="loading"
      :document-type="EDocumentIdentifier.Order"
      :file-name="
        userTemplates.find((template) => template.identifier == EDocumentIdentifier[EDocumentIdentifier.Order])
          ?.fileName || ''
      "
      @onSubmit="handleDocumentLinkSubmit"
    />

    <document-link-card
      id="plant-passport"
      :document-header="t('account.passports')"
      :loading="loading"
      :document-type="EDocumentIdentifier.PlantPassport"
      :file-name="
        userTemplates.find((template) => template.identifier == EDocumentIdentifier[EDocumentIdentifier.PlantPassport])
          ?.fileName || ''
      "
      @onSubmit="handleDocumentLinkSubmit"
    />

    <document-link-card
      id="product-sticker"
      :document-header="t('account.product_sticker')"
      :loading="loading"
      :document-type="EDocumentIdentifier.ProductSticker"
      :file-name="
        userTemplates.find((template) => template.identifier == EDocumentIdentifier[EDocumentIdentifier.ProductSticker])
          ?.fileName || ''
      "
      @onSubmit="handleDocumentLinkSubmit"
    />
  </page-layout>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { ref, onMounted } from 'vue';
import PageLayout from '@/layouts/PageLayout.vue';
import DocumentLinkCard from '@/components/account/DocumentLinkCard.vue';
import TemplatesService from '@/api/services/TemplatesService';
import { toast } from 'vue3-toastify';
import { EDocumentIdentifier } from '@/api/types/EDocumentIdentifier';

const loading = ref(false);
const { t } = useI18n();

function handleDocumentLinkSubmit(link: string) {
  loading.value = true;
  // Handle the submission logic here
  console.log('Document Link Submitted:', link);
  loading.value = false;
}

const filter = ref(null);
const documentTitles = ref([t('account.payroll'), t('account.price_list'), t('account.offer_sheet')]);

function onFilter(val: string, update: (fn: () => void) => void) {
  update(() => {
    if (val === '') {
      documentTitles.value = [t('account.payroll'), t('account.price_list'), t('account.offer_sheet')];
    } else {
      documentTitles.value = documentTitles.value.filter((title) => title.toLowerCase().includes(val.toLowerCase()));
    }
  });
}

const documentMap: Record<string, string> = {
  [t('account.payroll')]: 'payroll',
  [t('account.price_list')]: 'price-list',
  [t('account.offer_sheet')]: 'offer-sheet',
};

function scrollToCard(value: string) {
  const elementId = documentMap[value];
  if (elementId) {
    const el = document.getElementById(elementId);
    if (el) {
      el.scrollIntoView({ behavior: 'smooth' });
    }
  }
}

onMounted(() => {
  loadData();
});

const userTemplates = ref([]);

function loadData() {
  TemplatesService.getTemplates()
    .then((resp) => {
      userTemplates.value = resp.data;
    })
    .catch(() => {
      toast.error("Couldn't upload document");
    });
}
</script>
