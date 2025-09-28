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
        <UpdateEmailCard
          :current-mail="authStore.user.email"
          :loading="updatingEmail"
          class="col-12 col-md-7"
          @on-submit="updateEmail"
        />
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
        <UpdatePasswordCard
          v-model:old-password="oldPassword"
          v-model:new-password="newPassword"
          class="col-12 col-md-7"
          :user="authStore.user"
          :loading="isUpdatingPassword"
          @on-submit="updatePassword"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import AuthService from '@/api/services/AuthService';
import UpdateEmailCard from '@/components/account/UpdateEmailCard.vue';
import UpdatePasswordCard from '@/components/account/UpdatePasswordCard.vue';
import { useAuthStore } from '@/stores/auth-store';
import { handleError } from '@/utils/error-handler';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { toast } from 'vue3-toastify';

const { t } = useI18n();
const authStore = useAuthStore();

const newPassword = ref('');
const oldPassword = ref('');
const isUpdatingPassword = ref(false);

async function updatePassword(oldPw: string, newPw: string) {
  isUpdatingPassword.value = true;
  const { error } = await AuthService.updatePassword(oldPw, newPw);
  isUpdatingPassword.value = false;

  if (error) {
    handleError(error, "Couldn't update password");
    return;
  }
  newPassword.value = '';
  oldPassword.value = '';

  toast.success('Password updated successfully');
}

const updatingEmail = ref(false);
async function updateEmail(email: string) {
  updatingEmail.value = true;
  const { error } = await AuthService.updateEmail(email);
  updatingEmail.value = false;

  if (error) {
    handleError(error, t('account.toasts.email_update_failed'));
    return;
  }

  if (authStore.user?.email) {
    authStore.user.email = email;
  }
  authStore.refreshAccessToken();

  toast.success(t('account.toasts.email_update_success'));
}
</script>
