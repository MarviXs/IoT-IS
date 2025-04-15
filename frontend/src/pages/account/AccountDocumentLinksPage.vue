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
      />

    </div>

    <document-link-card
      id="payroll"
      :document-type="t('account.payroll')"
      :loading="loading"
      @onSubmit="handleDocumentLinkSubmit"
    />

    <document-link-card
      id="price-list"
      :document-type="t('account.price_list')"
      :loading="loading"
      @onSubmit="handleDocumentLinkSubmit"
    />

    <document-link-card
      id="offer-sheet"
      :document-type="t('account.offer_sheet')"
      :loading="loading"
      @onSubmit="handleDocumentLinkSubmit"
    />

  </page-layout>
  <!-- <CreateDocumentDialog v-model="isCreateDialogOpen" /> -->
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { ref } from 'vue';
import { mdiPlus } from '@quasar/extras/mdi-v7';
import SearchBar from '@/components/core/SearchBar.vue';
import { mdiAccount, mdiFileDocument } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';
import DocumentLinkCard from '@/components/account/DocumentLinkCard.vue';
// import CreateDocumentDialog from '@/components/account/CreateDocumentDialog.vue';
//import { PaginationClient, PaginationTable } from '@/models/Pagination';

const loading = ref(false);
const { t } = useI18n();

function handleDocumentLinkSubmit(link: string) {
  loading.value = true;
  // Handle the submission logic here
  console.log('Document Link Submitted:', link);
  loading.value = false;
}

const filter = ref('');
const documentTitles = ref([
  t('account.payroll'),
  t('account.price_list'),
  t('account.offer_sheet'),
]);

function onFilter(val: string, update: (fn: () => void) => void) {
  update(() => {
    if (val === '') {
      documentTitles.value = [
        t('account.payroll'),
        t('account.price_list'),
        t('account.offer_sheet'),
      ];
    } else {
      documentTitles.value = documentTitles.value.filter(title =>
        title.toLowerCase().includes(val.toLowerCase())
      );
    }
  });
}

const documentMap: Record<string, string> = {
  [t('account.payroll')]: 'payroll',
  [t('account.price_list')]: 'price-list',
  [t('account.offer_sheet')]: 'offer-sheet'
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



</script>
