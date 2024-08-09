<template>
  <q-card class="shadow q-pa-lg">
    <q-form ref="qform" autocomplete="off">
      <q-input
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
        @click.prevent="onSubmit"
      ></q-btn>
    </q-form>
  </q-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { QForm, QInput } from 'quasar';
import { useI18n } from 'vue-i18n';
import { mdiEyeOff, mdiEye } from '@quasar/extras/mdi-v7';

defineProps({
  loading: {
    type: Boolean,
    default: false,
  },
});
const emit = defineEmits(['onSubmit']);

const oldPassword = defineModel<string>('oldPassword');
const newPassword = defineModel<string>('newPassword');

const { t } = useI18n();

const qform = ref<QForm>();

const hidePwCurrent = ref(true);
const hidePwNew = ref(true);

const changingPassword = ref(false);

const currentPasswordRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];
const newPasswordRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

async function onSubmit() {
  const valid = qform.value?.validate();
  if (!valid) return;

  emit('onSubmit', oldPassword.value, newPassword.value);
}
</script>
