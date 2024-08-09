<template>
  <q-card class="shadow q-pa-lg">
    <q-input :model-value="currentMail" :label="t('account.current_email')" readonly disable></q-input>
    <q-form ref="qform" autocomplete="off">
      <q-input
        ref="mailRef"
        v-model="mail"
        autocomplete="off"
        :label="t('account.new_email')"
        type="email"
        :rules="mailRules"
        lazy-rules
      ></q-input>
      <q-btn
        class="float-right q-mt-lg"
        style="min-width: 95px"
        color="primary"
        unelevated
        type="submit"
        :label="t('global.save')"
        :loading="loading"
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

defineProps({
  currentMail: {
    type: String,
    required: true,
  },
  loading: {
    type: Boolean,
    required: false,
  },
});
const emit = defineEmits(['onSubmit']);
const mail = defineModel({ type: String, default: '' });

const { t } = useI18n();

const qform = ref<QForm>();

const mailRules = [
  (val: string) => (val && val.length > 0) || t('global.rules.required'),
  (val: string) => {
    const emailRegex = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
    return emailRegex.test(val) || t('account.rules.email_invalid');
  },
];

async function onSubmit() {
  const valid = qform.value?.validate();
  if (!valid) return;

  emit('onSubmit', mail.value);
}
</script>
