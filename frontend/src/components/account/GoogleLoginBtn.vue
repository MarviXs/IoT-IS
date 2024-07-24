<template>
  <GoogleLogin :callback="login" prompt />
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth-store';
import { handleError } from '@/utils/error-handler';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { toast } from 'vue3-toastify';

const { t } = useI18n();

const loading = defineModel('loading', {
  type: Boolean,
  required: true,
});

const authStore = useAuthStore();
const router = useRouter();

async function login(response: { credential: string }) {
  try {
    loading.value = true;
    await authStore.loginByGoogle(response.credential);
    toast.success(t('auth.login.toasts.login_success'));
    router.push('/');
  } catch (error) {
    handleError(error, t('auth.login.toasts.login_failed'));
  } finally {
    loading.value = false;
  }
}
</script>
