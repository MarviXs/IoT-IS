<template>
  <q-card class="shadow q-pa-lg">
    <q-form ref="qform" autocomplete="off">
      <q-select
        v-model="selectedRole"
        autocomplete="off"
        :label="t('account.role.label')"
        :options="availableRoles"
        map-options
        emit-value
      ></q-select>
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
import { QForm } from 'quasar';
import { Role } from '@/models/Role';
import { useI18n } from 'vue-i18n';

defineProps({
  loading: {
    type: Boolean,
    default: false,
  },
});
const emit = defineEmits(['onSubmit']);

const { t } = useI18n();

const selectedRole = defineModel<Role>();
const qform = ref<QForm>();

async function onSubmit() {
  const valid = qform.value?.validate();
  if (!valid) return;

  emit('onSubmit', selectedRole.value);
}

const availableRoles = [
  { label: t('account.role.admin'), value: Role.ADMIN },
  { label: t('account.role.user'), value: Role.USER },
];
</script>
