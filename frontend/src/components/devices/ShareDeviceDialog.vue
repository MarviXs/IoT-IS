<template>
  <q-dialog v-model="isDialogOpen">
    <q-card style="max-width: 350px" class="q-pa-xs full-width">
      <q-card-section>
        <div class="text-h6">{{ t('device.share_device') }}</div>
      </q-card-section>
      <q-card-section class="q-pt-none column q-gutter-md">
        <q-input v-model="emailToShare" autofocus :label="t('account.email')" />
        <q-btn
          unelevated
          color="primary"
          :label="t('device.share_device')"
          no-caps
          :loading="shareInProgress"
          @click="shareDevice"
        />
        <div>
          <div class="text-shared q-my-md">{{ t('device.shared_with') }}</div>
          <q-list bordered separator>
            <q-item>
              <div class="row justify-center items-center">
                <div>{{ device.owner.email }}</div>
                <!-- eslint-disable-next-line @intlify/vue-i18n/no-raw-text -->
                <div>&nbsp; ({{ t('global.owner') }})</div>
              </div>
            </q-item>
            <q-item v-for="user in sharedWithUsers" :key="user.id">
              <q-item-section>{{ user.email }}</q-item-section>
              <q-item-section side>
                <q-btn round flat dense :icon="mdiClose" @click="removeSharedUser(user)" />
              </q-item-section>
            </q-item>
          </q-list>
        </div>
      </q-card-section>
      <q-card-actions align="right" class="text-primary">
        <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { PropType, ref } from 'vue';
import { handleError } from '@/utils/error-handler';
import { computed } from 'vue';
import { toast } from 'vue3-toastify';
import DeviceService from '@/api/services/DeviceService';
import { User } from '@/models/User';
import { useI18n } from 'vue-i18n';
import { mdiClose } from '@quasar/extras/mdi-v6';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  device: {
    type: Object as PropType<Device>,
    required: true,
  },
});

const { t } = useI18n();

const emailToShare = ref('');
const sharedWithUsers = ref<User[]>([]);
const shareInProgress = ref(false);
async function shareDevice() {
  if (isAlreadySharedWithUser.value) {
    toast.error(t('device.toasts.share.already_shared'));
    return;
  }
  try {
    shareInProgress.value = true;
    await DeviceService.shareDevice(props.device.id, emailToShare.value);
    getSharedUsers();
    emailToShare.value = '';
    toast.success(t('device.toasts.share.share_success'));
  } catch (error) {
    handleError(error, t('device.toasts.share.share_failed'));
  } finally {
    shareInProgress.value = false;
  }
}

async function removeSharedUser(user: User) {
  try {
    await DeviceService.removeSharedUser(props.device.id, user.id);
    sharedWithUsers.value = sharedWithUsers.value.filter((u) => u.id !== user.id);
    getSharedUsers();
    toast.success(t('device.toasts.share.user_remove_success'));
  } catch (error) {
    handleError(error, t('device.toasts.share.user_remove_failed'));
  }
}

async function getSharedUsers() {
  try {
    sharedWithUsers.value = await DeviceService.getSharedUsers(props.device.id);
  } catch (error) {
    handleError(error, t('device.toasts.share.shared_users_load_failed'));
  }
}
getSharedUsers();

const isAlreadySharedWithUser = computed(() => {
  if (!emailToShare.value) return false;
  if (props.device.owner.email === emailToShare.value) return true;
  return sharedWithUsers.value.some((u) => u.email === emailToShare.value);
});
</script>

<style lang="scss" scoped>
.text-shared {
  font-size: 1rem;
  font-weight: 500;
}
</style>
