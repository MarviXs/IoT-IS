<template>
  <PageLayout
    v-if="user.data && user.data.mail"
    :title="user.data.mail"
    :previous-title="t('global.user_management')"
    previous-route="/user-management"
  >
    <div class="column q-mt-lg">
      <div class="row q-mb-xl">
        <div class="col-12 col-md-5 q-mb-md">
          <div class="text-h6 text-secondary">
            {{ t('account.change_role') }}
          </div>
          <div class="text-grey-14">
            {{ t('account.change_role_desc') }}
          </div>
        </div>
        <UpdateRoleCard :user="user.data" class="col-12 col-md-7" @update="user.refresh" />
      </div>
      <div class="row q-mb-xl">
        <div class="col-12 col-md-5 q-mb-md">
          <div class="text-h6 text-secondary">
            {{ t('account.update_email') }}
          </div>
          <div class="text-grey-14">
            {{ t('account.update_email_other_desc') }}
          </div>
        </div>
        <UpdateEmailCard :user="user.data" class="col-12 col-md-7" @update="user.refresh" />
      </div>
      <div class="row q-mb-xl">
        <div class="col-12 col-md-5 q-mb-md">
          <div class="text-h6 text-secondary">
            {{ t('account.update_password') }}
          </div>
          <div class="text-grey-14">
            {{ t('account.update_password_other_desc') }}
          </div>
        </div>
        <UpdatePasswordCard
          :user="user.data"
          :require-old-password="false"
          class="col-12 col-md-7"
          @update="user.refresh"
        />
      </div>
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import UpdateEmailCard from '@/components/account/UpdateEmailCard.vue';
import UpdatePasswordCard from '@/components/account/UpdatePasswordCard.vue';
import UpdateRoleCard from '@/components/account/UpdateRoleCard.vue';
import { useAsyncData } from '@/composables/useAsyncData';
import PageLayout from '@/layouts/PageLayout.vue';
import AuthService from '@/services/AuthService';
import { reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';

const { t } = useI18n();
const route = useRoute();

const user = reactive(
  useAsyncData(() => AuthService.getUserById(route.params.id.toString()), t('account.toasts.get_user_failed')),
);
</script>

<style lang="scss" scoped></style>
