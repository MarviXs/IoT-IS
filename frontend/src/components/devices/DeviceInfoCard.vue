<template>
  <div v-if="props.device">
    <q-expansion-item switch-toggle-side>
      <template #header>
        <div class="text-weight-medium text-subtitle1 flex items-center">{{ t('device.device_info') }}</div>
      </template>

      <div class="q-mt-sm q-gutter-y-sm column justify-between no-wrap q-px-lg q-pb-lg">
        <div class="row">
          <div class="col-4 text-grey-color">{{ t('global.name') }}</div>
          <div class="col-8">{{ props.device.name }}</div>
        </div>
        <div class="row">
          <div class="col-4 text-grey-color">{{ t('device_template.label') }}</div>
          <div class="col-8">
            <router-link v-if="props.device.deviceTemplate" :to="`/device-templates/${props.device.deviceTemplate.id}`">
              {{ props.device.deviceTemplate.name }}
            </router-link>
            <!-- eslint-disable-next-line @intlify/vue-i18n/no-raw-text -->
            <span v-else>-</span>
          </div>
        </div>
        <!-- <div class="row">
          <div class="col-4 text-grey-color">{{ t('device.mac_address') }}</div>
          <div class="col-8">{{ props.device.mac ?? '-' }}</div>
        </div> -->
        <div class="row items-center q-mt-none">
          <div class="col-4 text-grey-color">{{ t('device.access_token') }}</div>
          <div class="col-8 row items-center">
            <div class="token-container">{{ isTokenVisible ? props.device.accessToken : maskedToken }}</div>
            <q-btn
              v-if="props.device.accessToken"
              color="secondary"
              class="q-ml-xs"
              flat
              round
              dense
              :icon="mdiEye"
              @click="toggleTokenVisibility"
            />
          </div>
        </div>
      </div>
    </q-expansion-item>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { PropType } from 'vue';
import { useI18n } from 'vue-i18n';
import { DeviceResponse } from '@/api/types/Device';
import { mdiEye } from '@quasar/extras/mdi-v7';

const props = defineProps({
  device: {
    type: Object as PropType<DeviceResponse>,
    required: true,
  },
});

const { t } = useI18n();

const isTokenVisible = ref(false);

const toggleTokenVisibility = () => {
  isTokenVisible.value = !isTokenVisible.value;
};

const maskedToken = computed(() => {
  return props.device.accessToken ? '●'.repeat(props.device.accessToken.length) : '-';
});
</script>

<style lang="scss" scoped>
.token-container {
  display: inline-block;
  font-family: monospace;
  white-space: nowrap;
}
</style>
