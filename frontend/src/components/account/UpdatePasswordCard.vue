<template>
  <q-card class="shadow q-pa-lg">
    <q-form autocomplete="off">
      <q-input
        v-if="requireOldPassword"
        ref="currentPwRef"
        v-model="oldPassword"
        autocomplete="off"
        :label="t('account.current_password')"
        :type="hidePwCurrent ? 'password' : 'text'"
        lazy-rules
        :rules="currentPasswordRules"
      >
        <template #append>
          <q-icon
            :name="hidePwCurrent ? mdiEyeOff : mdiEye"
            class="cursor-pointer"
            @click="hidePwCurrent = !hidePwCurrent"
          />
        </template>
      </q-input>
      <q-input
        ref="newPwRef"
        v-model="newPassword"
        autocomplete="off"
        :label="t('account.new_password')"
        :type="hidePwNew ? 'password' : 'text'"
        :rules="newPasswordRules"
      >
        <template #append>
          <q-icon :name="hidePwNew ? mdiEyeOff : mdiEye" class="cursor-pointer" @click="hidePwNew = !hidePwNew" />
        </template>
      </q-input>
      <q-btn
        class="float-right q-mt-lg"
        style="min-width: 95px"
        color="primary"
        unelevated
        type="submit"
        :label="t('global.save')"
        :loading="changingPassword"
        no-caps
        @click.prevent="updatePassword"
      ></q-btn>
    </q-form>
  </q-card>
</template>

<script setup lang="ts">
import { PropType, ref } from 'vue';
import { handleError } from '@/utils/error-handler';
import AuthService from '@/services/AuthService';
import { User, UserUpdate } from '@/models/User';
import { toast } from 'vue3-toastify';
import { QInput } from 'quasar';
import { isFormValid } from '@/utils/form-validation';
import { useI18n } from 'vue-i18n';
import { mdiEyeOff, mdiEye } from '@quasar/extras/mdi-v6';

const props = defineProps({
  user: {
    type: Object as PropType<User>,
    required: true,
  },
  requireOldPassword: {
    type: Boolean,
    default: true,
  },
});
const emit = defineEmits(['update']);

const { t } = useI18n();

const currentPwRef = ref<QInput>();
const newPwRef = ref<QInput>();

const hidePwCurrent = ref(true);
const hidePwNew = ref(true);

const oldPassword = ref('');
const newPassword = ref('');

const changingPassword = ref(false);

const currentPasswordRules = [
  (val: string) => (val && val.length > 0) || t('global.rules.required'),
  (val: string) => val === props.user?.password || t('account.rules.password_current_wrong'),
];

const newPasswordRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

async function updatePassword() {
  const form = [newPwRef.value];
  if (props.requireOldPassword) {
    form.push(currentPwRef.value);
  }

  if (!isFormValid(form)) {
    return;
  }

  const updateUser: UserUpdate = {
    password: newPassword.value,
  };

  try {
    changingPassword.value = true;
    await AuthService.updateUser(updateUser, props.user.uid);
    toast.success(t('account.toasts.password_change_success'));
    emit('update');
  } catch (err) {
    handleError(err, t('account.toasts.password_change_failed'));
  } finally {
    changingPassword.value = false;
  }
}
</script>
