<template>
  <PageLayout :breadcrumbs="[{ to: '/user-management', label: t('global.user_management') }, { label: user?.email }]">
    <div v-if="user" class="column q-mt-lg">
      <div class="row q-mb-xl">
        <div class="col-12 col-md-5 q-mb-md">
          <div class="text-h6 text-secondary">
            {{ t('account.change_role') }}
          </div>
          <div class="text-grey-14">
            {{ t('account.change_role_desc') }}
          </div>
        </div>
        <UpdateRoleCard
          v-model="selectedRole"
          class="col-12 col-md-7"
          :loading="updatingRole"
          @on-submit="updateRole"
        />
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
        <UpdateEmailCard
          v-model="newEmail"
          :current-mail="user.email"
          class="col-12 col-md-7"
          :loading="updatingEmail"
          @on-submit="updateEmail"
        />
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
        <UpdateUserPasswordCard class="col-12 col-md-7" :loading="updatingPassword" @on-submit="updatePassword" />
      </div>
    </div>
  </PageLayout>
</template>

<script setup lang="ts">
import UpdateEmailCard from '@/components/account/UpdateEmailCard.vue';
import UpdateRoleCard from '@/components/account/UpdateRoleCard.vue';
import UpdateUserPasswordCard from '@/components/account/UpdateUserPasswordCard.vue';
import PageLayout from '@/layouts/PageLayout.vue';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import UserManagementService from '@/api/services/UserManagementService';
import { ref } from 'vue';
import type { GetUserByIdResponse } from '@/api/services/UserManagementService';
import { handleError } from '@/utils/error-handler';
import { Role } from '@/models/Role';
import { toast } from 'vue3-toastify';

const { t } = useI18n();
const route = useRoute();
const userId = route.params.id as string;

const user = ref<GetUserByIdResponse>();
const isLoadingUser = ref(false);

async function getUser() {
  isLoadingUser.value = true;
  const { data, error } = await UserManagementService.getUserById(userId);
  isLoadingUser.value = false;

  if (error) {
    handleError(error, t('account.toasts.get_user_failed'));
    return;
  }

  user.value = data;
  selectedRole.value = (data.roles[0] as Role) ?? Role.USER;
}
getUser();

const selectedRole = ref<Role>();
const updatingRole = ref(false);
async function updateRole(role: Role) {
  updatingRole.value = true;
  const { error } = await UserManagementService.updateUserRole(userId, role);
  updatingRole.value = false;

  if (error) {
    handleError(error, t('account.toasts.role_change_failed'));
    return;
  }

  toast.success(t('account.toasts.role_change_success'));
}

const newEmail = ref('');
const updatingEmail = ref(false);
async function updateEmail(email: string) {
  updatingEmail.value = true;
  const { error } = await UserManagementService.updateUserEmail(userId, email);
  updatingEmail.value = false;

  if (error) {
    handleError(error, t('account.toasts.email_update_failed'));
    return;
  }

  user.value.email = email;
  toast.success(t('account.toasts.email_update_success'));
}

const updatingPassword = ref(false);
async function updatePassword(password: string) {
  updatingPassword.value = true;
  const { error } = await UserManagementService.updateUserPassword(userId, password);
  updatingPassword.value = false;

  if (error) {
    handleError(error, t('account.toasts.password_change_failed'));
    return;
  }

  toast.success(t('account.toasts.password_change_success'));
}
</script>

<style lang="scss" scoped></style>
