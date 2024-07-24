<template>
  <q-card class="shadow q-pa-lg">
    <q-input v-model="currentMail" :label="t('account.current_email')" readonly disable></q-input>
    <q-form ref="qform" autocomplete="off">
      <q-input
        ref="mailRef"
        v-model="newEmail"
        autocomplete="off"
        :label="t('account.new_email')"
        type="email"
        :rules="mailRules"
      ></q-input>
      <q-btn
        class="float-right q-mt-lg"
        style="min-width: 95px"
        color="primary"
        unelevated
        type="submit"
        :label="t('global.save')"
        :loading="changingEmail"
        no-caps
        @click.prevent="updateEmail"
      ></q-btn>
    </q-form>
  </q-card>
</template>

<script setup lang="ts">
import { PropType, ref, watch } from 'vue';
import { handleError } from '@/utils/error-handler';
import AuthService from '@/services/AuthService';
import { User, UserUpdate } from '@/models/User';
import { toast } from 'vue3-toastify';
import { QForm, QInput } from 'quasar';
import { isFormValid } from '@/utils/form-validation';
import { useI18n } from 'vue-i18n';

const props = defineProps({
  user: {
    type: Object as PropType<User>,
    required: true,
  },
});
const emit = defineEmits(['update']);

const { t } = useI18n();

const mailRef = ref<QInput>();
const qform = ref<QForm>();
const currentMail = ref(props.user.mail);
const newEmail = ref('');
const changingEmail = ref(false);

const mailRules = [
  (val: string) => (val && val.length > 0) || t('global.rules.required'),
  (val: string) => {
    const emailRegex = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
    return emailRegex.test(val) || t('account.rules.email_invalid');
  },
];

async function updateEmail() {
  if (!isFormValid([mailRef.value])) {
    return;
  }

  const updateUser: UserUpdate = {
    mail: newEmail.value,
  };

  try {
    changingEmail.value = true;
    await AuthService.updateUser(updateUser, props.user.uid);
    toast.success(t('account.toasts.email_update_success'));
    newEmail.value = '';
    qform.value?.reset();
    emit('update');
  } catch (err) {
    handleError(err, t('account.toasts.email_update_failed'));
  } finally {
    changingEmail.value = false;
  }
}

watch(
  () => props.user,
  () => (currentMail.value = props.user.mail),
  { immediate: true },
);
</script>
