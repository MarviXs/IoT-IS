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
          <q-list v-if="sharedUsers.length > 0" bordered separator>
            <q-item v-for="user in sharedUsers" :key="user.email">
              <q-item-section>{{ user.email }} ({{ user.permission }})</q-item-section>
              <q-item-section side>
                <q-btn round flat dense :icon="mdiClose" @click="removeSharedUser(user.email)" />
              </q-item-section>
            </q-item>
          </q-list>
          <div v-else class="text-caption">{{ t('device.no_shared_users') }}</div>
        </div>
      </q-card-section>
      <q-card-actions align="right" class="text-primary">
        <q-btn v-close-popup flat :label="t('global.cancel')" no-caps />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { handleError } from '@/utils/error-handler';
import { computed } from 'vue';
import { toast } from 'vue3-toastify';
import { useI18n } from 'vue-i18n';
import { mdiClose } from '@quasar/extras/mdi-v7';
import DeviceSharingService, { SharedUsers } from '@/api/services/DeviceSharingService';

const isDialogOpen = defineModel<boolean>();
const props = defineProps({
  deviceId: {
    type: String,
    required: true,
  },
});

const { t } = useI18n();

const emailToShare = ref('');

const shareInProgress = ref(false);
async function shareDevice() {
  shareInProgress.value = true;
  const { data, error } = await DeviceSharingService.shareDevice(
    { email: emailToShare.value, permission: 'Editor' },
    props.deviceId,
  );
  shareInProgress.value = false;

  if (error) {
    handleError(error, "Couldn't share device");
    return;
  }

  toast.success(t('device.toasts.share.share_success'));
  emailToShare.value = '';

  getSharedUsers();
}

async function removeSharedUser(email: string) {
  const { error } = await DeviceSharingService.unshareDevice({ email }, props.deviceId);
  if (error) {
    handleError(error, "Couldn't remove shared user");
    return;
  }
  getSharedUsers();
}

const sharedUsers = ref<SharedUsers>([]);

async function getSharedUsers() {
  const { data, error } = await DeviceSharingService.getSharedUsers(props.deviceId);
  if (error) {
    handleError(error, "Couldn't get shared users");
    return;
  }
  sharedUsers.value = data;
}
getSharedUsers();
</script>

<style lang="scss" scoped>
.text-shared {
  font-size: 1rem;
  font-weight: 500;
}
</style>
