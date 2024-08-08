<template>
  <q-card class="shadow q-pa-lg">
    <q-form ref="qform" autocomplete="off">
      <q-input
        v-model="password"
        autocomplete="off"
        :label="t('account.new_password')"
        :type="hidePassword ? 'password' : 'text'"
        :rules="passwordRules"
      >
        <template #append>
          <q-icon
            :name="hidePassword ? mdiEyeOff : mdiEye"
            class="cursor-pointer"
            @click="hidePassword = !hidePassword"
          />
        </template>
      </q-input>
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
import { mdiEyeOff, mdiEye } from '@quasar/extras/mdi-v7';

defineProps({
  loading: {
    type: Boolean,
    required: true,
  },
});
const emit = defineEmits(['onSubmit']);
const password = ref('');

const { t } = useI18n();

const hidePassword = ref(true);

const qform = ref<QForm>();
const passwordRules = [(val: string) => (val && val.length > 0) || t('global.rules.required')];

async function onSubmit() {
  const valid = qform.value?.validate();
  if (!valid) return;

  emit('onSubmit', password);
  password.value = '';
}
</script>
