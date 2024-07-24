<template>
  <div>
    <div v-if="authStore.user" class="column">
      <div class="row q-mb-xl">
        <div class="col-12 col-md-5 q-mb-md">
          <div class="text-h6 text-secondary">
            {{ t('account.update_email') }}
          </div>
          <div class="text-grey-14">
            {{ t('account.update_email_desc') }}
          </div>
        </div>
        <UpdateEmailCard :user="authStore.user" class="col-12 col-md-7" />
      </div>
      <div class="row justify-between q-mb-xl">
        <div class="col-12 col-md-5 q-mb-md">
          <div class="text-h6 text-secondary">
            {{ t('account.update_password') }}
          </div>
          <div class="text-grey-14">
            {{ t('account.update_password_desc') }}
          </div>
        </div>
        <UpdatePasswordCard :user="authStore.user" class="col-12 col-md-7" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UpdateEmailCard from '@/components/account/UpdateEmailCard.vue';
import UpdatePasswordCard from '@/components/account/UpdatePasswordCard.vue';
import { useAuthStore } from '@/stores/auth-store';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const authStore = useAuthStore();
</script>
