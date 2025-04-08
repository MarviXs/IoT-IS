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
      <SearchBar v-model="filter" class="col-grow col-lg-auto" />
      <q-btn
        class="shadow col-grow col-lg-auto"
        color="primary"
        unelevated
        no-caps
        size="15px"
        :label="t('account.add_document')"
        :icon="mdiPlus"
        @click="isCreateDialogOpen = true"
      />
    </div>

    <document-link-card :document-type="t('account.payroll')" :loading="loading" @onSubmit="handleDocumentLinkSubmit" />
    <document-link-card
      :document-type="t('account.price_list')"
      :loading="loading"
      @onSubmit="handleDocumentLinkSubmit"
    />
    <document-link-card
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
const filter = ref('');
const isCreateDialogOpen = ref(false);

function handleDocumentLinkSubmit(link: string) {
  loading.value = true;
  // Handle the submission logic here
  console.log('Document Link Submitted:', link);
  loading.value = false;
}
</script>
