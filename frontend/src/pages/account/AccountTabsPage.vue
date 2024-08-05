<template>
  <page-layout :breadcrumbs="[{ label: t('account.account_settings') }]">
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
      </q-tabs>
    </q-card>
    <router-view />
  </page-layout>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { mdiAccount } from '@quasar/extras/mdi-v7';
import PageLayout from '@/layouts/PageLayout.vue';

const { t } = useI18n();
</script>
<style lang="scss" scoped></style>
