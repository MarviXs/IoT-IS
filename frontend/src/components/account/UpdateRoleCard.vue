<template>
  <q-card class="shadow q-pa-lg">
    <q-form ref="qform" autocomplete="off">
      <q-select
        v-model="newRole"
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
        :loading="changingRole"
        no-caps
        @click.prevent="updateRole"
      ></q-btn>
    </q-form>
  </q-card>
</template>

<script setup lang="ts">
import { PropType, computed, ref } from 'vue';
import { handleError } from '@/utils/error-handler';
import AuthService from '@/services/AuthService';
import { User } from '@/models/User';
import { toast } from 'vue3-toastify';
import { QForm, QInput } from 'quasar';
import { isFormValid } from '@/utils/form-validation';
import { Role } from '@/models/Role';
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
const changingRole = ref(false);

const availableRoles = [
  { label: t('account.role.admin'), value: Role.ADMIN },
  { label: t('account.role.user'), value: Role.USER },
];

const currentRole = computed(() => {
  return props.user.authorities?.at(0)?.authority;
});
const newRole = ref<Role>(currentRole.value ?? Role.USER);

async function updateRole() {
  if (!isFormValid([mailRef.value])) {
    return;
  }

  try {
    changingRole.value = true;
    await AuthService.setRole(newRole.value, props.user.uid);
    toast.success(t('account.toasts.role_change_success'));
    qform.value?.reset();
    emit('update');
  } catch (err) {
    handleError(err, t('account.toasts.role_change_failed'));
  } finally {
    changingRole.value = false;
  }
}
</script>
